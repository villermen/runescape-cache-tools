using System;
using System.IO;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    internal class VorbisSetupHeaderPacket : VorbisHeaderPacket
    {
        public const byte PacketType = 0x05;

        public static VorbisSetupHeaderPacket Decode(byte[] packetData)
        {
            var packetStream = new MemoryStream(packetData);
            var packetReader = new BinaryReader(packetStream);

            VerifyHeaderSignature(packetStream, PacketType);

            throw new NotImplementedException();
        }
    }
}