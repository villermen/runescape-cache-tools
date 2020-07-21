using System;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.Audio
{
    /// <summary>
    /// A file that serves as a map to stitch audio chunks together in the right order while also containing the first chunk.
    /// </summary>
    public class JagaFile
    {
        public static readonly byte[] MagicNumber = { 0x4a, 0x41, 0x47, 0x41 };

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

        public override void Decode(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));

            // Verify magic number
            if (!reader.ReadBytes(4).SequenceEqual(JagaFile.MagicNumber))
            {
                throw new DecodeException("JAGA magic number incorrect");
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

        public override byte[] Encode()
        {
            throw new NotImplementedException("Encoding of JAGA files is not yet implemented.");
        }

        public class AudioChunkDescriptor
        {
            public AudioChunkDescriptor(int position, int length, int fileId)
            {
                this.Position = position;
                this.Length = length;
                this.FileId = fileId;
            }

            /// <summary>
            ///     The file id within the same index that contains the next chunk.
            ///     If 0, this is the file contained within the jaga file.
            /// </summary>
            public int FileId { get; }

            /// <summary>
            ///     The length in bytes of this chunk.
            /// </summary>
            public int Length { get; }

            /// <summary>
            ///     Position in bytes of the combined audio data.
            /// </summary>
            public int Position { get; }
        }
    }
}
