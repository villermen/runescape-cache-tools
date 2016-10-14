using System.IO;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    public class VorbisSetupHeader : VorbisHeader
    {
        public const byte PacketType = 0x05;

        public byte[] Data { get; set; }

        public VorbisSetupHeader(Stream packetStream)
        {
            var packetReader = new BinaryReader(packetStream);

            DecodeHeader(packetStream, PacketType);

            // I don't really care for the contents of the setup header packet for now
            Data = packetReader.ReadBytes((int)(packetStream.Length - packetStream.Position));
        }

        public override void Encode(Stream stream)
        {
            EncodeHeader(stream);

            stream.Write(Data, 0, Data.Length);
        }
    }
}