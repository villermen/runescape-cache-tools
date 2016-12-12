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

            this.UnknownInteger1 = reader.ReadInt32BigEndian();
            this.UnknownInteger2 = reader.ReadInt32BigEndian();
            this.SampleFrequency = reader.ReadInt32BigEndian();
            this.UnknownInteger3 = reader.ReadInt32BigEndian();
            this.ChunkCount = reader.ReadInt32BigEndian();

            this.ChunkDescriptors = new AudioChunkDescriptor[this.ChunkCount];

            var position = (int)reader.BaseStream.Position + this.ChunkCount * 8;
            for (var chunkIndex = 0; chunkIndex < this.ChunkCount; chunkIndex++)
            {
                this.ChunkDescriptors[chunkIndex] = new AudioChunkDescriptor(position, reader.ReadInt32BigEndian(),
                    reader.ReadInt32BigEndian());

                position += this.ChunkDescriptors[chunkIndex].Length;
            }

            // The rest of the file is the first chunk
            var containedChunkStartPosition = reader.BaseStream.Position;
            this.ContainedChunkData = reader.ReadBytes((int)(reader.BaseStream.Length - containedChunkStartPosition));
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