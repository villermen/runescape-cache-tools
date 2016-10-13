using System;
using System.Collections.Generic;
using System.IO;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    public class VorbisReader : IDisposable
    {
        public VorbisReader(Stream input)
        {
            BaseStream = input;
        }

        public Stream BaseStream { get; }

        private int NextPageSequenceNumber { get; set; }

        /// <summary>
        ///     Can contain upcoming pages in stream that have been peeked at but have not been used yet.
        /// </summary>
        private Queue<VorbisPage> PageBuffer { get; } = new Queue<VorbisPage>();

        public VorbisPacket ReadPacket()
        {
            // Read pages till a full packet is obtained
            var packetDataWriter = new MemoryStream();
            var page = ReadPage();
            do
            {
                packetDataWriter.Write(page.Data, 0, page.Data.Length);

                page = ReadPage();
            }
            while (page.HeaderType.HasFlag(VorbisPageHeaderType.ContinuedPacket));

            // Last read page is not part of the packet: Save it for later.
            PageBuffer.Enqueue(page);

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

        public VorbisPage ReadPage()
        {
            return PageBuffer.Count > 0 ? PageBuffer.Dequeue() : VorbisPage.Decode(BaseStream);
        }

        public void Dispose()
        {
            BaseStream?.Dispose();
        }
    }
}