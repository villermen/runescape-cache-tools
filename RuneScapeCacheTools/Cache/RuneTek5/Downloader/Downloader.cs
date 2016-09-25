using System.IO;
using System.Net.Sockets;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5.Downloader
{
    public class Downloader
    {
        public string ContentHost { get; set; } = "content.runescape.com";

        public int ContentPort { get; set; } = 43594;

        public int MajorVersion { get; set; } = 873;

        public int MinorVersion { get; set; } = 1;

        public string Key { get; set; } = "0OD4uV6PB0iiUzHXDeqgmZy7Z3BogkXY";

        public byte HandshakeType { get; set; } = 15;

        public int LanguageIndex { get; set; } = 0;

        /*
         * Get key from game page (param with name 1), https://world#.runescape.com
         * Connect to content server (content.runescape.com)
         * 
         * 
         */
        public Downloader()
        {
           
        }

        // TODO: Make private
        // TODO: Configure major version
        public void Connect()
        {
            using (var contentClient = new TcpClient(ContentHost, ContentPort))
            {
                var contentWriter = new BinaryWriter(contentClient.GetStream());
                var contentReader = new BinaryReader(contentClient.GetStream());

                var handshakeLength = (byte) (9 + Key.Length + 1);

                contentWriter.Write(HandshakeType);
                contentWriter.Write(handshakeLength);
                contentWriter.WriteInt32BigEndian(MajorVersion);
                contentWriter.WriteInt32BigEndian(MinorVersion);
                contentWriter.WriteNullTerminatedString(Key);
                contentWriter.Write(LanguageIndex);
                contentWriter.Flush();

                var response = (HandshakeResponse) contentReader.ReadByte();
            }
        }
    }
}