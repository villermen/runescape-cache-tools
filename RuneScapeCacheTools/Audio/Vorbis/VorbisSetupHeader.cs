using System;
using System.IO;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    internal class VorbisSetupHeader : VorbisHeader
    {
        public const byte PacketType = 0x05;

        public static VorbisSetupHeader Decode(byte[] packetData)
        {
            throw new NotImplementedException();
        }
    }
}