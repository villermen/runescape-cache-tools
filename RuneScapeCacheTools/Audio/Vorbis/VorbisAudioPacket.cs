using System.IO;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    public class VorbisAudioPacket : VorbisPacket
    {
        /// <summary>
        ///     The full undecoded data of the packet.
        /// </summary>
        public byte[] Data { get; private set; }

        public VorbisAudioPacket(byte[] packetData)
        {
            // Verify that this is indeed an audio packet
            var packetType = packetData[0] & 0x01;

            if (packetType != 0)
            {
                throw new VorbisException("Audio packet type must be 0, but 1 was read.");
            }

            // I don't really care for the breakdown of the audio packet for now
            Data = packetData;
        }

        public override void Encode(Stream stream)
        {
            stream.Write(Data, 0, Data.Length);
        }
    }
}