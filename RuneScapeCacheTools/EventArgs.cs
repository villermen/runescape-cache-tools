using System;

namespace RuneScapeCacheTools
{
	public class ProgressChangedEventArgs : EventArgs
	{
		public readonly int Done;
		public readonly float Progress;
		public readonly int Total;

		public ProgressChangedEventArgs(int done, int total)
		{
			Done = done;
			Total = total;

			Progress = 100f / total * done;
		}
	}
}
