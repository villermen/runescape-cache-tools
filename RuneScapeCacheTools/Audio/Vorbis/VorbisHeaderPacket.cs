using System.IO;
using System.Linq;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    public abstract class VorbisHeaderPacket : VorbisPacket
    {
        public static readonly byte[] VorbisSignature = { 0x76, 0x6F, 0x72, 0x62, 0x69, 0x73 };

        public static void VerifyHeaderSignature(Stream dataStream, byte expectedPacketType)
        {
            var dataReader = new BinaryReader(dataStream);

            var packetType = dataReader.ReadByte();
            if (packetType != expectedPacketType)
            {
                throw new VorbisException($"Vorbis comment header packet type incorrect ({packetType} instead of {expectedPacketType}).");
            }

            var vorbisSignature = dataReader.ReadBytes(6);
            if (!vorbisSignature.SequenceEqual(VorbisSignature))
            {
                throw new VorbisException("Vorbis header signature incorrect.");
            }
        }
    }
}