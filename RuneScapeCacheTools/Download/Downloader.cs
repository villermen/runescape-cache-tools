using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using Org.BouncyCastle.Pkcs;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Enums;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Download
{
    /// <summary>
    /// </summary>
    /// <author>Villermen</author>
    /// <author>Method</author>
    public class Downloader : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Downloader));

        public Downloader(CacheBase cache)
        {
            Cache = cache;
        }

        public CacheBase Cache { get; }

        public string ContentHost { get; set; } = "content.runescape.com";

        public int ContentPort { get; set; } = 43594;

        /// <summary>
        ///     The major version is needed to correctly connect to the content server.
        ///     If connection states the version is outdated, the <see cref="MajorVersion" /> will be increased until it is
        ///     accepted.
        /// </summary>
        private int MajorVersion { get; set; } = 873;

        /// <summary>
        ///     The minor version is needed to correctly connect to the content server.
        ///     This seems to always be 1.
        /// </summary>
        public int MinorVersion { get; set; } = 1;

        /// <summary>
        ///     The handshake type is needed to correctly connect to the content server.
        /// </summary>
        private byte HandshakeType { get; } = 15;

        public Language Language { get; set; } = Language.English;

        /// <summary>
        ///     The page used in obtaining the content server handshake key.
        /// </summary>
        public string KeyPage { get; set; } = "http://world2.runescape.com";

        public int BlockLength { get; set; } = 102400;

        /// <summary>
        ///     The regex used to obtain the content server handshake key from the set <see cref="KeyPage" />.
        ///     The first capture group needs to result in the key.
        /// </summary>
        public Regex KeyPageRegex { get; set; } = new Regex(@"<param\s+name=""1""\s+value=""([^""]+)""");

        private int LoadingRequirementsLength { get; } = 26 * 4;

        private TcpClient ContentClient { get; set; }

        public bool Connected { get; private set; }

        private Dictionary<Tuple<int, int>, FileRequest> PendingFileRequests { get; } =
            new Dictionary<Tuple<int, int>, FileRequest>();

        public void Dispose()
        {
            ContentClient.Dispose();
        }

        public void Connect()
        {
            var key = GetKeyFromPage();

            // Retry connecting with an increasing major version until the server no longer reports we're outdated
            var connected = false;
            while (!connected)
            {
                ContentClient = new TcpClient(ContentHost, ContentPort);

                var handshakeWriter = new BinaryWriter(ContentClient.GetStream());
                var handshakeReader = new BinaryReader(ContentClient.GetStream());

                var handshakeLength = (byte)(9 + key.Length + 1);

                handshakeWriter.Write(HandshakeType);
                handshakeWriter.Write(handshakeLength);
                handshakeWriter.WriteInt32BigEndian(MajorVersion);
                handshakeWriter.WriteInt32BigEndian(MinorVersion);
                handshakeWriter.WriteNullTerminatedString(key);
                handshakeWriter.Write((byte)Language);
                handshakeWriter.Flush();

                var response = (HandshakeResponse)handshakeReader.ReadByte();

                switch (response)
                {
                    case HandshakeResponse.Success:
                        connected = true;
                        Logger.Info($"Successfully connected to content server with major version {MajorVersion}.");
                        break;

                    case HandshakeResponse.Outdated:
                        ContentClient.Dispose();
                        ContentClient = null;
                        Logger.Info($"Content server says {MajorVersion} is outdated.");
                        MajorVersion++;
                        break;

                    default:
                        ContentClient.Dispose();
                        ContentClient = null;
                        throw new DownloaderException($"Content server responded to handshake with {response}.");
                }
            }

            // Required loading element sizes. They are unnsed by this tool and I have no idea what they are for. So yeah...
            var contentReader = new BinaryReader(ContentClient.GetStream());
            contentReader.ReadBytes(LoadingRequirementsLength);

            SendConnectionInfo();

            Connected = true;
        }

        private string GetKeyFromPage()
        {
            var request = WebRequest.Create(KeyPage);
            var response = request.GetResponse();
            var responseStream = response.GetResponseStream();

            if (responseStream == null)
            {
                throw new DownloaderException($"No handshake key could be obtained from \"{KeyPage}\".");
            }

            using (var reader = new StreamReader(responseStream))
            {
                var responseString = reader.ReadToEnd();

                var key = KeyPageRegex.Match(responseString).Groups[1].Value;

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
        private void SendConnectionInfo()
        {
            var writer = new BinaryWriter(ContentClient.GetStream());

            // I don't know
            writer.Write((byte) 6);
            writer.WriteUInt24BigEndian(4);
            writer.WriteInt16BigEndian(0);
            writer.Flush();

            writer.Write((byte) 3);
            writer.WriteUInt24BigEndian(0);
            writer.WriteInt16BigEndian(0);
            writer.Flush();
        }

        public RuneTek5CacheFile DownloadFile(int indexId, int fileId)
        {
            if (!Connected)
            {
                throw new DownloaderException("Can't request file when disconnected.");
            }

            var writer = new BinaryWriter(ContentClient.GetStream());

            // Send the file request to the content server
            writer.Write((byte)(indexId == RuneTek5Cache.MetadataIndexId ? 1 : 0));
            writer.Write((byte)indexId);
            writer.WriteInt32BigEndian(fileId);

            var fileRequest = new FileRequest();

            var pendingFileRequestCount = PendingFileRequests.Count;

            PendingFileRequests.Add(new Tuple<int, int>(indexId, fileId), fileRequest);

            // Spin up the processor when it is not running
            if (pendingFileRequestCount == 0)
            {
                Task.Run(() => ProcessRequests());
            }

            // TODO: Caching for reference tables
            var referenceTableEntry = indexId != RuneTek5Cache.MetadataIndexId ? DownloadReferenceTable(indexId).Files[fileId] : null;

            fileRequest.WaitForCompletion();

            return new RuneTek5CacheFile(fileRequest.DataStream.ToArray(), referenceTableEntry);
        }

        public ReferenceTable DownloadReferenceTable(int indexId)
        {
            return new ReferenceTable(DownloadFile(RuneTek5Cache.MetadataIndexId, indexId), indexId);
        }

        public MasterReferenceTable DownloadMasterReferenceTable()
        {
            return new MasterReferenceTable(DownloadFile(RuneTek5Cache.MetadataIndexId, RuneTek5Cache.MetadataIndexId));
        }

        public void ProcessRequests()
        {
            while (PendingFileRequests.Count > 0)
            {
                // Read one chunk (or the leftover)
                if (ContentClient.Available >= 5)
                {
                    var reader = new BinaryReader(ContentClient.GetStream());

                    var readByteCount = 0;

                    var indexId = reader.ReadByte();
                    var fileId = reader.ReadInt32BigEndian() & 0x7fffffff;

                    readByteCount += 5;

                    var requestKey = new Tuple<int, int>(indexId, fileId);

                    if (!PendingFileRequests.ContainsKey(requestKey))
                    {
                        throw new DownloaderException("Invalid response received (maybe not all data was consumed by the previous operation?");
                    }

                    var request = PendingFileRequests[requestKey];
                    var writer = new BinaryWriter(request.DataStream);

                    // The first part of the file always contains the filesize, which we need to know, but is also part of the file
                    if (request.FileSize == 0)
                    {
                        var compressionType = (CompressionType)reader.ReadByte();
                        var length = reader.ReadInt32BigEndian();

                        readByteCount += 5;

                        request.FileSize = 5 + (compressionType != CompressionType.None ? 4 : 0) + length;

                        writer.Write((byte)compressionType);
                        writer.WriteInt32BigEndian(length);
                    }

                    var remainingBlockLength = BlockLength - readByteCount;

                    if (remainingBlockLength > request.RemainingLength)
                    {
                        remainingBlockLength = request.RemainingLength;
                    }

                    writer.Write(reader.ReadBytes(remainingBlockLength));

                    if (request.RemainingLength == 0)
                    {
                        request.Complete();
                        PendingFileRequests.Remove(requestKey);
                    }
                }

                // var leftoverBytes = new BinaryReader(ContentClient.GetStream()).ReadBytes(ContentClient.Available);
            }
        }
    }
}