using System;
using System.Collections.Generic;
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
	public abstract class CacheBase : IDisposable
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(CacheBase));

	    private string _cacheDirectory;

	    private string _outputDirectory;

	    private string _temporaryDirectory;

        protected CacheBase(string cacheDirectory)
		{
		    CacheDirectory = cacheDirectory;
            OutputDirectory = "output";
            TemporaryDirectory = Path.GetTempPath() + "rsct";
        }

		/// <summary>
		///   The directory where the cache is located.
		/// </summary>
		public string CacheDirectory
        {
            get { return _cacheDirectory; }
            set { _cacheDirectory = PathExtensions.FixDirectory(value); }
        }

	    /// <summary>
	    ///   The directory where the extracted cache files will be stored.
	    /// </summary>
	    public string OutputDirectory
	    {
	        get { return _outputDirectory; }
	        set { _outputDirectory = PathExtensions.FixDirectory(value); }
	    }

	    /// <summary>
		///   Temporary files used while processing will be stored here.
		/// </summary>
		public string TemporaryDirectory
        {
            get { return _temporaryDirectory; }
            set { _temporaryDirectory = PathExtensions.FixDirectory(value); }
        }

        /// <summary>
        ///   Processor used on obtained data.
        /// </summary>
        public IExtensionGuesser ExtensionGuesser { get; set; } = new ExtendableExtensionGuesser();

		public abstract int IndexCount { get; }

		public abstract int GetFileCount(int indexId);

		public abstract int GetArchiveFileCount(int indexId, int archiveId);

		/// <summary>
		///   Extracts every file from every index.
		/// </summary>
		/// <param name="overwrite"></param>
		/// <returns></returns>
		public void Extract(bool overwrite = false)
		{
			var indexIds = Enumerable.Range(0, IndexCount);
			Parallel.ForEach(indexIds, indexId => { Extract(indexId, overwrite); });
		}

		/// <summary>
		///   Extracts specified indexes fully.
		/// </summary>
		/// <param name="indexIds"></param>
		/// <param name="overwrite"></param>
		/// <returns></returns>
		public void Extract(IEnumerable<int> indexIds, bool overwrite = false)
		{
			Parallel.ForEach(indexIds, indexId => { Extract(indexId, overwrite); });
		}

		/// <summary>
		///   Extracts specified index fully.
		/// </summary>
		/// <param name="indexId"></param>
		/// <param name="overwrite"></param>
		/// <returns></returns>
		public void Extract(int indexId, bool overwrite = false)
		{
			var fileIds = Enumerable.Range(0, GetFileCount(indexId));
			Parallel.ForEach(fileIds, fileId => { Extract(indexId, fileId, overwrite); });
		}

		/// <summary>
		///   Extracts specified files from the specified index.
		/// </summary>
		/// <param name="indexId"></param>
		/// <param name="fileIds"></param>
		/// <param name="overwrite"></param>
		/// <returns></returns>
		public void Extract(int indexId, IEnumerable<int> fileIds, bool overwrite = false)
		{
			Parallel.ForEach(fileIds, fileId => { Extract(indexId, fileId, overwrite); });
		}

		/// <summary>
		///   Extracts the specified file from the specified index.
		/// </summary>
		/// <param name="indexId"></param>
		/// <param name="fileId"></param>
		/// <param name="overwrite"></param>
		/// <returns></returns>
		public void Extract(int indexId, int fileId, bool overwrite = false)
		{
			try
			{
				var fileData = GetFileData(indexId, fileId);

				if (fileData == null)
				{
					Logger.Info($"Skipped index {indexId} file {fileId} because it contains no data.");
					return;
				}

				var extension = ExtensionGuesser.GuessExtension(ref fileData);

				// Throw an exception if the output directory is not yet set or does not exist
				if (string.IsNullOrWhiteSpace(OutputDirectory))
				{
					throw new CacheException("Output directory must be set before file extraction.");
				}

				// Delete existing file (if allowed)
				var existingFilePath = GetFileOutputPath(indexId, fileId);
				if (existingFilePath != null)
				{
					if (!overwrite)
					{
						Logger.Info($"Skipped index {indexId} file {fileId} because it is already extracted.");
						return;
					}

					File.Delete(existingFilePath);
				}

				// Construct new path for file
				string newFilePath = $"{OutputDirectory}extracted/{indexId}/{fileId}";
				if (!string.IsNullOrWhiteSpace(extension))
				{
					newFilePath += $".{extension}";
				}

				// Create directories where necessary, before writing to file
				Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
				File.WriteAllBytes(newFilePath, fileData);
				Logger.Info($"Extracted index {indexId} file {fileId}.");
			}
			catch (SectorException exception)
			{
				Logger.Info($"Could not extract index {indexId} file {fileId}: {exception.Message}");
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="indexId"></param>
		/// <returns>The path to the directory of the given index, or null if it does not exist.</returns>
		public virtual string GetIndexOutputPath(int indexId)
		{
			string indexPath = $"{OutputDirectory}extracted/{indexId}/";

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
					.EnumerateFiles($"{OutputDirectory}extracted/{indexId}/", $"{fileId}*")
					.FirstOrDefault(file => Regex.IsMatch(file, $@"(/|\\){fileId}(\..+)?$"));

				if (!string.IsNullOrWhiteSpace(path))
				{
					return path;
				}

				if (!extractIfMissing)
				{
					return null;
				}

				Extract(indexId, fileId);
				return GetFileOutputPath(indexId, fileId);
			}
			catch (DirectoryNotFoundException)
			{
				return null;
			}
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

	    public void Dispose()
	    {
	        Dispose(true);
            GC.SuppressFinalize(this);
	    }

	    protected virtual void Dispose(bool disposing)
	    {
	    }

	    ~CacheBase()
	    {
	        Dispose(false);
	    }
	}
}