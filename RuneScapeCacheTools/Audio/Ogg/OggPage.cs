using System;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Audio.Vorbis;

namespace Villermen.RuneScapeCacheTools.Audio.Ogg
{
    public class OggPage
    {
        public const int MaxDataLength = 255 * 255;
        public const byte StreamStructureVersion = 0x00;
        public static readonly byte[] CapturePattern = { 0x4F, 0x67, 0x67, 0x53 };

        private byte[] _data;
        public long AbsoluteGranulePosition { get; private set; }
        public int Checksum { get; private set; }

        public byte[] Data
        {
            get { return _data; }

            set
            {
                if (value.Length > MaxDataLength)
                {
                    throw new VorbisException($"One page cannot contain more than {MaxDataLength} bytes.");
                }

                _data = value;
            }
        }

        public VorbisPageHeaderType HeaderType { get; private set; }
        public int SequenceNumber { get; private set; }
        public int StreamSerialNumber { get; private set; }

        public OggPage()
        { 
        }

        public OggPage(Stream pageStream)
        {
            var pageReader = new BinaryReader(pageStream);

            var capturePattern = pageReader.ReadBytes(4);
            if (!capturePattern.SequenceEqual(CapturePattern))
            {
                throw new OggException($"Invalid capture pattern \"0x{BitConverter.ToString(capturePattern)}\" (magic number).");
            }

            var streamStructureVersion = pageReader.ReadByte();
            if (streamStructureVersion != StreamStructureVersion)
            {
                throw new VorbisException($"Invalid stream structure version \"{streamStructureVersion}\", only Vorbis I is supported.");
            }

            HeaderType = (VorbisPageHeaderType)pageReader.ReadByte();
            AbsoluteGranulePosition = pageReader.ReadInt64();

            StreamSerialNumber = pageReader.ReadInt32();
            SequenceNumber = pageReader.ReadInt32();

            var checksum = pageReader.ReadUInt32();

            var segmentCount = pageReader.ReadByte();
            var lacingValues = new byte[segmentCount];
            for (var segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                lacingValues[segmentIndex] = pageReader.ReadByte();
            }

            var dataLength = lacingValues.Aggregate(0, (total, addition) => total + addition);
            Data = pageReader.ReadBytes(dataLength);

            // Calculate checksum from the obtained values.
            // If the checksum is calculated from the reconstructed data, there might be inconsistencies (e.g. non-standard lacing values)
            var crc = new OggCrc();
            var checksumStream = new BinaryWriter(crc);
            checksumStream.Write(capturePattern);
            checksumStream.Write(streamStructureVersion);
            checksumStream.Write((byte)HeaderType);
            checksumStream.Write(AbsoluteGranulePosition);
            checksumStream.Write(StreamSerialNumber);
            checksumStream.Write(SequenceNumber);
            checksumStream.Write(0); // Empty checksum
            checksumStream.Write(segmentCount);
            foreach (var lacingValue in lacingValues)
            {
                checksumStream.Write(lacingValue);
            }
            checksumStream.Write(Data);

            var calculatedChecksum = crc.Value;
            if (checksum != calculatedChecksum)
            {
                throw new OggException($"Calculated checksum \"{calculatedChecksum}\" doesn't match obtained checksum \"{checksum}\".");
            }
        }

        public void Encode(Stream pageStream)
        {
            // Obtain data without checksum and add the checksum to it
            var data = EncodeWithoutChecksum();

            var crc = new OggCrc();
            crc.Update(data);
            var checksum = crc.Value;

            var dataStream = new MemoryStream(data)
            {
                Position = 4 + 1 + 1 + 8 + 4 + 4
            };

            var dataWriter = new BinaryWriter(dataStream);

            dataWriter.Write(checksum);

            dataStream.Position = 0;
            dataStream.CopyTo(pageStream);
        }

        private byte[] EncodeWithoutChecksum()
        {
            var pageStream = new MemoryStream();
            var pageWriter = new BinaryWriter(pageStream);

            pageWriter.Write(CapturePattern);
            pageWriter.Write(StreamStructureVersion);
            pageWriter.Write((byte)HeaderType);
            pageWriter.Write(AbsoluteGranulePosition);
            pageWriter.Write(StreamSerialNumber);
            pageWriter.Write(SequenceNumber);
            pageWriter.Write(0);

            var lacingValues = GetLacingValues();

            pageWriter.Write((byte)lacingValues.Length);

            foreach (var lacingValue in lacingValues)
            {
                pageWriter.Write(lacingValue);
            }

            pageWriter.Write(Data);

            return pageStream.ToArray();
        }

        private byte[] GetLacingValues()
        {
            var dataLength = Data.Length;
            var lacingValues = Enumerable.Repeat((byte)255, dataLength / 255).ToList();

            var remainder = (byte)(dataLength % 255);
            if (remainder > 0)
            {
                lacingValues.Add(remainder);
            }

            return lacingValues.ToArray();
        }
    }
}