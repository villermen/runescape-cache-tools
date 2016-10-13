using System;
using System.IO;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    public class VorbisSetupHeader : VorbisHeaderPacket
    {
        public const byte PacketType = 0x05;

        public static VorbisSetupHeader Decode(byte[] packetData)
        {
            var packetStream = new MemoryStream(packetData);
            var packetReader = new BinaryReader(packetStream);

            VerifyHeaderSignature(packetStream, PacketType);

            // I don't really care about this header yet

            return new VorbisSetupHeader();
        }

        public override void Encode(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}