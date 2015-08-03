using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using RuneScapeCacheTools;
using WinForms = System.Windows.Forms;

namespace RuneScapeCacheToolsGUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private bool _canExtract;

		public MainWindow()
		{
			InitializeComponent();

			//initialize view
			cacheDirectoryTextBox.Text = Cache.CacheDirectory;
			outputDirectoryTextBox.Text = Cache.OutputDirectory;
			UpdateCacheView();

			CacheExtractJob.JobCreated += RegisterNewJob;
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

		private void changeCacheDirectoryButton_Click(object sender, RoutedEventArgs e)
		{
			var dirDialog = new WinForms.FolderBrowserDialog
			{
				RootFolder = Environment.SpecialFolder.Desktop,
				SelectedPath = Cache.CacheDirectory.Replace("/", "\\") //because windows...
			};

			var dialogResult = dirDialog.ShowDialog();

			if (dialogResult == WinForms.DialogResult.OK)
				Cache.CacheDirectory = dirDialog.SelectedPath;

			UpdateCacheView();
		}

		private void changeOutputDirectoryButton_Click(object sender, RoutedEventArgs e)
		{
			var dirDialog = new WinForms.FolderBrowserDialog
			{
				RootFolder = Environment.SpecialFolder.Desktop,
				SelectedPath = Cache.OutputDirectory.Replace("/", "\\") //because windows...
			};

			var dialogResult = dirDialog.ShowDialog();

			if (dialogResult == WinForms.DialogResult.OK)
				Cache.OutputDirectory = dirDialog.SelectedPath;

			outputDirectoryTextBox.Text = Cache.OutputDirectory;
		}

		private void UpdateCacheView()
		{
			cacheDirectoryTextBox.Text = Cache.CacheDirectory;

			cacheTreeView.Items.Clear();

			//show error if cache not detected
			if (!File.Exists(Cache.CacheDirectory + Cache.CacheFileName))
			{
				cacheTreeView.Items.Add("No cache file found in directory.");
				_canExtract = false;
				return;
			}

			//fill the treeview
			var cacheTreeItem = new TreeViewItem
			{
				Header = "Cache",
				Tag = -1,
				ContextMenu = (ContextMenu)Resources["ExtractContextMenu"]
			};
			cacheTreeItem.MouseDoubleClick += cacheTreeViewItem_DoubleClick;

			cacheTreeItem.ExpandSubtree();

			foreach (int archiveId in Cache.GetArchiveIds())
			{
				var archiveTreeItem = new TreeViewItem
				{
					Header = $"Archive {archiveId}", 
					Tag = archiveId,
					ContextMenu = (ContextMenu)Resources["ExtractContextMenu"]
				};

				archiveTreeItem.MouseDoubleClick += cacheTreeViewItem_DoubleClick;

				cacheTreeItem.Items.Add(archiveTreeItem);
			}

			_canExtract = true;
			cacheTreeView.Items.Add(cacheTreeItem);
		}

		/// <summary>
		/// Returns the archive id of the tree view item the context menu is bound to.
		/// </summary>
		private static int GetArchiveIdFromContextItem(object sender)
		{
			//get the treeview item the contextmenu is bound to
			var menuItem = (MenuItem)sender;
			var contextMenu = (ContextMenu)menuItem.Parent;
			var treeViewItem = (TreeViewItem)contextMenu.PlacementTarget;
			return (int)treeViewItem.Tag;
		}

		private void showOutputDirectoryMenuItem_Click(object sender, RoutedEventArgs e)
		{
			int selectedArchive = GetArchiveIdFromContextItem(sender);
			OpenArchiveDirectory(selectedArchive);
		}

		private async void extractDefaultToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int selectedArchive = GetArchiveIdFromContextItem(sender);
			await StartExtractionAsync(selectedArchive, false);
		}

		private async void extractOverwriteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int selectedArchive = GetArchiveIdFromContextItem(sender);
			await StartExtractionAsync(selectedArchive, true);
		}

		private void showOutputDirectoryButton_Click(object sender, RoutedEventArgs e)
		{
			Process.Start(Cache.OutputDirectory);
		}

		private void OpenArchiveDirectory(int archiveId)
		{
			string openFile = archiveId == -1 
				? Cache.OutputDirectory + "cache/"
				: Cache.OutputDirectory + "cache/" + archiveId + "/";

			if (!File.Exists(openFile) && !Directory.Exists(openFile))
				MessageBox.Show("That location hasn't been extracted yet.\n" + openFile);
			else
				Process.Start(openFile);
		}

		private async Task StartExtractionAsync(int archiveId, bool overwrite)
		{
			if (!_canExtract)
			{
				MessageBox.Show("Cannot extract at this time.");
				return;
			}

			var job = archiveId == -1 
				? new CacheExtractJob(overwrite) 
				: new CacheExtractJob(archiveId, overwrite);

			await job.StartAsync();
		}

		/// <summary>
		/// Invoke default contextmenu event when treeview is double clicked.
		/// </summary>
		private void cacheTreeViewItem_DoubleClick(object sender, EventArgs e)
		{
			var treeViewItem = (TreeViewItem)sender;

			//parents will also be triggered, make sure only the source will be fired
			if (!treeViewItem.IsSelected)
				return;

			treeViewItem.ContextMenu.PlacementTarget = treeViewItem;
			var menuItem = (MenuItem)treeViewItem.ContextMenu.Items[0];

			menuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
		}

		/// <summary>
		/// Creates a new user control for the job and adds it to the stackpanel
		/// </summary>
		private void RegisterNewJob(CacheExtractJob sender, EventArgs args)
		{
			var userControl = new ExtractJobUserControl(sender);
			jobsStackPanel.Children.Add(userControl);
		}

		private void defaultCacheDirectoryButton_Click(object sender, RoutedEventArgs e)
		{
			Cache.CacheDirectory = Cache.DefaultCacheDirectory;
			UpdateCacheView();
		}
	}
}
