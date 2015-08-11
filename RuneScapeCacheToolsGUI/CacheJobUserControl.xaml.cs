using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RuneScapeCacheTools;

namespace RuneScapeCacheToolsGUI
{
	/// <summary>
	///     Interaction logic for CacheJobUserControl.xaml
	/// </summary>
	public partial class CacheJobUserControl
	{
		public readonly CacheJob Job;

		public CacheJobUserControl(CacheJob job)
		{
			InitializeComponent();

			Job = job;

			//bind events to control
			job.ProgressChanged += Job_ProgressChanged;
			job.Finished += Job_Finished;

			//job.Started += Job_Started;
			job.LogAdded += Job_LogAdded;
		}

		//private void Job_Started(CacheJob sender, EventArgs args)
		//{
		//	if (!Dispatcher.CheckAccess())
		//		Dispatcher.Invoke(() => Job_Started(sender, args));
		//}

		private void Job_Finished(CacheJob sender, EventArgs args)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(() => Job_Finished(sender, args));
				return;
			}

			Destroy();
		}

		private void Job_ProgressChanged(CacheJob sender, ProgressChangedEventArgs args)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(() => Job_ProgressChanged(sender, args));
				return;
			}

			progressBar.Value = args.Progress;
		}

		private void Job_LogAdded(CacheJob sender, string message)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(() => Job_LogAdded(sender, message));
				return;
			}

			statusLabel.Content = message;
		}

		private void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			//if job hasn't started yet, just dispose of it
			if (!Job.CanCancel)
				Destroy();
			else
				Job.Cancel();
		}

		/// <summary>
		///     Removes the control from it's parent panel.
		/// </summary>
		private void Destroy()
		{
			var parentControl = VisualTreeHelper.GetParent(this) as Panel;
			parentControl?.Children.Remove(this);
		}
	}
}
