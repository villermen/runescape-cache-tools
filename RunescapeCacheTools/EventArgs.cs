using System;

namespace RunescapeCacheTools
{
	public class ExtractProgressChangedEventArgs : EventArgs
	{
		public readonly int ArchiveId;
		public readonly int FileId;
		public readonly int Done;
		public readonly int Total;
		public readonly float Progress;

		public ExtractProgressChangedEventArgs(int archiveId, int fileId, int done, int total)
		{
			ArchiveId = archiveId;
			FileId = fileId;
			Done = done;
			Total = total;

			Progress = 100f / total * done;
		}
	}
}
