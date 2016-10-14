using System;
using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Audio.Ogg;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    public abstract class VorbisPacket
    {
        public abstract void Encode(Stream stream);

        /// <summary>
        ///     Converts the packet into a page, or multiple pages if the packet exceeds the maximum page length.
        ///     The pages will only have its data set.
        ///     Further details necessary for writing to a stream like sequence numbers must still be added.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OggPage> ToOggPages()
        {
            var packetStream = new MemoryStream();

            Encode(packetStream);

            var pages = new List<OggPage>();

            packetStream.Position = 0;
            var packetReader = new BinaryReader(packetStream);

            do
            {
                var remainingLength = (int)(packetStream.Length - packetStream.Position);

                var pageDataLength = Math.Min(remainingLength, OggPage.MaxDataLength);

                pages.Add(new OggPage
                {
                    Data = packetReader.ReadBytes(pageDataLength)
                });

                // Add an extra empty "terminator" page when there hasn't been a lacing value lower than 255
                if (remainingLength == OggPage.MaxDataLength)
                {
                    pages.Add(new OggPage());
                }
            }
            while (packetStream.Length - packetStream.Position > 0);

            return pages;
        }

        /// <summary>
        ///     Decodes one packet from the given stream.
        /// </summary>
        /// <returns></returns>
        public static VorbisPacket Decode(Stream packetStream)
        {
            // Decide which type of packet to decode, and act like we never read it
            var packetType = packetStream.ReadByte();
            packetStream.Position -= 1;

            switch (packetType)
            {
                case VorbisIdentificationHeader.PacketType:
                    return new VorbisIdentificationHeader(packetStream);

                case VorbisCommentHeader.PacketType:
                    return new VorbisCommentHeader(packetStream);

                case VorbisSetupHeader.PacketType:
                    return new VorbisSetupHeader(packetStream);

                default:
                    return new VorbisAudioPacket(packetStream);
            }
        }
    }
}