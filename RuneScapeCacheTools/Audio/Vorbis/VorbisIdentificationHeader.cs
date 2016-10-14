using System;
using System.IO;
using System.Linq;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    internal class VorbisIdentificationHeader : VorbisHeader
    {
        public const byte PacketType = 0x01;
        public const uint VorbisVersion = 0;
        public static readonly ushort[] AllowedBlocksizes = { 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

        public byte AudioChannels { get; private set; }
        public uint AudioSampleRate { get; private set; }
        public int BitrateMaximum { get; private set; }
        public int BitrateMinimum { get; private set; }
        public int BitrateNominal { get; private set; }
        public ushort Blocksize0 { get; private set; }
        public ushort Blocksize1 { get; private set; }

        public VorbisIdentificationHeader(Stream packetStream)
        {
            var packetReader = new BinaryReader(packetStream);

            DecodeHeader(packetStream, PacketType);

            var vorbisVersion = packetReader.ReadUInt32();
            if (vorbisVersion != VorbisVersion)
            {
                throw new VorbisException("Vorbis version should report 0 (Vorbis I).");
            }

            AudioChannels = packetReader.ReadByte();
            if (AudioChannels == 0)
            {
                throw new VorbisException("Audio channels must be greater than 0.");
            }

            AudioSampleRate = packetReader.ReadUInt32();
            if (AudioSampleRate == 0)
            {
                throw new VorbisException("Audio sample rate must be greater than 0.");
            }

            BitrateMaximum = packetReader.ReadInt32();
            BitrateNominal = packetReader.ReadInt32();
            BitrateMinimum = packetReader.ReadInt32();

            var blocksize = packetReader.ReadByte();

            Blocksize0 = (ushort)Math.Pow(2, blocksize & 0x0F);
            Blocksize1 = (ushort)Math.Pow(2, blocksize >> 4);

            if (!AllowedBlocksizes.Contains(Blocksize0))
            {
                throw new VorbisException($"Invalid first blocksize \"{Blocksize0}\".");
            }

            if (!AllowedBlocksizes.Contains(Blocksize1))
            {
                throw new VorbisException($"Invalid second blocksize \"{Blocksize1}\".");
            }

            if (Blocksize0 > Blocksize1)
            {
                throw new VorbisException($"First blocksize \"{Blocksize0}\" can't be greater than the second \"{Blocksize1}\".");
            }

            var framingFlag = (byte)(packetReader.ReadByte() & 0x01);

            if (framingFlag != 1)
            {
                throw new VorbisException("Framing flag should be 1 but is 0.");
            }
        }

        public override void Encode(Stream stream)
        {
            EncodeHeader(stream);

            var packetWriter = new BinaryWriter(stream);
            packetWriter.Write(VorbisVersion);
            packetWriter.Write(AudioChannels);
            packetWriter.Write(AudioSampleRate);
            packetWriter.Write(BitrateMaximum);
            packetWriter.Write(BitrateNominal);
            packetWriter.Write(BitrateMinimum);

            var blocksize = (byte)Math.Log(Blocksize0, 2);
            blocksize += (byte)((byte)Math.Log(Blocksize1, 2) << 4);

            packetWriter.Write(blocksize);
            packetWriter.Write((byte)1);
        }
    }
}