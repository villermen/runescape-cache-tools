namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    internal abstract class VorbisHeader : VorbisPacket
    {
        public static readonly byte[] Vorbis = { 0x76, 0x6F, 0x72, 0x62, 0x69, 0x73 };
    }
}