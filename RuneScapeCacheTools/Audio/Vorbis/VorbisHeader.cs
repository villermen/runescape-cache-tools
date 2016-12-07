using System.IO;
using System.Linq;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    public abstract class VorbisHeader : VorbisPacket
    {
        public static readonly byte[] VorbisSignature = { 0x76, 0x6F, 0x72, 0x62, 0x69, 0x73 };

        public byte HeaderPacketType { get; private set; }

        /// <summary>
        ///     Decodes and verifies the header of this header from the stream.
        ///     Use before decoding the header's data.
        /// </summary>
        /// <param name="dataStream"></param>
        /// <param name="expectedPacketType"></param>
        public void DecodeHeader(Stream dataStream, byte expectedPacketType)
        {
            var dataReader = new BinaryReader(dataStream);

            HeaderPacketType = dataReader.ReadByte();
            if (HeaderPacketType != expectedPacketType)
            {
                throw new VorbisException($"Vorbis comment header packet type incorrect ({HeaderPacketType} instead of {expectedPacketType}).");
            }

            var vorbisSignature = dataReader.ReadBytes(6);
            if (!vorbisSignature.SequenceEqual(VorbisSignature))
            {
                throw new VorbisException("Vorbis header signature incorrect.");
            }
        }

        /// <summary>
        ///     Encode header of this header to the stream.
        ///     Use before encoding the header's data.
        /// </summary>
        /// <param name="stream"></param>
        public void EncodeHeader(Stream stream)
        {
            var headerWriter = new BinaryWriter(stream);
            headerWriter.Write(HeaderPacketType);
            headerWriter.Write(VorbisSignature);
        }
    }
}