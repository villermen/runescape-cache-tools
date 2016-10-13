using System;
using System.IO;
using System.Linq;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    internal class VorbisIdentificationHeaderPacket : VorbisHeaderPacket
    {
        public const byte PacketType = 0x01;
        public static readonly ushort[] AllowedBlocksizes = { 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

        public static VorbisIdentificationHeaderPacket Decode(byte[] packetData)
        {
            var packetStream = new MemoryStream(packetData);
            var packetReader = new BinaryReader(packetStream);

            VerifyHeaderSignature(packetStream, PacketType);

            var packet = new VorbisIdentificationHeaderPacket();

            var vorbisVersion = packetReader.ReadUInt32();
            if (vorbisVersion != 0)
            {
                throw new VorbisException("Vorbis version should report 0 (Vorbis I).");
            }

            packet.AudioChannels = packetReader.ReadByte();
            if (packet.AudioChannels == 0)
            {
                throw new VorbisException("Audio channels must be greater than 0.");
            }

            packet.AudioSampleRate = packetReader.ReadUInt32();
            if (packet.AudioSampleRate == 0)
            {
                throw new VorbisException("Audio sample rate must be greater than 0.");
            }

            packet.BitrateMaximum = packetReader.ReadInt32();
            packet.BitrateNominal = packetReader.ReadInt32();
            packet.BitrateMinimum = packetReader.ReadInt32();

            byte blocksize = packetReader.ReadByte();

            packet.Blocksize0 = (ushort)Math.Pow(2, blocksize & 0x0F);
            packet.Blocksize1 = (ushort)Math.Pow(2, blocksize >> 4);

            if (!AllowedBlocksizes.Contains(packet.Blocksize0))
            {
                throw new VorbisException($"Invalid first blocksize \"{packet.Blocksize0}\".");
            }

            if (!AllowedBlocksizes.Contains(packet.Blocksize1))
            {
                throw new VorbisException($"Invalid second blocksize \"{packet.Blocksize1}\".");
            }

            if (packet.Blocksize0 > packet.Blocksize1)
            {
                throw new VorbisException($"First blocksize \"{packet.Blocksize0}\" can't be greater than the second \"{packet.Blocksize1}\".");
            }

            var framingFlag = (byte)(packetReader.ReadByte() & 0x01);

            if (framingFlag != 1)
            {
                throw new VorbisException("Framing flag should be 1 but is 0.");
            }

            return packet;
        }

        public byte AudioChannels { get; private set; }
        public uint AudioSampleRate { get; private set; }
        public int BitrateMaximum { get; private set; }
        public int BitrateNominal { get; private set; }
        public int BitrateMinimum { get; private set; }
        public ushort Blocksize0 { get; private set; }
        public ushort Blocksize1 { get; private set; }
    }
}