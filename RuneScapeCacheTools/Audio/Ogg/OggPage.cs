using System;
using System.Collections.Generic;
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
        public byte[] LacingValues { get; private set; }

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
                throw new OggException($"Invalid stream structure version \"{streamStructureVersion}\", only Vorbis I is supported.");
            }

            HeaderType = (VorbisPageHeaderType)pageReader.ReadByte();
            AbsoluteGranulePosition = pageReader.ReadInt64();

            StreamSerialNumber = pageReader.ReadInt32();
            SequenceNumber = pageReader.ReadInt32();

            var checksum = pageReader.ReadUInt32();

            var segmentCount = pageReader.ReadByte();
            LacingValues = new byte[segmentCount];
            for (var segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                LacingValues[segmentIndex] = pageReader.ReadByte();
            }

            var dataLength = LacingValues.Aggregate(0, (total, addition) => total + addition);
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
            foreach (var lacingValue in LacingValues)
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

        /// <summary>
        ///     Converts the data into a page, or multiple pages if the packet exceeds the maximum page length.
        ///     The pages will only have their data set.
        ///     Further details necessary for writing to a stream like sequence numbers must still be added.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<OggPage> FromPacket(byte[] data)
        {
            // TODO: Change this

            var dataStream = new MemoryStream(data);
            var dataReader = new BinaryReader(dataStream);

            do
            {
                var remainingLength = (int)(dataStream.Length - dataStream.Position);

                var pageDataLength = Math.Min(remainingLength, MaxDataLength);

                yield return new OggPage
                {
                    Data = dataReader.ReadBytes(pageDataLength)
                };

                // Add an extra empty "terminator" page when there hasn't been a lacing value lower than 255
                if (remainingLength == MaxDataLength)
                {
                   yield return new OggPage();
                }
            }
            while (dataStream.Length - dataStream.Position > 0);
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

            if (LacingValues == null)
            {
                CalculateLacingValues();
            }

            pageWriter.Write((byte)LacingValues.Length);

            foreach (var lacingValue in LacingValues)
            {
                pageWriter.Write(lacingValue);
            }

            pageWriter.Write(Data);

            return pageStream.ToArray();
        }

        private void CalculateLacingValues()
        {
            var dataLength = Data.Length;
            var lacingValues = Enumerable.Repeat((byte)255, dataLength / 255).ToList();

            var remainder = (byte)(dataLength % 255);

            // If the data length is the maximum allowed, a zero length page must be added
            if (dataLength < MaxDataLength)
            {
                lacingValues.Add(remainder);
            }

            LacingValues = lacingValues.ToArray();
        }
    }
}