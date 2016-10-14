using System.IO;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    public class VorbisSetupHeader : VorbisHeader
    {
        public const byte PacketType = 0x05;

        public byte[] Data { get; set; }

        public VorbisSetupHeader(byte[] packetData)
        {
            var packetStream = new MemoryStream(packetData);
            var packetReader = new BinaryReader(packetStream);

            DecodeHeader(packetStream, PacketType);

            // I don't really care about the contents of this header yet
            Data = packetReader.ReadBytes((int)(packetStream.Length - packetStream.Position));
        }

        public override void Encode(Stream stream)
        {
            EncodeHeader(stream);
        }
    }
}