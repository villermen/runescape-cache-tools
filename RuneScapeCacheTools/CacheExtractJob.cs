using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.BZip2;

namespace RuneScapeCacheTools
{
	public class CacheExtractJob : CacheJob
	{
		private List<int> _archiveIds;
		private List<int> _fileIds;

		/// <summary>
		/// The archives to extract files of.
		/// </summary>
		public List<int> ArchiveIds
		{
			get { return _archiveIds; }
			set
			{
				if (IsStarted)
					throw new InvalidOperationException("Can't change archives, job has already started.");

				_archiveIds = value;
			}
		}

		/// <summary>
		/// The files to extract, if null all files of specified archives will be extracted.
		/// </summary>
		public List<int> FileIds
		{
			get { return _fileIds; }
			set
			{
				if (IsStarted)
					throw new InvalidOperationException("Can't change files, job has already started.");

				_fileIds = value;
			}
		}

		public bool OverwriteExistingFiles { get; set; }

		/// <summary>
		/// Creates a new job to extract all archives fully.
		/// </summary>
		public CacheExtractJob(bool overwrite = false)
		{
			ArchiveIds = Cache.GetArchiveIds().ToList();
			OverwriteExistingFiles = overwrite;
		}

		/// <summary>
		/// Creates a new job to extract one archive fully.
		/// </summary>
		public CacheExtractJob(int archiveId, bool overwrite = false)
		{
			ArchiveIds = new List<int> { archiveId };
			OverwriteExistingFiles = overwrite;
		}

		/// <summary>
		/// Creates a new job to extract a list of archives fully.
		/// </summary>
		public CacheExtractJob(IEnumerable<int> archiveIds, bool overwrite = false)
		{
			ArchiveIds = archiveIds.ToList();
			OverwriteExistingFiles = overwrite;
		}

		/// <summary>
		/// Creates a job to extract a list of files out of a single archive.
		/// </summary>
		public CacheExtractJob(int archiveId, IEnumerable<int> fileIds, bool overwrite = false)
		{
			ArchiveIds = new List<int> { archiveId };
			FileIds = fileIds.ToList();
			OverwriteExistingFiles = overwrite;
		}

		/// <summary>
		/// Creates a job to extract one file of one archive.
		/// </summary>
		public CacheExtractJob(int archiveId, int fileId, bool overwrite = false)
		{
			ArchiveIds = new List<int> { archiveId };
			FileIds = new List<int> { fileId };
			OverwriteExistingFiles = overwrite;
		}

		public override async Task StartAsync()
		{
			//confirm properties are valid
			if (ArchiveIds == null || ArchiveIds.Count == 0)
				throw new ArgumentException("At least one archive must be set for extraction."); 

			IsStarted = true;

			//obtain total amount of files (in multi-archive mode)
			int totalFiles = FileIds?.Count ??
				(int)ArchiveIds.Sum(archiveId => new FileInfo(Cache.CacheDirectory + Cache.IndexFilePrefix + archiveId).Length / 6);

			await Task.Run(() =>
			{
				using (FileStream cacheFile = File.OpenRead(Cache.CacheDirectory + Cache.CacheFileName))
				{
					int processedFiles = 0;

					//process all given archives
					foreach (int archiveId in ArchiveIds)
					{
						//open index file and read in all files sequentially
						using (FileStream indexFile = File.OpenRead(Cache.CacheDirectory + Cache.IndexFilePrefix + archiveId))
						{
							//obtain list of all files in archive
							if (FileIds == null)
								_fileIds = Enumerable.Range(0, (int)indexFile.Length / 6).ToList(); //todo: turn back to 0

							foreach (int fileId in FileIds)
							{
								//exit loop when canceled
								if (IsCanceled)
								{
									break;
								}

								string fileDisplayName = archiveId + "/" + fileId;

								bool fileError = false;

								indexFile.Position = fileId * 6L;

								uint fileSize = indexFile.ReadBytes(3);
								long startChunkOffset = indexFile.ReadBytes(3) * 520L;

								if (fileSize > 0 && startChunkOffset > 0 && startChunkOffset + fileSize <= cacheFile.Length)
								{
									byte[] buffer = new byte[fileSize];
									int writeOffset = 0; //point to which we are writing to the buffer
									long currentChunkOffset = startChunkOffset;

									for (int chunkIndex = 0; writeOffset < fileSize && currentChunkOffset > 0; chunkIndex++)
									{
										cacheFile.Position = currentChunkOffset;

										int chunkSize;
										int checksumFileIndex = 0;

										if (fileId < 65536)
										{
											chunkSize = (int)Math.Min(512, fileSize - writeOffset);
										}
										else
										{
											//if file index exceeds 2 bytes, add 65536 and read 2(?) extra bytes
											chunkSize = (int)Math.Min(510, fileSize - writeOffset);

											cacheFile.ReadByte(); //this appears to always be 0
											checksumFileIndex = (cacheFile.ReadByte() << 16);
										}

										checksumFileIndex += (int)cacheFile.ReadBytes(2);
										int checksumChunkIndex = (int)cacheFile.ReadBytes(2);
										long nextChunkOffset = cacheFile.ReadBytes(3) * 520L;
										int checksumArchiveIndex = cacheFile.ReadByte();
										if (checksumFileIndex == fileId && checksumChunkIndex == chunkIndex && checksumArchiveIndex == archiveId &&
										    nextChunkOffset >= 0 && nextChunkOffset < cacheFile.Length)
										{
											cacheFile.Read(buffer, writeOffset, chunkSize);
											writeOffset += chunkSize;
											currentChunkOffset = nextChunkOffset;
										}
										else
											fileError = true;
									}

									//save file if there is nothing wrong with it
									if (!fileError)
									{
										//remove the first 5 bytes because they are not part of the file (they are most likely some kind of extra checksum I can't explain)
										byte[] tempBuffer = new byte[fileSize - 5];
										Array.Copy(buffer, 5, tempBuffer, 0, fileSize - 5);
										buffer = tempBuffer;

										//process the file
										string extension;
										ProcessFile(ref buffer, out extension);

										//create target directory if it doesn't exist yet
										Directory.CreateDirectory(Cache.OutputDirectory + "cache/" + archiveId + "/");

										//remove existing extensionless file (for backward compatibility)
										string extensionLessOutFile = Cache.OutputDirectory + "cache/" + archiveId + "/" + fileId;
										string outFile = extensionLessOutFile + extension;

										if (!File.Exists(outFile) || OverwriteExistingFiles) //todo: decide overwrite before processing
										{
											if (extension.Length > 0 && File.Exists(extensionLessOutFile))
											{
												File.Delete(extensionLessOutFile);
												Log(fileDisplayName + ": Deleted because this version has an extension.");
											}

											using (FileStream outFileStream = File.Open(extensionLessOutFile + extension, FileMode.Create))
											{
												outFileStream.Write(buffer, 0, buffer.Length);
												Log(fileDisplayName + extension + ": Extracted.");
											}
										}
										else
											Log(fileDisplayName + ": Ignored because it already exists.");
									}
									else
										Log(fileDisplayName + ": Ignored because a chunk's checksum doesn't match, ideally should not happen.");
								}
								else
									Log(fileDisplayName + ": Ignored because of size or offset.");

								ReportProgress(++processedFiles, totalFiles);
							}
						}
					}
				}
			});

			IsFinished = true;
		}

