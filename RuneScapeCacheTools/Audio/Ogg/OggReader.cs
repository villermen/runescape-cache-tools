using System;
using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Audio.Vorbis;

namespace Villermen.RuneScapeCacheTools.Audio.Ogg
{
    public class OggReader : IDisposable
    {
        // TODO: Is interleaving between packets possible?

        public OggReader(Stream input)
        {
            BaseStream = input;
        }

        public Stream BaseStream { get; }

        /// <summary>
        ///     Maps the serial number of the logical bitstreams encountered in the stream to their last read sequence number.
        /// </summary>
        private Dictionary<int, int> LogicalBitstreams { get; } = new Dictionary<int, int>();

        private bool FirstPageRead { get; set; }

        /// <summary>
        ///     Might contain the next page in the stream, if it was needed only for its header.
        /// </summary>
        private OggPage NextPage { get; set; }

        /// <summary>
        ///     Might contain a packet containing the data left over after the previous Vorbis packet was read.
        ///     Indicates that another Vorbis packet is available if not null.
        /// </summary>
        private Stream CurrentOggPacket { get; set; }

        public void Dispose()
        {
            BaseStream?.Dispose();
        }

        /// <summary>
        ///     Multiple Vorbis packets can be contained in one Ogg packet.
        ///     This packet reads out one vorbis packet from an Ogg packet.
        /// </summary>
        /// <returns></returns>
        public VorbisPacket ReadVorbisPacket()
        {
            // Obtain a new Ogg packet if the previous one is fully processed
            if (CurrentOggPacket == null)
            {
                var oggPacket = ReadOggPacket();

                if (oggPacket == null)
                {
                    return null;
                }

                CurrentOggPacket = oggPacket;
            }

            var packet = VorbisPacket.Decode(CurrentOggPacket);

            // Clear the packet stream if we have read it fully
            if (CurrentOggPacket.Length - CurrentOggPacket.Position == 0)
            {
                CurrentOggPacket = null;
            }

            return packet;
        }

        /// <summary>
        ///     Reads a complete Ogg packet from the stream.
        /// </summary>
        /// <returns></returns>
        public OggPacket ReadOggPacket()
        {
            var packet = new OggPacket();

            var page = ReadPage();

            // Cancel reading Vorbis packets from the previous Ogg packet, since we've progressed further now
            CurrentOggPacket = null;

            if (page == null)
            {
                return null;
            }

            while (true)
            {
                packet.Pages.Add(page);

                page = ReadPage();

                // Check if the next page is a continuation of this one
                if (page == null || !page.HeaderType.HasFlag(VorbisPageHeaderType.ContinuedPacket))
                {
                    // Next page is part of the next packet, or null. Save it for later.
                    NextPage = page;
                    break;
                }
            }

            return packet;
        }

        public OggPage ReadPage()
        {
            // Return null when the physical bitstream has ended
            if (FirstPageRead && LogicalBitstreams.Count == 0)
            {
                return null;
            }

            var page = NextPage ?? new OggPage(BaseStream);

            FirstPageRead = true;
            NextPage = null;

            // Cancel reading packets from the previous page, since we've progressed further now
            CurrentOggPacket = null;

            // Add the logical strema if this was its first packet
            if (page.HeaderType.HasFlag(VorbisPageHeaderType.FirstPage))
            {
                LogicalBitstreams.Add(page.StreamSerialNumber, page.SequenceNumber);
            }
            else
            {
                // Logical stream must be known if this is not its first packet
                if (!LogicalBitstreams.ContainsKey(page.StreamSerialNumber))
                {
                    throw new OggException($"Logical stream with serial number {page.StreamSerialNumber} was not started, yet a page of it was obtained.");
                }

                // Sequence number must be one higher than the previous one if not a continuation
                if (page.SequenceNumber != LogicalBitstreams[page.StreamSerialNumber] + 1 && page.SequenceNumber != LogicalBitstreams[page.StreamSerialNumber])
                {
                    throw new OggException($"Obtained page does has wrong sequence number {page.SequenceNumber}, expected was {LogicalBitstreams[page.StreamSerialNumber] + 1}.");
                }

                // Update the sequence number
                LogicalBitstreams[page.StreamSerialNumber] = page.SequenceNumber;
            }

            // Remove the logical stream if this was its last packet
            if (page.HeaderType.HasFlag(VorbisPageHeaderType.LastPage))
            {
                LogicalBitstreams.Remove(page.StreamSerialNumber);
            }

            return page;
        }
    }
}