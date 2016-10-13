using System;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    [Flags]
    public enum VorbisPageHeaderType
    {
        ContinuedPacket = 0x01,
        FirstPage = 0x02,
        LastPage = 0x04
    }
}