		/// <summary>
		/// Processes a file buffer.
		/// Will decompress, and find an appropriate extension for it if possible.
		/// </summary>
		private static void ProcessFile(ref byte[] buffer, out string extension)
		{
			extension = "";

			//decompress gzip
			if (buffer.Length > 5 && (buffer[4] << 8) + buffer[5] == 0x1f8b) //gzip
			{
				//remove another 4 non-file bytes
				byte[] tempBuffer = new byte[buffer.Length - 4];
				Array.Copy(buffer, 4, tempBuffer, 0, buffer.Length - 4);
				buffer = tempBuffer;

				GZipStream decompressionStream = new GZipStream(new MemoryStream(buffer), CompressionMode.Decompress);

				int readBytes;
				tempBuffer = new byte[0];

				do
				{
					byte[] readBuffer = new byte[100000];
					readBytes = decompressionStream.Read(readBuffer, 0, 100000);

					int storedBytes = tempBuffer.Length;
					Array.Resize(ref tempBuffer, tempBuffer.Length + readBytes);
					Array.Copy(readBuffer, 0, tempBuffer, storedBytes, readBytes);
				}
				while (readBytes == 100000);

				buffer = tempBuffer;
			}

			//decompress bzip2
			if (buffer.Length > 9 && buffer[4] == 0x31 && buffer[5] == 0x41 && buffer[6] == 0x59 && buffer[7] == 0x26 && buffer[8] == 0x53 && buffer[9] == 0x59) //bzip2
			{
				//remove another 4 non-file bytes
				byte[] tempBuffer = new byte[buffer.Length - 4];
				Array.Copy(buffer, 4, tempBuffer, 0, buffer.Length - 4);
				buffer = tempBuffer;

				//prepend file header
				byte[] magic = {
					0x42, 0x5a, //BZ (signature)
					0x68,		//h (version)
					0x31		//*100kB block-size
				};

				tempBuffer = new byte[magic.Length + buffer.Length];
				magic.CopyTo(tempBuffer, 0);
				buffer.CopyTo(tempBuffer, magic.Length);
				buffer = tempBuffer;

				BZip2InputStream decompressionStream = new BZip2InputStream(new MemoryStream(buffer));

				int readBytes;
				tempBuffer = new byte[0];

				do
				{
					byte[] readBuffer = new byte[100000];
					readBytes = decompressionStream.Read(readBuffer, 0, 100000);

					int storedBytes = tempBuffer.Length;
					Array.Resize(ref tempBuffer, tempBuffer.Length + readBytes);
					Array.Copy(readBuffer, 0, tempBuffer, storedBytes, readBytes);
				}
				while (readBytes == 100000);

				buffer = tempBuffer;
			}

			//detect appropriate extension
			if (buffer.Length > 3 && (buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3] == 0x4f676753)
				extension = ".ogg";
			else if (buffer.Length > 3 && (buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3] == 0x4a414741)
				extension = ".jaga";
			else if (buffer.Length > 3 && (uint)(buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3] == 0x89504e47)
				extension = ".png";
		}
	}
}
