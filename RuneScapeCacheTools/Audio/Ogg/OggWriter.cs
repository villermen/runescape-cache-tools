using System;
using System.IO;

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

        public void WriteOggPacket(OggPacket packet)
        {
            foreach (var page in packet.Pages)
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