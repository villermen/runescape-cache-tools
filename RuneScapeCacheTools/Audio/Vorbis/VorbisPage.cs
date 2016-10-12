using System;
using System.IO;
using System.Linq;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    public class VorbisPage
    {
        public static VorbisPage Decode(Stream pageStream)
        {
            var pageReader = new BinaryReader(pageStream);

            var capturePattern = pageReader.ReadBytes(4);
            if (!capturePattern.SequenceEqual(new byte[] { 0x4F, 0x67, 0x67, 0x53 }))
            {
                throw new Exception("VorbisPageException, invalid capture pattern (magic number)");
            }

            var streamStructureVersion = pageReader.ReadByte();
            if (streamStructureVersion != 0x00)
            {
                throw new Exception("VorbisPageException, invalid stream structure version");
            }

            var headerTypeFlag = pageReader.ReadByte();
            var absoluteGranulePosition = pageReader.ReadInt64();
            var streamSerialNumber = pageReader.ReadInt32();

            var pageSequenceNumber = pageReader.ReadInt32();
            // TODO: compare sequence numbers in reader
            //if (NextPageSequenceNumber++ != pageSequenceNumber)
            //{
            //    throw new Exception("VorbisPageException, invalid sequence number");
            //}

            var pageChecksum = pageReader.ReadInt32();
            var segmentCount = pageReader.ReadByte();

            var lacingValues = new byte[segmentCount];

            for (var segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                lacingValues[segmentIndex] = pageReader.ReadByte();
            }

            var packetLength = lacingValues.Aggregate(0, (total, addition) => total + addition);
            var pageData = pageReader.ReadBytes(packetLength);

            throw new NotImplementedException();
        }
    }
}
