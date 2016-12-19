using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using Villermen.RuneScapeCacheTools.Cache.CacheFile;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    /// <summary>
    ///     The <see cref="CacheDownloader" /> provides the means to download current cache files from the runescape servers.
    ///     Downloading uses 2 different interfaces depending on the <see cref="Index" /> of the requested file: The original
    ///     TCP based interface, and a much simpler HTTP interface.
    ///     Properties prefixed with Tcp or Http will only be used by the specified downloading method.
    /// </summary>
    /// <author>Villermen</author>
    /// <author>Method</author>
    public class CacheDownloader : CacheBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CacheDownloader));

        static CacheDownloader()
        {
            // Set the (static) security protocol used for web requests
            // Mono does not seem to be capable of this yet: http://www.c-sharpcorner.com/news/mono-now-comes-with-support-for-tls-12
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public override IEnumerable<Index> Indexes => this.GetMasterReferenceTable().ReferenceTableFiles.Keys;

        public string ContentHost { get; set; } = "content.runescape.com";

        public IList<Index> IndexesUsingHttpInterface { get; } = new List<Index> { Index.Music };

        public Language Language { get; set; } = Language.English;

        public int TcpBlockLength { get; set; } = 102400;

        public bool TcpConnected { get; private set; }

        public int TcpContentPort { get; set; } = 43594;

        /// <summary>
        ///     The page used in obtaining the content server handshake key.
        /// </summary>
        public string TcpKeyPage { get; set; } = "http://world2.runescape.com";

        /// <summary>
        ///     The regex used to obtain the content server handshake key from the set <see cref="TcpKeyPage" />.
        ///     The first capture group needs to result in the key.
        /// </summary>
        public Regex TcpKeyPageRegex { get; set; } = new Regex(@"<param\s+name=""\-?\d+""\s+value=""([^""]{32})""");

        /// <summary>
        ///     The minor version is needed to correctly connect to the content server.
        ///     This seems to always be 1.
        /// </summary>
        public int TcpMinorVersion { get; set; } = 1;

        private MasterReferenceTable CachedMasterReferenceTable { get; set; }

        private ConcurrentDictionary<Index, ReferenceTable> CachedReferenceTables { get; } = new ConcurrentDictionary<Index, ReferenceTable>();

        private ConcurrentDictionary<Tuple<Index, int>, FileRequest> PendingFileRequests { get; } =
            new ConcurrentDictionary<Tuple<Index, int>, FileRequest>();

        private TcpClient TcpContentClient { get; set; }

        /// <summary>
        ///     The handshake type is needed to correctly connect to the content server.
        /// </summary>
        private byte TcpHandshakeType { get; } = 15;

        private int TcpLoadingRequirementsLength { get; } = 26 * 4;

        /// <summary>
        ///     The major version is needed to correctly connect to the content server.
        ///     If connection states the version is outdated, the <see cref="TcpMajorVersion" /> will be increased until it is
        ///     accepted.
        /// </summary>
        private int TcpMajorVersion { get; set; } = 876;

        private object TcpResponseProcessorLock { get; } = new object();

        private object TcpConnectLock { get; } = new object();

        public override T GetFile<T>(Index index, int fileId)
        {
            var fileInfo = index != Index.ReferenceTables ? this.GetReferenceTable(index).GetFileInfo(fileId) : new CacheFileInfo
            {
                Index = index,
                FileId = fileId
            };

            var newFileRequest = this.IndexesUsingHttpInterface.Contains(index) ? (FileRequest)new HttpFileRequest(index, fileId, fileInfo) : new TcpFileRequest(index, fileId, fileInfo);

            var requestKey = new Tuple<Index, int>(index, fileId);

            var fileRequest = this.PendingFileRequests.GetOrAdd(requestKey, newFileRequest);

            var requestOwner = fileRequest == newFileRequest;

            // Start downloading if our request was the one added
            if (requestOwner)
            {
                if (fileRequest is TcpFileRequest)
                {
                    this.StartFileDownloadTcp((TcpFileRequest)fileRequest);
                }
                else
                {
                    this.StartFileDownloadHttp((HttpFileRequest)fileRequest);
                }
            }

            var fileData = fileRequest.WaitForCompletion();

            var file = RuneTek5FileDecoder.DecodeFile(fileData, fileInfo);

            if (!(file is T))
            {
                throw new ArgumentException($"Obtained file is of type  of given type {file.GetType().Name} instead of requested {nameof(T)}.");
            }

            return file as T;
        }

        public override IEnumerable<int> GetFileIds(Index index)
        {
            return this.GetReferenceTable(index).FileIds;
        }

        public override CacheFileInfo GetFileInfo(Index index, int fileId)
        {
            return this.GetReferenceTable(index).GetFileInfo(fileId);
        }

        public MasterReferenceTable GetMasterReferenceTable()
        {
            if (this.CachedMasterReferenceTable != null)
            {
                return this.CachedMasterReferenceTable;
            }

            this.CachedMasterReferenceTable = new MasterReferenceTable(this.GetFile<DataCacheFile>(Index.ReferenceTables, (int)Index.ReferenceTables));

            return this.CachedMasterReferenceTable;
        }

        public ReferenceTable GetReferenceTable(Index index)
        {
            return this.CachedReferenceTables.GetOrAdd(index, index2 => ReferenceTable.Decode(this.GetFile<DataCacheFile>(Index.ReferenceTables, (int)index)));
        }

        public void TcpConnect()
        {
            lock (this.TcpConnectLock)
            {
                if (this.TcpConnected)
                {
                    // No need for this now is there?
                    return;
                }

                CacheDownloader.Logger.Debug("Connecting to content server with TCP.");

                var key = this.GetTcpKeyFromPage();

                // Retry connecting with an increasing major version until the server no longer reports we're outdated
                var connected = false;
                while (!connected)
                {
                    this.TcpContentClient = new TcpClient(this.ContentHost, this.TcpContentPort);

                    var handshakeWriter = new BinaryWriter(this.TcpContentClient.GetStream());
                    var handshakeReader = new BinaryReader(this.TcpContentClient.GetStream());

                    var handshakeLength = (byte) (9 + key.Length + 1);

                    handshakeWriter.Write(this.TcpHandshakeType);
                    handshakeWriter.Write(handshakeLength);
                    handshakeWriter.WriteInt32BigEndian(this.TcpMajorVersion);
                    handshakeWriter.WriteInt32BigEndian(this.TcpMinorVersion);
                    handshakeWriter.WriteNullTerminatedString(key);
                    handshakeWriter.Write((byte)this.Language);
                    handshakeWriter.Flush();

                    var response = (TcpHandshakeResponse) handshakeReader.ReadByte();

                    switch (response)
                    {
                        case TcpHandshakeResponse.Success:
                            connected = true;
                            CacheDownloader.Logger.Info($"Successfully connected to content server with major version {this.TcpMajorVersion}.");
                            break;

                        case TcpHandshakeResponse.Outdated:
                            this.TcpContentClient.Dispose();
                            this.TcpContentClient = null;
                            CacheDownloader.Logger.Info($"Requested connection used outdated version {this.TcpMajorVersion}. Retrying with higher major version.");
                            this.TcpMajorVersion++;
                            break;

                        default:
                            this.TcpContentClient.Dispose();
                            this.TcpContentClient = null;
                            throw new DownloaderException($"Content server responded to handshake with {response}.");
                    }
                }

                // Required loading element sizes. They are unnsed by this tool and I have no idea what they are for. So yeah...
                var contentReader = new BinaryReader(this.TcpContentClient.GetStream());
                contentReader.ReadBytes(this.TcpLoadingRequirementsLength);

                this.SendTcpConnectionInfo();

                this.TcpConnected = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            this.TcpContentClient.Dispose();
        }

        private void AppendVersionToRequestData(FileRequest request)
        {
            if (request.CacheFileInfo != null)
            {
                var dataWriter = new BinaryWriter(request.DataStream);
                dataWriter.WriteUInt16BigEndian((ushort)request.CacheFileInfo.Version);
            }
        }

        private string GetTcpKeyFromPage()
        {
            var request = WebRequest.CreateHttp(this.TcpKeyPage);
            using (var response = request.GetResponse())
            {
                var responseStream = response.GetResponseStream();

                if (responseStream == null)
                {
                    throw new DownloaderException($"No handshake key could be obtained from \"{this.TcpKeyPage}\".");
                }

                var reader = new StreamReader(responseStream);
                var responseString = reader.ReadToEnd();

                var key = this.TcpKeyPageRegex.Match(responseString).Groups[1].Value;

                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new DownloaderException("Obtained handshake key is empty.");
                }

                return key;
            }
        }

        /// <summary>
        ///     Sends the initial connection status and login packets to the server.
        /// </summary>
        private void SendTcpConnectionInfo()
        {
            CacheDownloader.Logger.Debug("Sending initial connection status and login packets.");

            var writer = new BinaryWriter(this.TcpContentClient.GetStream());

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

        private void StartFileDownloadHttp(HttpFileRequest fileRequest)
        {
            Task.Run(() =>
            {
                CacheDownloader.Logger.Debug($"Requesting {fileRequest.Index}/{fileRequest.FileId} using HTTP.");

                var webRequest = WebRequest.CreateHttp($"http://{this.ContentHost}/ms?m=0&a={(int)fileRequest.Index}&g={fileRequest.FileId}&c={fileRequest.CacheFileInfo.Crc}&v={fileRequest.CacheFileInfo.Version}");
                using (var response = (HttpWebResponse)webRequest.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new DownloaderException($"HTTP interface responded with status code: {response.StatusCode}.");
                    }

                    var responseReader = new BinaryReader(response.GetResponseStream());
                    fileRequest.Write(responseReader.ReadBytes((int)response.ContentLength));

                    FileRequest removedRequest;
                    this.PendingFileRequests.TryRemove(new Tuple<Index, int>(fileRequest.Index, fileRequest.FileId), out removedRequest);

                    this.AppendVersionToRequestData(fileRequest);

                    fileRequest.Complete();
                }
            });
        }

        private void StartFileDownloadTcp(TcpFileRequest fileRequest)
        {
            Task.Run(() =>
            {
                if (!this.TcpConnected)
                {
                    this.TcpConnect();
                }

                CacheDownloader.Logger.Debug($"Requesting {fileRequest.Index}/{fileRequest.FileId} using TCP.");

                // Send the request
                var writer = new BinaryWriter(this.TcpContentClient.GetStream());

                // Send the file request to the content server
                writer.Write((byte)(fileRequest.Index == Index.ReferenceTables ? 1 : 0));
                writer.Write((byte)fileRequest.Index);
                writer.WriteInt32BigEndian(fileRequest.FileId);

                // This will process all received TCP chunks until the given requested file is complete (so it might also complete other requested files).
                // Only one processor may be running at any given moment
                lock (this.TcpResponseProcessorLock)
                {
                    CacheDownloader.Logger.Debug("Starting TCP request processor.");

                    while (this.PendingFileRequests.ContainsKey(new Tuple<Index, int>(fileRequest.Index, fileRequest.FileId)))
                    {
                        // Read one chunk
                        if (this.TcpContentClient.Available >= 5)
                        {
                            var reader = new BinaryReader(this.TcpContentClient.GetStream());

                            var readByteCount = 0;

                            var index = (Index)reader.ReadByte();
                            var fileId = reader.ReadInt32BigEndian() & 0x7fffffff;

                            readByteCount += 5;

                            var requestKey = new Tuple<Index, int>(index, fileId);

                            if (!this.PendingFileRequests.ContainsKey(requestKey))
                            {
                                throw new DownloaderException("Invalid response received (maybe not all data was consumed by the previous operation?");
                            }

                            var request = (TcpFileRequest)this.PendingFileRequests[requestKey];
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

                            var remainingBlockLength = this.TcpBlockLength - readByteCount;

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

                                this.AppendVersionToRequestData(removedRequest);

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

                    CacheDownloader.Logger.Debug("TCP request processor finished.");
                }
            });
        }
    }
}