using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Villermen.RuneScapeCacheTools.Audio
{
    public class VorbisReader
    {
        public VorbisReader(Stream stream)
        {
            DataStream = stream;
        }

        private Stream DataStream { get; }

        private int NextPageSequenceNumber { get; set; }

        public byte[] GetPageData()
        {
            var dataReader = new BinaryReader(DataStream);

            var capturePattern = dataReader.ReadBytes(4);
            if (!capturePattern.SequenceEqual(new byte[]{ 0x4f, 0x67, 0x67, 0x53 }))
            {
                throw new Exception("VorbisPageException, invalid capture pattern (magic number)");
            }

            var streamStructureVersion = dataReader.ReadByte();
            if (streamStructureVersion != 0x00)
            {
                throw new Exception("VorbisPageException, invalid stream structure version");
            }

            var headerTypeFlag = dataReader.ReadByte();
            var absoluteGranulePosition = dataReader.ReadInt64();
            var streamSerialNumber = dataReader.ReadInt32();

            var pageSequenceNumber = dataReader.ReadInt32();
            if (NextPageSequenceNumber++ != pageSequenceNumber)
            {
                throw new Exception("VorbisPageException, invalid sequence number");
            }

            var pageChecksum = dataReader.ReadInt32();
            var segmentCount = dataReader.ReadByte();

            var lacingValues = new byte[segmentCount];

            for (var segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                lacingValues[segmentIndex] = dataReader.ReadByte();
            }

            var packetLength = lacingValues.Aggregate(0, (total, addition) => total + addition);
            return dataReader.ReadBytes(packetLength);

            //var freshPacket = (headerTypeFlag & 1) == 0;

            //if (freshPacket)
            //{
            //    nthFreshPacket++;
            //}

            //// "The Vorbis text comment header is the second (of three) header packets that begin a Vorbis bitstream."
            //if (nthFreshPacket == 2)
            //{
            //    var packetReader = new BinaryReader(new MemoryStream(packetData));

            //    var vendorLength = packetReader.ReadUInt32();
            //    var vendorString = Encoding.UTF8.GetString(packetReader.ReadBytes((int)vendorLength));
            //    var userCommentListLength = packetReader.ReadUInt32();

            //    for (var userCommentIndex = 0; userCommentIndex < userCommentListLength; userCommentIndex++)
            //    {
            //        var userCommentLength = packetReader.ReadUInt32();
            //        var userComment = Encoding.UTF8.GetString(packetReader.ReadBytes((int)userCommentLength));
            //    }

            //    var framingBit = packetReader.ReadByte() << 7;
            //}
        }
    }
}