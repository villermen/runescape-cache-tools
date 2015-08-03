using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RuneScapeCacheTools;

namespace RuneScapeCacheToolsGUI
{
	/// <summary>
	/// Interaction logic for CacheJobUserControl.xaml
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
			job.Started += Job_Started;
			job.LogAdded += Job_LogAdded;
		}

		private void Job_Started(CacheJob sender, EventArgs args)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(() => Job_Started(sender, args));
				return;
			}

			//set action button to cancel
			actionButton.Content = "Cancel";
		}

		private void Job_Finished(CacheJob sender, EventArgs args)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(() => Job_Finished(sender, args));
				return;
			}

			//change action button into close button
			statusLabel.Content = "Finished";
			actionButton.Content = "Close";
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

		private void actionButton_Click(object sender, RoutedEventArgs e)
		{
			//take action depending on Content
			switch ((string)actionButton.Content)
			{
				case "Cancel":
					Job.Cancel();
					break;

				case "Close":
					//remove from parent
					var parentControl = VisualTreeHelper.GetParent(this) as Panel;
					parentControl?.Children.Remove(this);
					break;
			}
		}
	}
}
