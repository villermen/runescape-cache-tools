using System;
using System.Collections.Concurrent;
using System.IO;
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

        private readonly object _connectLock = new object();

        private readonly object _responseProcessorLock = new object();

        /// <summary>
        /// The major version is needed to correctly connect to the content server.
        /// If connection states the version is outdated, the <see cref="_contentVersionMajor" /> will be increased until it is accepted.
        /// </summary>
        private int _contentVersionMajor = 880;

        private TcpClient _contentClient;

        private bool _connected = false;

        private ConcurrentDictionary<Tuple<Index, int>, FileRequest> PendingFileRequests { get; } =
            new ConcurrentDictionary<Tuple<Index, int>, FileRequest>();

        public TcpFileDownloader(string contentHost = "content.runescape.com", int contentPort = 43594, string keyPage = "http://world2.runescape.com")
        {
            this._contentHost = contentHost;
            this._contentPort = contentPort;
            this._keyPage = keyPage;
        }

        public async Task<BinaryFile> DownloadFileAsync(Index index, int fileId, CacheFileInfo fileInfo = null)
        {
            throw new System.NotImplementedException();
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
            if (!this._connected)
            {
                lock (this._connectLock)
                {
                    if (this._connected)
                    {
                        // No need for this now is there?
                        return;
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

                    this.SendTcpConnectionInfo();

                    this._connected = true;
                }
            }
        }

        /// <summary>
        /// Sends the initial connection status and login packets to the server.
        /// </summary>
        private void SendTcpConnectionInfo()
        {
            TcpFileDownloader.Logger.Debug("Sending initial connection status and login packets.");

            var writer = new BinaryWriter(this._contentClient.GetStream());

            // I don't know
            writer.Write((byte)6);
            writer.WriteUInt24BigEndian(4);
            writer.WriteInt16BigEndian(0);
            writer.Flush();

            writer.Write((byte)3);
            writer.WriteUInt24BigEndian(0);
            writer.WriteInt16BigEndian(0);
            writer.Flush();
        }

        private void StartFileDownloadTcp(FileRequest fileRequest)
        {
            // TODO: This at least needs a different name, and at most a full rework

            // TODO: Wrap in processorlock in its entirety? Account for early-out late-in
            Task.Run(() =>
            {
                if (!this._connected)
                {
                    this.Connect();
                }

                TcpFileDownloader.Logger.Debug($"Requesting {fileRequest.Index}/{fileRequest.FileId} using TCP.");

                // Send the request
                var writer = new BinaryWriter(this._contentClient.GetStream());

                // Send the file request to the content server
                writer.Write((byte)(fileRequest.Index == Index.ReferenceTables ? 1 : 0));
                writer.Write((byte)fileRequest.Index);
                writer.WriteInt32BigEndian(fileRequest.FileId);

                // This will process all received TCP chunks until the given requested file is complete (so it might also complete other requested files).
                // Only one processor may be running at any given moment
                lock (this._responseProcessorLock)
                {
                    TcpFileDownloader.Logger.Debug("Starting TCP request processor.");

                    while (this.PendingFileRequests.ContainsKey(new Tuple<Index, int>(fileRequest.Index, fileRequest.FileId)))
                    {
                        // Read one chunk
                        if (this._contentClient.Available >= 5)
                        {
                            var reader = new BinaryReader(this._contentClient.GetStream());

                            var readByteCount = 0;

                            var index = (Index)reader.ReadByte();
                            var fileId = reader.ReadInt32BigEndian() & 0x7fffffff;

                            readByteCount += 5;

                            var requestKey = new Tuple<Index, int>(index, fileId);

                            if (!this.PendingFileRequests.ContainsKey(requestKey))
                            {
                                throw new DownloaderException("Invalid response received (maybe not all data was consumed by the previous operation?");
                            }

                            var request = (FileRequest)this.PendingFileRequests[requestKey];
                            var dataWriter = new BinaryWriter(request.DataStream);

                            // The first part of the file always contains the filesize, which we need to know, but is also part of the file
                            if (request.FileSize == 0)
                            {
                                var compressionType = (CompressionType)reader.ReadByte();
                                var length = reader.ReadInt32BigEndian();

                                readByteCount += 5;

                                request.FileSize = 5 + (compressionType != CompressionType.None ? 4 : 0) + length;

                                dataWriter.Write((byte)compressionType);
                                dataWriter.WriteInt32BigEndian(length);
                            }

                            var remainingBlockLength = TcpFileDownloader.BlockLength - readByteCount;

                            if (remainingBlockLength > request.RemainingLength)
                            {
                                remainingBlockLength = request.RemainingLength;
                            }

                            dataWriter.Write(reader.ReadBytes(remainingBlockLength));

                            if (request.RemainingLength == 0)
                            {
                                // The request got completed, remove it from the list of pending requests
                                FileRequest removedRequest;
                                this.PendingFileRequests.TryRemove(requestKey, out removedRequest);

                                // TODO: this.AppendVersionToRequestData(removedRequest);

                                removedRequest.Complete();

                                // Exit the loop if this was the file originally requested
                                if (removedRequest == fileRequest)
                                {
                                    break;
                                }
                            }
                        }

                        // var leftoverBytes = new BinaryReader(TcpContentClient.GetStream()).ReadBytes(TcpContentClient.Available);
                    }

                    TcpFileDownloader.Logger.Debug("TCP request processor finished.");
                }
            });
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
            protected FileRequest(Index index, int fileId, CacheFileInfo cacheFileInfo)
            {
                this.Index = index;
                this.FileId = fileId;
                this.CacheFileInfo = cacheFileInfo;
            }

            public int FileSize { get; set; }

            public int RemainingLength => (int)(this.FileSize - this.DataStream.Length);

            public CacheFileInfo CacheFileInfo { get; }
            public MemoryStream DataStream { get; } = new MemoryStream();
            public int FileId { get; }
            public Index Index { get; }
            private TaskCompletionSource<byte[]> CompletionSource { get; } = new TaskCompletionSource<byte[]>();

            public void Write(byte[] data)
            {
                if (data.Length > this.RemainingLength)
                {
                    throw new DownloaderException("Tried to write more bytes than were remaining in the file.");
                }

                this.DataStream.Write(data, 0, data.Length);

                if (this.RemainingLength == 0)
                {
                    this.Complete();
                }
            }

            public void Complete()
            {
                this.CompletionSource.SetResult(this.DataStream.ToArray());
            }

            public byte[] WaitForCompletion()
            {
                // Wait for CompletionSource with a timeout
                if (Task.WhenAny(this.CompletionSource.Task, Task.Delay(TimeSpan.FromSeconds(10))).Result == this.CompletionSource.Task)
                {
                    return this.CompletionSource.Task.Result;
                }

                throw new TimeoutException("The file request was not fulfilled within 10 seconds.");
            }
        }
    }
}