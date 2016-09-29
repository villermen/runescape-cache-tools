using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
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

        /// <summary>
        ///     The regex used to obtain the content server handshake key from the set <see cref="KeyPage" />.
        ///     The first capture group needs to result in the key.
        /// </summary>
        public Regex KeyPageRegex { get; set; } = new Regex(@"<param\s+name=""1""\s+value=""([^""]+)""");

        private int LoadingRequirementsLength { get; } = 26 * 4;

        private TcpClient ContentClient { get; set; }

        public bool Connected { get; private set; }

        private Dictionary<Tuple<int, int>, TaskCompletionSource<Stream>> PendingRequests { get; } =
            new Dictionary<Tuple<int, int>, TaskCompletionSource<Stream>>();

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

                var handshakeLength = (byte) (9 + key.Length + 1);

                handshakeWriter.Write(HandshakeType);
                handshakeWriter.Write(handshakeLength);
                handshakeWriter.WriteInt32BigEndian(MajorVersion);
                handshakeWriter.WriteInt32BigEndian(MinorVersion);
                handshakeWriter.WriteNullTerminatedString(key);
                handshakeWriter.Write((byte) Language);
                handshakeWriter.Flush();

                var response = (HandshakeResponse) handshakeReader.ReadByte();

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
            writer.Write((byte) (indexId == RuneTek5Cache.MetadataIndexId ? 1 : 0));
            writer.Write((byte) indexId);
            writer.WriteInt32BigEndian(fileId);

            var reader = new BinaryReader(ContentClient.GetStream());

            var fileIndexId = reader.ReadByte();
            var fileFileId = reader.ReadInt32BigEndian() & 0x7fffffff;

            if (fileIndexId != indexId)
            {
                throw new DownloaderException(
                    $"Obtained file's index id ({fileIndexId}) does not match requested ({indexId}).");
            }

            if (fileFileId != fileId)
            {
                throw new DownloaderException(
                    $"Obtained file's file id ({fileFileId}) does not match requested ({fileId}).");
            }

            var referenceTableEntry = indexId != RuneTek5Cache.MetadataIndexId ? DownloadReferenceTable(indexId).Files[fileId] : null;

            return new RuneTek5CacheFile(ContentClient.GetStream(), referenceTableEntry);
        }

        public ReferenceTable DownloadReferenceTable(int indexId)
        {
            return new ReferenceTable(DownloadFile(RuneTek5Cache.MetadataIndexId, indexId).Data, indexId);
        }   
    }
}