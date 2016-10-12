using System;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    internal class VorbisIdentificationHeader : VorbisHeader
    {
        public const byte PacketType = 0x01;

        public static VorbisIdentificationHeader Decode(byte[] packetData)
        {
            throw new NotImplementedException();
        }
    }
}