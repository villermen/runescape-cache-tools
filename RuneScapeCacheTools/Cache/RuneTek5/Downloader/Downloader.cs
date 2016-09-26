using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using log4net;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5.Downloader
{
    public class Downloader
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Downloader));

        public CacheBase Cache { get; }

        public string ContentHost { get; set; } = "content.runescape.com";

        public int ContentPort { get; set; } = 43594;

        /// <summary>
        /// The major version is needed to correctly connect to the content server.
        /// 
        /// If connection states the version is outdated, the <see cref="MajorVersion"/> will be increased until it is accepted.
        /// </summary>
        private int MajorVersion { get; set; } = 850;

        /// <summary>
        /// The minor version is needed to correctly connect to the content server.
        /// 
        /// This seems to always be 1.
        /// </summary>
        public int MinorVersion { get; set; } = 1;

        /// <summary>
        /// The handshake type is needed to correctly connect to the content server.
        /// </summary>
        private byte HandshakeType { get; } = 15;

        public Language Language { get; set; } = Language.English;

        /// <summary>
        /// The page used in obtaining the content server handshake key.
        /// </summary>
        public string KeyPage { get; set; } = "http://world2.runescape.com";

        /// <summary>
        /// The regex used to obtain the content server handshake key from the set <see cref="KeyPage"/>.
        /// 
        /// The first capture group needs to result in the key.
        /// </summary>
        public Regex KeyPageRegex { get; set; } = new Regex(@"<param\s+name=""1""\s+value=""([^""]+)""");

        public Downloader(CacheBase cache)
        {
            Cache = cache;
        }

        public void Connect()
        {
            var key = GetKeyFromPage();

            // Retry connecting with an increasing major version until the server no longer reports we're outdated
            HandshakeResponse response;

            do
            {
                using (var contentClient = new TcpClient(ContentHost, ContentPort))
                {
                    var contentWriter = new BinaryWriter(contentClient.GetStream());
                    var contentReader = new BinaryReader(contentClient.GetStream());

                    var handshakeLength = (byte)(9 + key.Length + 1);

                    contentWriter.Write(HandshakeType);
                    contentWriter.Write(handshakeLength);
                    contentWriter.WriteInt32BigEndian(MajorVersion);
                    contentWriter.WriteInt32BigEndian(MinorVersion);
                    contentWriter.WriteNullTerminatedString(key);
                    contentWriter.Write((byte) Language);
                    contentWriter.Flush();

                    response = (HandshakeResponse) contentReader.ReadByte();

                    if (response == HandshakeResponse.InvalidKey)
                    {
                        throw new DownloaderException("Handshake was not accepted by server.");
                    }
                }

                MajorVersion++;
            }
            while (response == HandshakeResponse.Outdated);

            MajorVersion--;

            if (response != HandshakeResponse.Success)
            {
                throw new DownloaderException($"Content server responded to handshake with {response}.");
            }
        }

        public string GetKeyFromPage()
        {
            var request = WebRequest.Create(KeyPage);
            var response = request.GetResponse();
            var responseStream = response.GetResponseStream();

            if (responseStream == null)
            {
                throw new DownloaderException($"No content key could be obtained from \"{KeyPage}\".");
            }

            using (var reader = new StreamReader(responseStream))
            {
                var responseString = reader.ReadToEnd();

                var key = KeyPageRegex.Match(responseString).Groups[1].Value;

                if (string.IsNullOrWhiteSpace(key))
                { 
                    throw new DownloaderException("Obtained content key is empty.");
                }

                return key;
            }
        }
    }
}