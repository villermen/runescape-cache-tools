using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;

namespace Villermen.RuneScapeCacheTools.Cache
{
	/// <summary>
	///   Base class for current cache systems.
	///   Cache should include indexes and archives in order to use this.
	/// </summary>
	public abstract class CacheBase
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(CacheBase));

		protected CacheBase()
		{
			CacheDirectory = DefaultCacheDirectory;
		}

		/// <summary>
		///   The directory that is the default location for this type of cache.
		/// </summary>
		public abstract string DefaultCacheDirectory { get; }

		/// <summary>
		///   The directory where the cache is located.
		/// </summary>
		public virtual string CacheDirectory { get; set; }

		/// <summary>
		///   The directory where the extracted cache files will be stored.
		/// </summary>
		public string OutputDirectory { get; set; }

		/// <summary>
		///   Temporary files used while processing will be stored here.
		/// </summary>
		public string TemporaryDirectory { get; set; } = Path.GetTempPath() + "rsct/";

		/// <summary>
		///   Processor used on obtained data.
		/// </summary>
		public IExtensionGuesser ExtensionGuesser { get; set; } = new ExtendableExtensionGuesser();

		public abstract int IndexCount { get; }

		public abstract int GetFileCount(int indexId);

		public abstract int GetArchiveFileCount(int indexId, int archiveId);

		/// <summary>
		///   Extracts every file in every index.
		/// </summary>
		/// <returns></returns>
		public async Task ExtractAllAsync()
		{
			var indexIds = Enumerable.Range(0, IndexCount);
			await Task.Run(() => { Parallel.ForEach(indexIds, indexId => { ExtractIndexAsync(indexId).Wait(); }); });
		}

		/// <summary>
		///   Extracts every file in the given index.
		/// </summary>
		/// <param name="indexId"></param>
		/// <returns></returns>
		public async Task ExtractIndexAsync(int indexId)
		{
			var fileIds = Enumerable.Range(0, GetFileCount(indexId));
			await Task.Run(() =>
			{
				Parallel.ForEach(fileIds, fileId =>
				{
					ExtractFile(indexId, fileId);
					Logger.Info($"Extracted index {indexId} file {fileId}.");
				});
			});
		}

		/// <summary>
		///   Extracts and processes the given file in the given index.
		/// </summary>
		/// <param name="indexId"></param>
		/// <param name="fileId"></param>
		/// <returns></returns>
		public void ExtractFile(int indexId, int fileId)
		{
			var fileData = GetFileData(indexId, fileId);

			if (fileData == null)
			{
				return;
			}

			var extension = ExtensionGuesser.GuessExtension(ref fileData);

			WriteOutputFile(indexId, fileId, fileData, extension);
		}

		/// <summary>
		/// </summary>
		/// <param name="indexId"></param>
		/// <returns>The path to the directory of the given index, or null if it does not exist.</returns>
		public virtual string GetIndexOutputPath(int indexId)
		{
			string indexPath = $"{OutputDirectory}cache/{indexId}/";

			return Directory.Exists(indexPath) ? indexPath : null;
		}

		/// <summary>
		///   Finds the path for the given extracted file.
		/// </summary>
		/// <param name="indexId"></param>
		/// <param name="fileId"></param>
		/// <param name="extractIfMissing">Try to extract the file if it hasn't been extracted yet.</param>
		/// <returns>Returns the path to the obtained file, or null if it does not exist.</returns>
		public virtual string GetFileOutputPath(int indexId, int fileId, bool extractIfMissing = false)
		{
			try
			{
				var path = Directory
					.EnumerateFiles($"{OutputDirectory}cache/{indexId}/", $"{fileId}*")
					.FirstOrDefault(file => Regex.IsMatch(file, $@"(/|\\){fileId}(\..+)?$"));

				if (!string.IsNullOrWhiteSpace(path))
				{
					return path;
				}

				if (!extractIfMissing)
				{
					Logger.Info($"File with index {indexId} file {fileId} not found, extracting...");
					return null;
				}

				ExtractFile(indexId, fileId);
				return GetFileOutputPath(indexId, fileId);
			}
			catch (DirectoryNotFoundException exception)
			{
				Logger.Error($"File output path for index {indexId} file {fileId} not found.", exception);
				return null;
			}
		}

		/// <summary>
		///   Write out the given data to the specified file in the output directory.
		///   Deletes a previous version of the file (regardless of extension) if it exists.
		/// </summary>
		/// <param name="indexId"></param>
		/// <param name="fileId"></param>
		/// <param name="data"></param>
		/// <param name="extension">File extension, without the dot.</param>
		protected void WriteOutputFile(int indexId, int fileId, byte[] data, string extension = null)
		{
			// Throw an exception if the output directory is not yet set or does not exist
			if (string.IsNullOrWhiteSpace(OutputDirectory) || !Directory.Exists(OutputDirectory))
			{
				throw new DirectoryNotFoundException("Output directory does not exist.");
			}

			// Delete existing file
			var existingFilePath = GetFileOutputPath(indexId, fileId);
			if (!string.IsNullOrWhiteSpace(existingFilePath))
			{
				File.Delete(existingFilePath);
			}

			// Construct new path
			string newFilePath = $"{OutputDirectory}cache/{indexId}/{fileId}";
			if (!string.IsNullOrWhiteSpace(extension))
			{
				newFilePath += $".{extension}";
			}

			// Create directories where necessary, before writing to file
			Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
			File.WriteAllBytes(newFilePath, data);
		}

		/// <summary>
		///   Returns the raw data for the given file.
		/// </summary>
		/// <param name="indexId"></param>
		/// <param name="fileId"></param>
		/// <returns></returns>
		public abstract byte[] GetFileData(int indexId, int fileId);

		/// <summary>
		///   Returns the data for all the files in the specified archive.
		/// </summary>
		/// <param name="indexId"></param>
		/// <param name="archiveId"></param>
		/// <returns></returns>
		public byte[][] GetArchiveFiles(int indexId, int archiveId)
		{
			var fileCount = GetArchiveFileCount(indexId, archiveId);
			var archiveFilesData = new byte[fileCount][];

			for (var fileId = 0; fileId < fileCount; fileId++)
			{
				archiveFilesData[fileId] = GetArchiveFileData(indexId, archiveId, fileId);
			}

			return archiveFilesData;
		}

		/// <summary>
		///   Returns the data for the specified file in the specified archive.
		/// </summary>
		/// <param name="indexId"></param>
		/// <param name="archiveId"></param>
		/// <param name="fileId"></param>
		/// <returns></returns>
		public abstract byte[] GetArchiveFileData(int indexId, int archiveId, int fileId);
	}
}