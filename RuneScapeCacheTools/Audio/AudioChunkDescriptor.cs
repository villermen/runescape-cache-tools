namespace Villermen.RuneScapeCacheTools.Audio
{
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