using System;

namespace RuneScapeCacheTools
{
	public abstract class CacheJob
	{
		public bool IsStarted { get; protected set; }
		public bool IsFinished { get; protected set; }
		public bool IsCanceled { get; protected set; }

		public delegate void CacheJobEventHandler(CacheJob sender, EventArgs args);
		public delegate void CacheJobEventHandler<TEventArgs>(CacheJob sender, TEventArgs args);

		/// <summary>
		/// Will fire when a new job is created.
		/// </summary>
		public static event CacheJobEventHandler Created;

		public event CacheJobEventHandler Started;
		public event CacheJobEventHandler Finished;
		public event CacheJobEventHandler Canceled;

		/// <summary>
		/// Fires when progress on the current job has changed.
		/// </summary>
		public event CacheJobEventHandler<ProgressChangedEventArgs> ProgressChanged;

		/// <summary>
		/// Fires when a new message has been added to the log.
		/// </summary>
		public event CacheJobEventHandler<string> LogAdded;

		public void Cancel()
		{
			if (!CanCancel())
				throw new InvalidOperationException("Job must be cancelable.");

			IsCanceled = true;
		}

		public bool CanCancel()
		{
			return IsStarted && !IsFinished;
		}

		protected void OnJobCreated(CacheJob sender, EventArgs args)
		{
			Created?.Invoke(sender, args);
		}

		protected void OnStarted(CacheJob sender, EventArgs args)
		{
			Started?.Invoke(sender, args);
		}

		protected void OnFinished(CacheJob sender, EventArgs args)
		{
			Finished?.Invoke(sender, args);
		}

		protected void OnCanceled(CacheJob sender, EventArgs args)
		{
			Canceled?.Invoke(sender, args);
		}

		protected void OnProgressChanged(CacheJob sender, ProgressChangedEventArgs args)
		{
			ProgressChanged?.Invoke(sender, args);
		}

		protected void OnLogAdded(CacheJob sender, string args)
		{
			LogAdded?.Invoke(sender, args);
		}
	}
}