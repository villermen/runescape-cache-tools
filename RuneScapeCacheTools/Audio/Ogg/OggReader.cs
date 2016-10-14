using System;
using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Audio.Vorbis;

namespace Villermen.RuneScapeCacheTools.Audio.Ogg
{
    public class OggReader : IDisposable
    {
        public OggReader(Stream input)
        {
            BaseStream = input;
        }

        public Stream BaseStream { get; }

        private int NextPageSequenceNumber { get; set; }

        private bool LastPageRead { get; set; }

        /// <summary>
        ///     Can contain upcoming pages in stream that have been peeked at but have not been used yet.
        /// </summary>
        private Queue<OggPage> PageBuffer { get; } = new Queue<OggPage>();

        public VorbisPacket ReadPacket()
        {
            // Read pages till a full packet is obtained
            var packetDataWriter = new MemoryStream();
            var page = ReadPage();

            if (page == null)
            {
                return null;
            }

            do
            {
                packetDataWriter.Write(page.Data, 0, page.Data.Length);

                page = ReadPage();
            }
            while (page != null && page.HeaderType.HasFlag(VorbisPageHeaderType.ContinuedPacket));

            // Last read page is not part of the packet: Save it for later.
            if (page != null)
            {
                PageBuffer.Enqueue(page);
            }

            var packetData = packetDataWriter.ToArray();
            var packetType = packetData[0];

            switch (packetType)
            {
                // Identification header
                case VorbisIdentificationHeader.PacketType:
                    return VorbisIdentificationHeader.Decode(packetData);

                // Comment header
                case VorbisCommentHeader.PacketType:
                    return VorbisCommentHeader.Decode(packetData);

                // Setup header
                case VorbisSetupHeader.PacketType:
                    return VorbisSetupHeader.Decode(packetData);

                // Audio packet
                default:
                    return VorbisAudioPacket.Decode(packetData);
            }
        }

        public OggPage ReadPage()
        {
            if (LastPageRead)
            {
                return null;
            }

            var page = PageBuffer.Count > 0 ? PageBuffer.Dequeue() : OggPage.Decode(BaseStream);

            if (page.HeaderType.HasFlag(VorbisPageHeaderType.LastPage))
            {
                LastPageRead = true;
            }

            return page;
        }

        public void Dispose()
        {
            BaseStream?.Dispose();
        }
    }
}