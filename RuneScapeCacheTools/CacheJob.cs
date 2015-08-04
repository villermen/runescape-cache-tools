using System;
using System.Threading.Tasks;

namespace RuneScapeCacheTools
{
	public abstract class CacheJob
	{
		private bool _isStarted;
		private bool _isFinished;
		private bool _isCanceled;

		public bool IsStarted
		{
			get
			{
				return _isStarted;
			}

			protected set
			{
				if (value && _isStarted)
					throw new InvalidOperationException("Job is already started.");

				if (!value)
					throw new InvalidOperationException("Cannot unstart job.");

				_isStarted = true;
				Started?.Invoke(this, EventArgs.Empty);
			}
		}

		public bool IsFinished
		{
			get
			{
				return _isFinished;
			}

			protected set
			{
				if (value && _isFinished)
					throw new InvalidOperationException("Job has already finished.");

				if (!value)
					throw new InvalidOperationException("Cannot unfinish job.");

				_isFinished = true;
				Finished?.Invoke(this, EventArgs.Empty);
			}
		}

		public bool IsCanceled
		{
			get
			{
				return _isCanceled;
			}

			protected set
			{
				if (value && _isCanceled)
					throw new InvalidOperationException("Job has already been canceled.");

				if (!value)
					throw new InvalidOperationException("Cannot uncancel job.");

				_isCanceled = true;
				Canceled?.Invoke(this, EventArgs.Empty);
			}
		}

		public bool IsRunning => IsStarted && !IsFinished;

		public bool CanCancel => IsRunning;

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

		protected CacheJob()
		{
			Created?.Invoke(this, EventArgs.Empty);
		}

		public void Start()
		{
			StartAsync().ConfigureAwait(true);
		}

		public abstract Task StartAsync();

		public void Cancel()
		{
			if (!CanCancel)
				throw new InvalidOperationException("Job must be cancelable.");

			IsCanceled = true;
		}

		protected void ReportProgress(int done, int total)
		{
			ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(done, total));
		}

		protected void Log(string args)
		{
			LogAdded?.Invoke(this, args);
		}
	}
}