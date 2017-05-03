using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using Villermen.RuneScapeCacheTools.Cache.Files;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Exceptions;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    [Obsolete("Unfinished")]
    public class TcpFileDownloader : IFileDownloader, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TcpFileDownloader));

        /// <summary>
        /// The minor version is needed to correctly connect to the content server.
        /// This seems to always be 1.
        /// </summary>
        private const int ContentVersionMinor = 1;

        private const Language Language = Cache.Language.English;

        private const int BlockLength = 102400;

        /// <summary>
        ///  The handshake type is needed to correctly connect to the content server.
        /// </summary>
        private const byte HandshakeType  = 15;

        private const int LoadingRequirements = 27;

        /// <summary>
        /// The regex used to obtain the content server handshake key from the set <see cref="_keyPage" />.
        /// The first capture group needs to result in the key.
        /// </summary>
        private static readonly Regex KeyFilter = new Regex(@"<param\s+name=""\-?\d+""\s+value=""([^""]{32})""");

        private readonly string _contentHost;

        private readonly int _contentPort;

        private readonly string _keyPage;

        private readonly object _processorLock = new object();

        /// <summary>
        /// The major version is needed to correctly connect to the content server.
        /// If connection states the version is outdated, the <see cref="_contentVersionMajor" /> will be increased until it is accepted.
        /// </summary>
        private int _contentVersionMajor = 880;

        private TcpClient _contentClient;

        private bool _connected = false;

        private ConcurrentDictionary<Tuple<Index, int>, FileRequest> FileRequests { get; } =
            new ConcurrentDictionary<Tuple<Index, int>, FileRequest>();

        public TcpFileDownloader(string contentHost = "content.runescape.com", int contentPort = 43594, string keyPage = "http://world2.runescape.com")
        {
            this._contentHost = contentHost;
            this._contentPort = contentPort;
            this._keyPage = keyPage;
        }

        public async Task<BinaryFile> DownloadFileAsync(Index index, int fileId, CacheFileInfo fileInfo = null)
        {
            // Add the request, or get an existing one
            var request = this.FileRequests.GetOrAdd(
                new Tuple<Index, int>(index, fileId),
                new FileRequest(index, fileId, fileInfo));

            Task.Run(new Action(this.ProcessRequests));

            return await request.WaitForCompletionAsync();
        }

        private string GetKey()
        {
            var request = WebRequest.CreateHttp(this._keyPage);
            using (var response = request.GetResponse())
            {
                var responseStream = response.GetResponseStream();

                if (responseStream == null)
                {
                    throw new DownloaderException($"No handshake key could be obtained from \"{this._keyPage}\".");
                }

                var reader = new StreamReader(responseStream);
                var responseString = reader.ReadToEnd();

                var key = TcpFileDownloader.KeyFilter.Match(responseString).Groups[1].Value;

                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new DownloaderException("Obtained TCP handshake key is empty.");
                }

                return key;
            }
        }

        private void Connect()
        {
            if (this._connected)
            {
                throw new DownloaderException("Tried to connect while already connected.");
            }

            TcpFileDownloader.Logger.Debug("Connecting to content server with TCP.");

            var key = this.GetKey();

            // Retry connecting with an increasing major version until the server no longer reports we're outdated
            var connected = false;
            while (!connected)
            {
                this._contentClient = new TcpClient(this._contentHost, this._contentPort);

                var handshakeWriter = new BinaryWriter(this._contentClient.GetStream());
                var handshakeReader = new BinaryReader(this._contentClient.GetStream());

                var handshakeLength = (byte) (9 + key.Length + 1);

                handshakeWriter.Write(TcpFileDownloader.HandshakeType);
                handshakeWriter.Write(handshakeLength);
                handshakeWriter.WriteInt32BigEndian(this._contentVersionMajor);
                handshakeWriter.WriteInt32BigEndian(TcpFileDownloader.ContentVersionMinor);
                handshakeWriter.WriteNullTerminatedString(key);
                handshakeWriter.Write((byte)TcpFileDownloader.Language);
                handshakeWriter.Flush();

                var response = (HandshakeResponse)handshakeReader.ReadByte();

                switch (response)
                {
                    case HandshakeResponse.Success:
                        connected = true;
                        TcpFileDownloader.Logger.Info($"Successfully connected to content server with major version {this._contentVersionMajor}.");
                        break;

                    case HandshakeResponse.Outdated:
                        this._contentClient.Dispose();
                        this._contentClient = null;
                        TcpFileDownloader.Logger.Info($"Requested connection used outdated version {this._contentVersionMajor}. Retrying with higher major version.");
                        this._contentVersionMajor++;
                        break;

                    default:
                        this._contentClient.Dispose();
                        this._contentClient = null;
                        throw new DownloaderException($"Content server responded to handshake with {response}.");
                }
            }

            // Required loading element sizes. They are unnsed by this tool and I have no idea what they are for. So yeah...
            var contentReader = new BinaryReader(this._contentClient.GetStream());
            contentReader.ReadBytes(TcpFileDownloader.LoadingRequirements * 4);

            // Send the initial connection status and login packets to the server.
            TcpFileDownloader.Logger.Debug("Sending initial connection status and login packets.");

            var writer = new BinaryWriter(this._contentClient.GetStream());

            // I don't know what exactly, but this is how it's done
            writer.Write((byte)6);
            writer.WriteUInt24BigEndian(4);
            writer.WriteInt16BigEndian(0);
            writer.Flush();

            writer.Write((byte)3);
            writer.WriteUInt24BigEndian(0);
            writer.WriteInt16BigEndian(0);
            writer.Flush();

            this._connected = true;
        }

        private void ProcessRequests()
        {
            lock (this._processorLock)
            {
                // Check if still needed after lock is obtained
                if (!this.FileRequests.Any())
                {
                    return;
                }

                if (!this._connected)
                {
                    this.Connect();
                }

                TcpFileDownloader.Logger.Debug("Starting TCP response processor.");

                while (this.FileRequests.Any())
                {
                    // Request all unrequested files
                    foreach (var requestPair in this.FileRequests.Where(request => !request.Value.Requested))
                    {
                        this.RequestFile(requestPair.Value);

                        // TODO: Limit to x amount of pending requests
                    }

                    // Read one chunk
                    if (this._contentClient.Available >= 5)
                    {
                        var reader = new BinaryReader(this._contentClient.GetStream());

                        var readByteCount = 0;

                        // Check which file this chunk is for
                        var index = (Index) reader.ReadByte();
                        var fileId = reader.ReadInt32BigEndian() & 0x7fffffff;

                        readByteCount += 5;

                        var requestKey = new Tuple<Index, int>(index, fileId);

                        if (!this.FileRequests.ContainsKey(requestKey))
                        {
                            throw new DownloaderException("Invalid response received (maybe not all data was consumed by the previous operation?");
                        }

                        var request = this.FileRequests[requestKey];

                        // The first part of the file always contains the filesize, which we need to know, but is also part of the file
                        if (!request.MetaWritten)
                        {
                            var compressionType = (CompressionType) reader.ReadByte();
                            var length = reader.ReadInt32BigEndian();

                            readByteCount += 5;

                            request.WriteMeta(compressionType, length);
                        }

                        var remainingBlockLength = TcpFileDownloader.BlockLength - readByteCount;

                        if (remainingBlockLength > request.RemainingLength)
                        {
                            remainingBlockLength = request.RemainingLength;
                        }

                        request.WriteContent(reader.ReadBytes(remainingBlockLength));

                        if (request.Completed)
                        {
                            // The request got completed, remove it from the list of pending requests
                            FileRequest removedRequest;
                            this.FileRequests.TryRemove(requestKey, out removedRequest);
                        }
                    }
                }

                TcpFileDownloader.Logger.Debug("TCP request processor finished.");
            }
        }

        private void RequestFile(FileRequest request)
        {
            if (request.Requested)
            {
                throw new DownloaderException("File to be requested is already requested.");
            }

            TcpFileDownloader.Logger.Debug($"Requesting {(int)request.Index}/{request.FileId} using TCP.");

            var writer = new BinaryWriter(this._contentClient.GetStream());

            writer.Write((byte)(request.Index == Index.ReferenceTables ? 1 : 0));
            writer.Write((byte)request.Index);
            writer.WriteInt32BigEndian(request.FileId);

            request.Requested = true;
        }

        public void Dispose()
        {
            this._contentClient.Dispose();
        }

        private enum HandshakeResponse
        {
            Undefined = -1,

            Success = 0,
            Outdated = 6,
            InvalidKey = 48
        }

        private class FileRequest
        {
            private readonly CacheFileInfo _cacheFileInfo;
            private readonly MemoryStream _dataStream = new MemoryStream();
            private readonly TaskCompletionSource<BinaryFile> _completionSource = new TaskCompletionSource<BinaryFile>();
            private int _fileSize;

            public Index Index { get; }
            public int FileId { get; }
            public int RemainingLength => (int)(this._fileSize - this._dataStream.Length);
            public bool Completed { get; private set; }
            public bool Requested { get; set; }
            public bool MetaWritten { get; private set; }

            public FileRequest(Index index, int fileId, CacheFileInfo cacheFileInfo)
            {
                this.Index = index;
                this.FileId = fileId;
                this._cacheFileInfo = cacheFileInfo;
            }

            public void WriteMeta(CompressionType compressionType, int length)
            {
                if (this._dataStream.Length != 0)
                {
                    throw new DownloaderException("File metadata must be written before anything else.");
                }

                var writer = new BinaryWriter(this._dataStream);
                writer.Write((byte)compressionType);
                writer.WriteInt32BigEndian(length);

                this._fileSize = 5 + (compressionType != CompressionType.None ? 4 : 0) + length;

                this.MetaWritten = true;
            }

            public void WriteContent(byte[] data)
            {
                if (!this.MetaWritten)
                {
                    throw new DownloaderException("File content must be written after metadata");
                }

                if (data.Length > this.RemainingLength)
                {
                    throw new DownloaderException("Tried to write more bytes than were remaining in the file.");
                }

                this._dataStream.Write(data, 0, data.Length);

                if (this.RemainingLength == 0)
                {
                    // Append file version if possible
                    if (this._cacheFileInfo?.Version != null)
                    {
                        var writer = new BinaryWriter(this._dataStream);
                        writer.WriteUInt16BigEndian((ushort)this._cacheFileInfo.Version);
                    }

                    this.Completed = true;
                    this._completionSource.SetResult(RuneTek5FileDecoder.DecodeFile(this._dataStream.ToArray(), this._cacheFileInfo));
                }
            }

            public async Task<BinaryFile> WaitForCompletionAsync()
            {
                 return await this._completionSource.Task;
            }
        }
    }
}