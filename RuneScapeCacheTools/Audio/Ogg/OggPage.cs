using System;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Audio.Vorbis;

namespace Villermen.RuneScapeCacheTools.Audio.Ogg
{
    public class OggPage
    {
        public static readonly byte[] CapturePattern = { 0x4F, 0x67, 0x67, 0x53 };
        public const byte StreamStructureVersion = 0x00;
        public const int MaxDataLength = 255 * 255;

        public static OggPage Decode(Stream pageStream)
        {
            var page = new OggPage();
            var pageReader = new BinaryReader(pageStream);

            var capturePattern = pageReader.ReadBytes(4);
            if (!capturePattern.SequenceEqual(OggPage.CapturePattern))
            {
                throw new OggException($"Invalid capture pattern \"0x{BitConverter.ToString(capturePattern)}\" (magic number).");
            }

            var streamStructureVersion = pageReader.ReadByte();
            if (streamStructureVersion != OggPage.StreamStructureVersion)
            {
                throw new VorbisException($"Invalid stream structure version \"{streamStructureVersion}\", only Vorbis I is supported.");
            }

            page.HeaderType = (VorbisPageHeaderType)pageReader.ReadByte();
            page.AbsoluteGranulePosition = pageReader.ReadInt64();

            page.StreamSerialNumber = pageReader.ReadInt32();
            page.SequenceNumber = pageReader.ReadInt32();

            var checksum = pageReader.ReadUInt32();

            var segmentCount = pageReader.ReadByte();
            var lacingValues = new byte[segmentCount];
            for (var segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                lacingValues[segmentIndex] = pageReader.ReadByte();
            }

            var dataLength = lacingValues.Aggregate(0, (total, addition) => total + addition);
            page.Data = pageReader.ReadBytes(dataLength);

            // Calculate checksum from the obtained values.
            // If the checksum is calculated from the reconstructed data, there might be inconsistencies (e.g. non-standard lacing values)
            var crc = new VorbisCrc();
            var checksumStream = new BinaryWriter(crc);
            checksumStream.Write(capturePattern);
            checksumStream.Write(streamStructureVersion);
            checksumStream.Write((byte)page.HeaderType);
            checksumStream.Write(page.AbsoluteGranulePosition);
            checksumStream.Write(page.StreamSerialNumber);
            checksumStream.Write(page.SequenceNumber);
            checksumStream.Write(0); // Empty checksum
            checksumStream.Write(segmentCount);
            foreach (var lacingValue in lacingValues)
            {
                checksumStream.Write(lacingValue);
            }
            checksumStream.Write(page.Data);

            var calculatedChecksum = crc.Value;
            if (checksum != calculatedChecksum)
            {
                throw new VorbisException($"Calculated checksum \"{calculatedChecksum}\" doesn't match obtained checksum \"{checksum}\".");
            }

            return page;
        }

        public void Encode(Stream pageStream)
        {
            // Obtain data without checksum and add the checksum to it
            var data = EncodeWithoutChecksum();

            var crc = new VorbisCrc();
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

            pageWriter.Write(OggPage.CapturePattern);
            pageWriter.Write(OggPage.StreamStructureVersion);
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
            lacingValues.Add((byte)(dataLength % 255));
            return lacingValues.ToArray();
        }

        public VorbisPageHeaderType HeaderType { get; private set; }
        public long AbsoluteGranulePosition { get; private set; }
        public int StreamSerialNumber { get; private set; }
        public int SequenceNumber { get; private set; }
        public int Checksum { get; private set; }

        private byte[] _data;

        public byte[] Data
        {
            get
            {
                return _data;
            }

            set
            {
                if (value.Length > OggPage.MaxDataLength)
                {
                    throw new VorbisException($"One page cannot contain more than {OggPage.MaxDataLength} bytes.");
                }

                _data = value;
            }
        }
    }
}
