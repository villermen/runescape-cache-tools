using RuneScapeCacheTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Villermen.RuneScapeCacheTools
{
	public abstract class Cache
	{
		/// <summary>
		/// The directory that is the default location for this type of cache.
		/// </summary>
		public abstract string DefaultCacheDirectory { get; }

		/// <summary>
		/// The location where the cache is located.
		/// </summary>
		public string CacheDirectory { get; set; }

		/// <summary>
		/// The location where the processed cache will be stored.
		/// </summary>
		public string OutputDirectory { get; set; }

		/// <summary>
		/// Temporary files used while processing will be stored here.
		/// </summary>
		public string TemporaryDirectory { get; set; }

		/// <summary>
		/// Processor used on obtained data.
		/// </summary>
		public IFileProcessor FileProcessor { get; set; } = new ExtendableFileProcessor();

		protected Cache()
		{
			CacheDirectory = DefaultCacheDirectory;
			TemporaryDirectory = Path.GetTempPath() + "rsct/";
		}

		protected Cache(IFileProcessor fileProcessor) : this()
		{
			FileProcessor = fileProcessor;
		}

		public abstract IEnumerable<int> getArchiveIds();

		public abstract IEnumerable<int> getFileIds(int archiveId);

		/// <summary>
		/// Extracts every file in every archive.
		/// </summary>
		/// <returns></returns>
		public async Task ExtractAllAsync()
		{
			IEnumerable<int> archiveIds = getArchiveIds();

			await Task.Run(() =>
			{
				Parallel.ForEach(archiveIds, async (archiveId) =>
				{
					await ExtractArchiveAsync(archiveId);
				});
			});
		}

		/// <summary>
		/// Extracts every file in the given archive.
		/// </summary>
		/// <param name="archiveId"></param>
		/// <returns></returns>
		public async Task ExtractArchiveAsync(int archiveId)
		{
			IEnumerable<int> fileIds = getFileIds(archiveId);

			await Task.Run(() =>
			{
				Parallel.ForEach(fileIds, (fileId) =>
				{
					ExtractFile(archiveId, fileId);
				});
			});
		}

		/// <summary>
		/// Extracts the given file in the given archive.
		/// </summary>
		/// <param name="archiveId"></param>
		/// <param name="fileId"></param>
		/// <returns></returns>
		public void ExtractFile(int archiveId, int fileId)
		{
			// TODO: return bool?
			byte[] fileData = GetFileData(archiveId, fileId);

			FileProcessor.Process(ref fileData);
			string extension = FileProcessor.GuessExtension(ref fileData);

			WriteFile(archiveId, fileId, fileData, extension);
		}

		/// <summary> 
		/// </summary>
		/// <param name="archiveId"></param>
		/// <returns>The path to the directory of the given archive, or null if it does not exist.</returns>
		public virtual string getArchiveOutputPath(int archiveId)
		{
			string archivePath = $"{OutputDirectory}cache/{archiveId}/";

			if (Directory.Exists(archivePath))
			{
				return archivePath;
			}

			return null;
		}

		/// <summary>
		/// Finds the path for the given extracted file.
		/// </summary>
		/// <param name="archiveId"></param>
		/// <param name="fileId"></param>
		/// <param name="extractIfMissing">Try to extract the file if it hasn't been extracted yet.</param>
		/// <returns>Returns the path to the obtained file, or null if it does not exist.</returns>
		public virtual string GetFileOutputPath(int archiveId, int fileId, bool extractIfMissing = false)
		{
			try
			{
				string path = Directory.EnumerateFiles($"{OutputDirectory}cache/{archiveId}/", $"{fileId}*")
					.Where(file => Regex.IsMatch(file, $@"(/|\\){fileId}(\..+)?$"))
					.FirstOrDefault();

				if (!string.IsNullOrWhiteSpace(path))
				{
					return path;
				}

				if (extractIfMissing)
				{
					ExtractFile(archiveId, fileId);
					return GetFileOutputPath(archiveId, fileId);
				}

				return null;
			}
			catch (DirectoryNotFoundException)
			{
				return null;
			}
		}

		/// <summary>
		/// Write out the given data to the specified file.
		/// Deletes a previous version of the file (regardless of extension) if it exists.
		/// </summary>
		/// <param name="archiveId"></param>
		/// <param name="fileId"></param>
		/// <param name="data"></param>
		/// <param name="extension">File extension, without the dot.</param>
		protected void WriteFile(int archiveId, int fileId, byte[] data, string extension = null)
		{
			// Throw an exception if the output directory is not yet set or does not exist
			if (string.IsNullOrWhiteSpace(OutputDirectory) || !Directory.Exists(OutputDirectory))
			{
				throw new DirectoryNotFoundException("Output directory does not exist.");
			}

			// Delete existing file
			string existingFilePath = GetFileOutputPath(archiveId, fileId);
			if (!string.IsNullOrWhiteSpace(existingFilePath))
			{
				File.Delete(existingFilePath);
			}

			// Construct new path
			string newFilePath = $"{OutputDirectory}cache/{archiveId}/{fileId}";
			if (!string.IsNullOrWhiteSpace(extension))
			{
				newFilePath += $".{extension}";
			}

			// Create directories where necessary, before writing to file
			Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
			File.WriteAllBytes(newFilePath, data);
		}

		/// <summary>
		/// Returns the raw data for the given file.
		/// </summary>
		/// <param name="archiveId"></param>
		/// <param name="fileId"></param>
		/// <returns></returns>
		protected abstract byte[] GetFileData(int archiveId, int fileId);

		/// <summary>
		/// Processes the data of a file.
		/// </summary>
		/// <param name="fileData"></param>
		/// <returns>If the method can determine an extension, this will be a non-null string containing the extension.</returns>
		protected virtual void ProcessFileData(ref byte[] fileData)
		{
			
		}
	}
}
