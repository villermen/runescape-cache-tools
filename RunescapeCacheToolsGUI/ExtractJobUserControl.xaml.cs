using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RunescapeCacheTools;

namespace RunescapeCacheToolsGUI
{
	/// <summary>
	/// Interaction logic for ExtractJobUserControl.xaml
	/// </summary>
	public partial class ExtractJobUserControl
	{
		public readonly CacheExtractJob Job;

		public ExtractJobUserControl(CacheExtractJob job)
		{
			InitializeComponent();

			Job = job;

			//bind events to control
			job.ProgressChanged += Job_ProgressChanged;
			job.Finished += Job_Finished;
			job.Started += Job_Started;
		}

		private void Job_Started(CacheExtractJob sender, EventArgs args)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(() => Job_Started(sender, args));
				return;
			}

			//set action button to cancel
			actionButton.Content = "Cancel";
		}

		private void Job_Finished(CacheExtractJob sender, EventArgs args)
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

		private void Job_ProgressChanged(CacheExtractJob sender, ExtractProgressChangedEventArgs args)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(() => Job_ProgressChanged(sender, args));
				return;
			}

			statusLabel.Content = $"{args.ArchiveId}/{args.FileId}";
			progressBar.Value = args.Progress;
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
