using System;
using System.IO;
using Villermen.RuneScapeCacheTools.Audio.Vorbis;

namespace Villermen.RuneScapeCacheTools.Audio.Ogg
{
    public class OggWriter : IDisposable
    {
        public OggWriter(Stream output)
        {
            BaseStream = output;
        }

        public Stream BaseStream { get; }

        public void Dispose()
        {
            BaseStream?.Dispose();
        }

        public void WritePacket(VorbisPacket packet)
        {
            foreach (var page in packet.ToPages())
            {
                WritePage(page);
            }
        }

        public void WritePage(OggPage page)
        {
            page.Encode(BaseStream);
        }
    }
}