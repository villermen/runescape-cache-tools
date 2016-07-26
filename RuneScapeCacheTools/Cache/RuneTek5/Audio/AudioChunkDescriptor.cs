namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5.Audio
{
	public class AudioChunkDescriptor
	{
		public AudioChunkDescriptor(int position, int length, int fileId)
		{
			Position = position;
			Length = length;
			FileId = fileId;
		}

		public int Position { get; }

		public int Length { get; }

		public int FileId { get; }
	}
}