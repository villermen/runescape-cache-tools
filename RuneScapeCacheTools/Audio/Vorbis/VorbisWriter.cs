using System;
using System.IO;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    public class VorbisWriter : IDisposable
    {
        public VorbisWriter(Stream output)
        {
            BaseStream = output;
        }

        public Stream BaseStream { get; }

        public void WritePacket(VorbisPacket packet)
        {
            foreach (var page in packet.ToPages())
            {
                WritePage(page);
            }
        }

        public void WritePage(VorbisPage page)
        {
            page.Encode(BaseStream);
        }

        public void Dispose()
        {
            BaseStream?.Dispose();
        }
    }
}