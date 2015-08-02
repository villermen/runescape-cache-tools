using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RunescapeCacheTools;

namespace RunescapeCacheToolsGUI
{
	public partial class MainForm : Form
	{
		private bool _canExtract;

		public MainForm()
		{
			InitializeComponent();

			//initialize view
			cacheDirectoryTextBox.Text = Cache.CacheDirectory;
			outputDirectoryTextBox.Text = Cache.OutputDirectory;
			UpdateCacheView();

			CacheExtractJob.JobCreated += RegisterNewJob;

			new CacheExtractJob();
			new CacheExtractJob();
			new CacheExtractJob();
			new CacheExtractJob();
			new CacheExtractJob();
		}

		/// <summary>
		/// Exports and shows obtained track names.
		/// </summary>
		private void exportNamesSoundtrackToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				//obtain track list
				var tracks = Soundtrack.GetTrackNames();

				//export to file
				using (var tracklistFile = new StreamWriter(File.Open(Cache.OutputDirectory + "tracknames.csv", FileMode.Create)))
				{
					//write headers
					
					tracklistFile.WriteLine("File Id,Name");

					foreach (var track in tracks)
						tracklistFile.WriteLine($"{track.Key},\"{track.Value}\"");
				}

				//show file
				Process.Start(Cache.OutputDirectory + "tracknames.csv");
			}
			catch (Exception ex)
			{
				DisplayError("Could not create track list.", ex);
			}
		}

		private void showMissingTracksToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				string namedSoundtrackDir = Cache.OutputDirectory + "soundtrack/named/";
				var tracks = Soundtrack.GetTrackNames();

				List<string> missingTracks = new List<string>();

				foreach (var track in tracks)
				{
					if (!Directory.EnumerateFiles(namedSoundtrackDir, track.Value + ".*").Any())
						missingTracks.Add(track.Value);
				}

				MessageBox.Show("The following tracks are missing: \n" + missingTracks.Aggregate((acc, track) => acc + track + "\n"));
			}
			catch (Exception ex)
			{			
				DisplayError("Could not create missing track list.", ex);
			}
		}

		private static void DisplayError(string message, Exception exception)
		{
			//show innerexception if it's an aggregate exception
			if (exception is AggregateException)
				exception = exception.InnerException;

			MessageBox.Show($"{message}\n{exception.GetType().FullName}: {exception.Message}");
		}

		private void changeCacheDirectoryButton_Click(object sender, EventArgs e)
		{
			var dirDialog = new FolderBrowserDialog
			{
				RootFolder = Environment.SpecialFolder.Desktop,
				SelectedPath = Cache.CacheDirectory.Replace("/", "\\") //because windows...
			};

			var dialogResult = dirDialog.ShowDialog();

			if (dialogResult == DialogResult.OK)
				Cache.CacheDirectory = dirDialog.SelectedPath;

			cacheDirectoryTextBox.Text = Cache.CacheDirectory;

			UpdateCacheView();
		}

		private void changeOutputDirectoryButton_Click(object sender, EventArgs e)
		{
			var dirDialog = new FolderBrowserDialog
			{
				RootFolder = Environment.SpecialFolder.Desktop,
				SelectedPath = Cache.OutputDirectory.Replace("/", "\\") //because windows...
			};
			
			var dialogResult = dirDialog.ShowDialog();

			if (dialogResult == DialogResult.OK)
				Cache.OutputDirectory = dirDialog.SelectedPath;

			outputDirectoryTextBox.Text = Cache.OutputDirectory;
		}

		private void UpdateCacheView()
		{
			cacheTreeView.Nodes.Clear();

			//show error if cache not detected
			if (!File.Exists(Cache.CacheDirectory + Cache.CacheFileName))
			{
				cacheTreeView.Nodes.Add("No cache file found in directory.");
				_canExtract = false;
				return;
			}

			//fill the treeview
			var cacheNode = cacheTreeView.Nodes.Add("Cache");
			cacheNode.Tag = -1;
			cacheNode.Expand();
			cacheNode.ContextMenuStrip = extractContextMenuStrip;

			foreach (int archiveId in Cache.GetArchiveIds())
			{
				var archiveNode = cacheNode.Nodes.Add("Archive " + archiveId);
				archiveNode.Tag = archiveId;
				archiveNode.ContextMenuStrip = extractContextMenuStrip;
			}

			_canExtract = true;
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TreeNode selectedNode = cacheTreeView.SelectedNode;

			string openFile = "";

			if (selectedNode.Level == 0)
				openFile = Cache.OutputDirectory;
			else if (selectedNode.Level == 1)
				openFile = Cache.OutputDirectory + selectedNode.Tag;

			if (!File.Exists(openFile) && !Directory.Exists(openFile))
			{
				MessageBox.Show("That part has not been extracted yet.\n" + openFile);
				return;
			}

			Process.Start(openFile);
		}

		private async void extractDefaultToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await StartExtractionAsync(false);
		}

		private async void extractOverwriteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			await StartExtractionAsync(true);
		}

		private void cacheTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			//set the selectednode on right click, so that the contextmenu knows what's up
			if (e.Button == MouseButtons.Right)
			{
				cacheTreeView.SelectedNode = e.Node;
			}
		}

		private void showOutputDirectoryButton_Click(object sender, EventArgs e)
		{
			Process.Start(Cache.OutputDirectory);
		}

		private async Task StartExtractionAsync(bool overwrite)
		{
			if (!_canExtract)
			{
				MessageBox.Show("Cannot extract at this time.");
				return;
			}

			TreeNode selectedNode = cacheTreeView.SelectedNode;

			CacheExtractJob job;

			switch (selectedNode.Level)
			{
				case 0:
					job = new CacheExtractJob(overwrite);
					break;

				case 1:
					job = new CacheExtractJob((int)selectedNode.Tag, overwrite);
					break;

				default:
					throw new InvalidOperationException("Invalid extract source.");
			}

			await job.StartAsync();
		}

		/*private void ExtractionCompleted(Task task)
		{
			if (extractProgressBar.InvokeRequired)
			{
				Invoke(new Action(() => extractProgressBar.Value = 0));
			}
			else
				extractProgressBar.Value = 0;
			
			_canExtract = true;
			AddLogMessage("Extraction completed.");
		}*/

		private void cacheTreeView_DoubleClick(object sender, EventArgs e)
		{
			//run 'open' context menu command if a valid node is selected
			if (cacheTreeView.SelectedNode.Tag is int)
				openToolStripMenuItem_Click(sender, e);
		}

		/// <summary>
		/// Cleans up old job events, and registers gui to new job's events.
		/// </summary>
		private void RegisterNewJob(CacheExtractJob sender, EventArgs args)
		{
			//construct job gui
			var jobControl = new ExtractJobUserControl();

			//register job to gui
			//sender.Started += (sender2, args2) => jobPanel.Text = "Started.";
			//sender.Canceled += (sender2, args2) => jobPanel.Text = "Canceled.";
			//sender.Finished += (sender2, args2) => jobPanel.Text = "Finished.";
			//sender.ProgressChanged += (sender2, args2) => jobPanel.Invoke((Action)(() =>
			//	jobStatus.Text = $"{args2.ArchiveId}/{args2.FileId}" ));

			//add panel to the form
			jobsTableLayoutPanel.Controls.Add(jobControl);

			//jobControl.Size = new Size(jobsFlowLayoutPanel.Size.Width, jobControl.Size.Height);
		}

		private void button1_Click(object sender, EventArgs e)
		{

		}
	}
}
