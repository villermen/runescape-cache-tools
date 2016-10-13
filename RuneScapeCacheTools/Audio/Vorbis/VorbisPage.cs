using System;
using System.IO;
using System.Linq;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    public class VorbisPage
    {
        public static readonly byte[] CapturePattern = { 0x4F, 0x67, 0x67, 0x53 };
        public const byte StreamStructureVersion = 0x00;

        public static VorbisPage Decode(Stream pageStream)
        {
            var page = new VorbisPage();

            var pageReader = new BinaryReader(pageStream);

            var capturePattern = pageReader.ReadBytes(4);
            if (!capturePattern.SequenceEqual(CapturePattern))
            {
                throw new Exception($"Invalid capture pattern \"0x{BitConverter.ToString(capturePattern)}\" (magic number).");
            }

            var streamStructureVersion = pageReader.ReadByte();
            if (streamStructureVersion != StreamStructureVersion)
            {
                throw new VorbisException($"Invalid stream structure version \"{streamStructureVersion}\", only Vorbis I is supported.");
            }

            page.HeaderType = (VorbisPageHeaderType)pageReader.ReadByte();
            page.AbsoluteGranulePosition = pageReader.ReadInt64();

            page.StreamSerialNumber = pageReader.ReadInt32();
            page.SequenceNumber = pageReader.ReadInt32();

            page.Checksum = pageReader.ReadInt32(); // TODO: Verify Vorbis page checksum

            var segmentCount = pageReader.ReadByte();
            var lacingValues = new byte[segmentCount];
            for (var segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                lacingValues[segmentIndex] = pageReader.ReadByte();
            }

            var packetLength = lacingValues.Aggregate(0, (total, addition) => total + addition);
            page.Data = pageReader.ReadBytes(packetLength);

            return page;
        }

        public VorbisPageHeaderType HeaderType { get; private set; }
        public long AbsoluteGranulePosition { get; private set; }
        public int StreamSerialNumber { get; private set; }
        public int SequenceNumber { get; private set; }
        public int Checksum { get; private set; }
        public byte[] Data { get; private set; }
    }
}
