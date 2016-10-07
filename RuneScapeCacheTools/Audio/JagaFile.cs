using System.IO;
using System.Linq;
using System.Text;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Audio
{
    public class JagaFile
    {
        public static byte[] MagicNumber = Encoding.ASCII.GetBytes("JAGA");

        public JagaFile(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));

            // Verify magic number
            if (!reader.ReadBytes(4).SequenceEqual(MagicNumber))
            {
                throw new JagaParseException("Magic number incorrect");
            }

            UnknownInteger1 = reader.ReadInt32BigEndian();
            UnknownInteger2 = reader.ReadInt32BigEndian();
            SampleFrequency = reader.ReadInt32BigEndian();
            UnknownInteger3 = reader.ReadInt32BigEndian();
            ChunkCount = reader.ReadInt32BigEndian();

            ChunkDescriptors = new AudioChunkDescriptor[ChunkCount];

            var position = (int)reader.BaseStream.Position + ChunkCount * 8;
            for (var chunkIndex = 0; chunkIndex < ChunkCount; chunkIndex++)
            {
                ChunkDescriptors[chunkIndex] = new AudioChunkDescriptor(position, reader.ReadInt32BigEndian(),
                    reader.ReadInt32BigEndian());

                position += ChunkDescriptors[chunkIndex].Length;
            }

            // The rest of the file is the first chunk
            var containedChunkStartPosition = reader.BaseStream.Position;
            ContainedChunkData = reader.ReadBytes((int)(reader.BaseStream.Length - containedChunkStartPosition));
        }

        public int ChunkCount { get; }

        public AudioChunkDescriptor[] ChunkDescriptors { get; }

        public byte[] ContainedChunkData { get; }

        public int SampleFrequency { get; }

        public int UnknownInteger1 { get; }

        /// <summary>
        ///     Something to do with length?
        /// </summary>
        public int UnknownInteger2 { get; }

        public int UnknownInteger3 { get; }
    }
}