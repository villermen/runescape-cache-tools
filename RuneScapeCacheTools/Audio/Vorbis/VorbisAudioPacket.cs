using System;
using System.IO;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    public class VorbisAudioPacket : VorbisPacket
    {
        public static VorbisAudioPacket Decode(byte[] packetData)
        {
            var packet = new VorbisAudioPacket();

            // Verify that this is indeed an audio packet
            var packetType = packetData[0] & 0x01;

            if (packetType != 0)
            {
                throw new VorbisException("Audio packet type must be 0, but 1 was read.");
            }

            // I don't really care for the breakdown of the audio packet for now
            packet.Data = packetData;

            return packet;
        }

        public override void Encode(Stream stream)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The full undecoded data of the packet.
        /// </summary>
        public byte[] Data { get; private set; }
    }
}