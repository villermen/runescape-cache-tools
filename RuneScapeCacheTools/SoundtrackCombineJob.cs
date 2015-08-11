using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RuneScapeCacheTools
{
	/// <summary>
	/// Job for concatenating the sound files (.jaga &amp; .ogg) of archive 40 into a single .ogg file.
	/// Names files where possible (if set).
	/// </summary>
	public class SoundtrackCombineJob : CacheJob
	{
		private bool _nameTracks;
		private bool _lossless;

		public bool NameTracks
		{
			get { return _nameTracks; }
			set
			{
				if (IsStarted)
					throw new InvalidOperationException("Can't change property, job has already started.");

				_nameTracks = value;
			}
		}

		public bool Lossless
		{
			get { return _lossless; }
			set
			{
				if (IsStarted)
					throw new InvalidOperationException("Can't change property, job has already started.");

				_lossless = value;
			}
		}

		public SoundtrackCombineJob(bool nameTracks = true, bool lossless = false)
		{
			NameTracks = nameTracks;
			Lossless = lossless;
		}


		public override async Task StartAsync()
		{
			await Task.Run(() =>
			{
				if (!Cache.ArchiveExtracted(40))
					throw new DirectoryNotFoundException("Archive 40 needs to be extracted at this point.");

				if (NameTracks && !Cache.ArchiveExtracted(17))
					throw new DirectoryNotFoundException("Archive 17 needs to be extracted at this point.");

				//get trackname lookup list
				var trackNames = NameTracks ? Soundtrack.GetTrackNames() : new Dictionary<int, string>();

				string archiveDir = Cache.OutputDirectory + "cache/40/";
				string soundtrackDir = Cache.OutputDirectory + "soundtrack/";
				Directory.CreateDirectory(soundtrackDir);
				int filesProcessed = 0;
				List<Task> soxTasks = new List<Task>();

				//gather all index files
				string[] indexFiles = Directory.GetFiles(archiveDir, "*.jaga");

				foreach (string indexFileString in indexFiles)
				{
					if (IsCanceled)
						break;

					int indexFileId = int.Parse(Path.GetFileNameWithoutExtension(indexFileString));

					//create output file name
					string outFileName = trackNames.ContainsKey(indexFileId) ? trackNames[indexFileId] : indexFileId.ToString();
					string outFile = soundtrackDir + outFileName + "." + (Lossless ? "flac" : "ogg");
					string workFile = Cache.TempDirectory + outFileName + "." + (Lossless ? "flac" : "ogg");
					string indexChunkFile = Cache.TempDirectory + "index" + indexFileId + ".ogg";

					//skip existing files
					if (File.Exists(outFile))
					{
						Log($"{indexFileId}: Already exists.");
						ReportProgress(++filesProcessed, indexFiles.Length);
						continue;
					}

					List<string> chunkFiles = new List<string>();

					using (FileStream indexFileStream = File.OpenRead(indexFileString))
					{
						bool skipFile = false;

						//read 4-byte file indexes from bye 33 up to OggS magic number (the first chunk)
						indexFileStream.Position = 32L;

						while (indexFileStream.ReadBytes(4) != 0x4f676753)
						{
							uint fileId = indexFileStream.ReadBytes(4);

							//check if the file exists and add it to the buffer if it does
							if (File.Exists(archiveDir + fileId + ".ogg"))
								chunkFiles.Add(archiveDir + fileId + ".ogg");
							else
							{
								//...or cancel construction of track
								Log($"{indexFileId}: Incomplete.");
								skipFile = true;
								break;
							}
						}

						if (skipFile)
						{
							ReportProgress(++filesProcessed, indexFiles.Length);
							continue;
						}

						//copy the index's audio chunk to a temp file so SoX can handle the combining
						using (FileStream tempIndexFile = File.Open(indexChunkFile, FileMode.Create, FileAccess.Write))
						{
							indexFileStream.Position -= 4L; //include OggS
							indexFileStream.CopyTo(tempIndexFile);
						}
					}

					Process soxProcess = new Process
					{
						StartInfo =
						{
							FileName = "sox",
							UseShellExecute = false,
							CreateNoWindow = true
						}
					};

					soxProcess.StartInfo.Arguments = $"--combine concatenate {indexChunkFile}";
					chunkFiles.ForEach(str =>
					{
						soxProcess.StartInfo.Arguments += " " + str;
					});
					soxProcess.StartInfo.Arguments +=
						$" -C 6 --comment \"Created by Viller's RuneScapeCacheTools, combined by SoX.\" \"{workFile}\"";

					//do the time consuming part on a task
					var soxTask = new Task(() =>
					{
                        soxProcess.Start();
						soxProcess.WaitForExit();

						if (soxProcess.ExitCode == 0)
						{
							//wait until unlocked (if locked)
							bool moved = false;
							do
							{
								try
								{
									File.Move(workFile, outFile);
									moved = true;
								}
								catch (IOException)
								{
									Thread.Sleep(100);
								}
							} while (!moved);

							Log($"{indexFileId}{(outFileName != indexFileId.ToString() ? $" ({outFileName})" : "")}: Combined by SoX.");
						}
						else
						{
							//remove the leftover index file
							File.Delete(indexChunkFile);
							Log($"{indexFileId}: SoX error code \"{soxProcess.ExitCode}\".");
						}
						ReportProgress(++filesProcessed, indexFiles.Length);
					});

					soxTasks.Add(soxTask);
					soxTask.Start();
				}

				//wait for all tasks to exit
				foreach (var soxTask in soxTasks)
					soxTask.Wait();

				IsFinished = true;
			});
		}
	}
}
