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