using System.IO;
using System.Linq;
using System.Text;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Audio
{
    public class JagaFile : CacheFile
    {
        public static byte[] MagicNumber = Encoding.ASCII.GetBytes("JAGA");

        public int ChunkCount { get; set; }

        public AudioChunkDescriptor[] ChunkDescriptors { get; set; }

        public byte[] ContainedChunkData { get; set; }

        public int SampleFrequency { get; set; }

        public int UnknownInteger1 { get; set; }

        /// <summary>
        ///     Something to do with length?
        /// </summary>
        public int UnknownInteger2 { get; set; }

        public int UnknownInteger3 { get; set; }

        public static explicit operator JagaFile(DataCacheFile dataFile)
        {
            var jagaFile = new JagaFile
            {
                Info = dataFile.Info
            };

            var reader = new BinaryReader(new MemoryStream(dataFile.Data));

            // Verify magic number
            if (!reader.ReadBytes(4).SequenceEqual(JagaFile.MagicNumber))
            {
                throw new DecodeException("JAGA magic number incorrect");
            }

            jagaFile.UnknownInteger1 = reader.ReadInt32BigEndian();
            jagaFile.UnknownInteger2 = reader.ReadInt32BigEndian();
            jagaFile.SampleFrequency = reader.ReadInt32BigEndian();
            jagaFile.UnknownInteger3 = reader.ReadInt32BigEndian();
            jagaFile.ChunkCount = reader.ReadInt32BigEndian();

            jagaFile.ChunkDescriptors = new AudioChunkDescriptor[jagaFile.ChunkCount];

            var position = (int)reader.BaseStream.Position + jagaFile.ChunkCount * 8;
            for (var chunkIndex = 0; chunkIndex < jagaFile.ChunkCount; chunkIndex++)
            {
                jagaFile.ChunkDescriptors[chunkIndex] = new AudioChunkDescriptor(position, reader.ReadInt32BigEndian(),
                    reader.ReadInt32BigEndian());

                position += jagaFile.ChunkDescriptors[chunkIndex].Length;
            }

            // The rest of the file is the first chunk
            var containedChunkStartPosition = reader.BaseStream.Position;
            jagaFile.ContainedChunkData = reader.ReadBytes((int)(reader.BaseStream.Length - containedChunkStartPosition));

            return jagaFile;
        }
    }
}