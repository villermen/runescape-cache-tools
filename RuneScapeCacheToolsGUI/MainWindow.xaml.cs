 using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using RuneScapeCacheTools;
using WinForms = System.Windows.Forms;

namespace RuneScapeCacheToolsGUI
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private bool _canExtract;
		private Dictionary<string, object> _config = new Dictionary<string, object>();

		public MainWindow()
		{
			LoadConfig();

			InitializeComponent();

			//initialize view
			cacheDirectoryTextBox.Text = LegacyCache.CacheDirectory;
			outputDirectoryTextBox.Text = LegacyCache.OutputDirectory;
			UpdateCacheView();

			CacheJob.Created += RegisterNewJob;
		}

		private static void DisplayError(Exception exception, string message = null)
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
				SelectedPath = LegacyCache.CacheDirectory.Replace("/", "\\") //because windows...
			};

			var dialogResult = dirDialog.ShowDialog();

			if (dialogResult == WinForms.DialogResult.OK)
				LegacyCache.CacheDirectory = dirDialog.SelectedPath;

			_config["cacheDirectory"] = LegacyCache.CacheDirectory;

			SaveConfig();

			UpdateCacheView();
		}

		private void changeOutputDirectoryButton_Click(object sender, RoutedEventArgs e)
		{
			var dirDialog = new WinForms.FolderBrowserDialog
			{
				RootFolder = Environment.SpecialFolder.Desktop,
				SelectedPath = LegacyCache.OutputDirectory.Replace("/", "\\") //because windows...
			};

			var dialogResult = dirDialog.ShowDialog();

			if (dialogResult == WinForms.DialogResult.OK)
				LegacyCache.OutputDirectory = dirDialog.SelectedPath;

			outputDirectoryTextBox.Text = LegacyCache.OutputDirectory;

			_config["outputDirectory"] = LegacyCache.OutputDirectory;

			SaveConfig();
		}

		private void UpdateCacheView()
		{
			cacheDirectoryTextBox.Text = LegacyCache.CacheDirectory;

			cacheTreeView.Items.Clear();

			//show error if cache not detected
			if (!File.Exists(LegacyCache.CacheDirectory + LegacyCache.CacheFileName))
			{
				cacheTreeView.Items.Add("No cache file found in directory.");
				_canExtract = false;
				return;
			}

			//fill the treeview
			var cacheTreeItem = new TreeViewItem
			{
				Header = "CacheMehRename",
				Tag = -1,
				ContextMenu = (ContextMenu)Resources["ExtractContextMenu"]
			};
			cacheTreeItem.MouseDoubleClick += cacheTreeViewItem_DoubleClick;

			cacheTreeItem.ExpandSubtree();

			foreach (var archiveId in LegacyCache.GetArchiveIds())
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
		///     Returns the archive id of the tree view item the context menu is bound to.
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
			var selectedArchive = GetArchiveIdFromContextItem(sender);
			OpenArchiveDirectory(selectedArchive);
		}

		private async void extractDefaultToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var selectedArchive = GetArchiveIdFromContextItem(sender);
			await StartExtractionAsync(selectedArchive, false);
		}

		private async void extractOverwriteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var selectedArchive = GetArchiveIdFromContextItem(sender);
			await StartExtractionAsync(selectedArchive, true);
		}

		private void showOutputDirectoryButton_Click(object sender, RoutedEventArgs e)
		{
			Process.Start(LegacyCache.OutputDirectory);
		}

		private void OpenArchiveDirectory(int archiveId)
		{
			var openFile = archiveId == -1
			? LegacyCache.OutputDirectory + "cache/"
			: LegacyCache.OutputDirectory + "cache/" + archiveId + "/";

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

			var job = archiveId == -1 ? new CacheExtractJob(overwrite) : new CacheExtractJob(archiveId, overwrite);

			await job.StartAsync();
		}

		/// <summary>
		///     Invoke default contextmenu event when treeview is double clicked.
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
		///     Creates a new user control for the job and adds it to the stackpanel
		/// </summary>
		private void RegisterNewJob(CacheJob sender, EventArgs args)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(() => RegisterNewJob(sender, args));
				return;
			}

			var userControl = new CacheJobUserControl(sender);
			jobsStackPanel.Children.Add(userControl);
		}

		private void defaultCacheDirectoryButton_Click(object sender, RoutedEventArgs e)
		{
			LegacyCache.CacheDirectory = LegacyCache.DefaultCacheDirectory;
			UpdateCacheView();
		}

		private void SaveConfig()
		{
			File.WriteAllText(LegacyCache.TempDirectory + "config.json", JsonConvert.SerializeObject(_config));
		}

		private void LoadConfig()
		{
			try
			{
				_config =
				JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(LegacyCache.TempDirectory + "config.json"));

				if (_config.ContainsKey("outputDirectory"))
				{
					string outputDirectory = (string)_config["outputDirectory"];
					if (Directory.Exists(outputDirectory))
						LegacyCache.OutputDirectory = outputDirectory;
				}

				if (_config.ContainsKey("cacheDirectory"))
				{
					string cacheDirectory = (string)_config["cacheDirectory"];
					if (Directory.Exists(cacheDirectory))
						LegacyCache.CacheDirectory = cacheDirectory;
				}
			}
			catch (FileNotFoundException) { }
		}

		private async void createSoundtrackDefaultMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (!_canExtract)
			{
				MessageBox.Show("Cannot extract at this time.");
				return;
			}

			try
			{
				await new SoundtrackCombineJob().StartAsync();
			}
			catch (Exception ex)
			{
				DisplayError(ex);
			}
		}

		private async void createSoundtrackLosslessMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (!_canExtract)
			{
				MessageBox.Show("Cannot extract at this time.");
				return;
			}

			try
			{
				await new SoundtrackCombineJob(true, true).StartAsync();
			}
			catch (Exception ex)
			{
				DisplayError(ex);
			}
		}

		/// <summary>
		///     Exports and shows obtained track names.
		/// </summary>
		private void showSoundtrackNamesMenuItem_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				//obtain track list
				var tracks = Soundtrack.GetTrackNames();

				//export to file
				using (var tracklistFile = new StreamWriter(File.Open(LegacyCache.OutputDirectory + "tracknames.csv", FileMode.Create)))
				{
					//write headers

					tracklistFile.WriteLine("File Id,Name");

					foreach (var track in tracks)
						tracklistFile.WriteLine($"{track.Key},\"{track.Value}\"");
				}

				//show file
				Process.Start(LegacyCache.OutputDirectory + "tracknames.csv");
			}
			catch (Exception ex)
			{
				DisplayError(ex, "Could not create track list.");
			}
		}

		private void showMissingSoundtrackNamesMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (!Directory.Exists(LegacyCache.OutputDirectory + "soundtrack/"))
			{
				MessageBox.Show("Soundtrack directory hasn't been created yet.");
				return;
			}

			try
			{
				var soundtrackDir = LegacyCache.OutputDirectory + "soundtrack/";
				var tracks = Soundtrack.GetTrackNames();

				var missingTracks = new Dictionary<int, string>();

				foreach (var track in tracks)
				{
					if (!Directory.EnumerateFiles(soundtrackDir, track.Value + ".*").Any())
						missingTracks.Add(track.Key, track.Value);
				}

				//export to file
				using (
				var missingTracklistFile =
				new StreamWriter(File.Open(LegacyCache.OutputDirectory + "missingtracknames.csv", FileMode.Create)))
				{
					//write headers
					missingTracklistFile.WriteLine("File Id,Name");

					foreach (var track in missingTracks)
						missingTracklistFile.WriteLine($"{track.Key},\"{track.Value}\"");
				}

				//show file
				Process.Start(LegacyCache.OutputDirectory + "missingtracknames.csv");
			}
			catch (Exception ex)
			{
				DisplayError(ex, "Could not create missing track list.");
			}
		}

		private void showSoundtrackDirectoryMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (!Directory.Exists(LegacyCache.OutputDirectory + "soundtrack/"))
			{
				MessageBox.Show("Soundtrack directory hasn't been created yet.");
				return;
			}

			Process.Start(LegacyCache.OutputDirectory + "soundtrack/");
		}

		private void updateSoundtrackNamesMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (!Directory.Exists(LegacyCache.OutputDirectory + "soundtrack/"))
			{
				MessageBox.Show("Soundtrack directory hasn't been created yet.");
				return;
			}

			try
			{
				var soundtrackDir = LegacyCache.OutputDirectory + "soundtrack/";
				var tracks = Soundtrack.GetTrackNames();

				var namedTracks = 0;

				foreach (var trackPair in tracks)
				{
					var foundFiles = Directory.GetFiles(soundtrackDir, trackPair.Key + ".*");

					if (foundFiles.Length > 0)
					{
						var extension = Path.GetExtension(foundFiles[0]);
						var destination = soundtrackDir + trackPair.Value + extension;

						//delete destination if it exists
						if (File.Exists(destination))
							File.Delete(destination);

						File.Move(foundFiles[0], destination);

						namedTracks++;
					}
				}

				MessageBox.Show($"Named {namedTracks} tracks.");
			}
			catch (Exception ex)
			{
				DisplayError(ex, "Could not update all track names.");
			}
		}
	}
}
