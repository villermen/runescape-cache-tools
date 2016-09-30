using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    ///     Base class for current cache systems.
    ///     For cache structures expressing the notion of indexes and archives.
    /// </summary>
    public abstract class CacheBase : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CacheBase));

        private string _outputDirectory;

        private string _temporaryDirectory;

        protected CacheBase(string cacheDirectory)
        {
            CacheDirectory = cacheDirectory;
            OutputDirectory = "output";
            TemporaryDirectory = Path.GetTempPath() + "rsct";
        }

        public abstract int IndexCount { get; }

        /// <summary>
        ///     The directory where the cache is located.
        /// </summary>
        public string CacheDirectory { get; }

        /// <summary>
        ///     Processor used on obtained data.
        /// </summary>
        public IExtensionGuesser ExtensionGuesser { get; set; } = new ExtendableExtensionGuesser();

        /// <summary>
        ///     The directory where the extracted cache files will be stored.
        /// </summary>
        public string OutputDirectory
        {
            get { return _outputDirectory; }
            set { _outputDirectory = PathExtensions.FixDirectory(value); }
        }

        /// <summary>
        ///     Temporary files used while processing will be stored here.
        /// </summary>
        public string TemporaryDirectory
        {
            get { return _temporaryDirectory; }
            set { _temporaryDirectory = PathExtensions.FixDirectory(value); }
        }

        /// <summary>
        ///     Returns the data and metadata for the requested file.
        /// </summary>
        /// <param name="indexId"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public abstract CacheFile GetFile(int indexId, int fileId);

        public abstract int GetFileCount(int indexId);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // public abstract int GetArchiveFileCount(int indexId, int archiveId);

        /// <summary>
        ///     Extracts every file from every index.
        /// </summary>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public void Extract(bool overwrite = false)
        {
            var indexIds = Enumerable.Range(0, IndexCount);
            Parallel.ForEach(indexIds, indexId => { Extract(indexId, overwrite); });
        }

        /// <summary>
        ///     Extracts specified indexes fully.
        /// </summary>
        /// <param name="indexIds"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public void Extract(IEnumerable<int> indexIds, bool overwrite = false)
        {
            Parallel.ForEach(indexIds, indexId => { Extract(indexId, overwrite); });
        }

        /// <summary>
        ///     Extracts specified index fully.
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
        ///     Extracts specified files from the specified index.
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
        ///     Extracts the specified file from the specified index.
        /// </summary>
        /// <param name="indexId"></param>
        /// <param name="fileId"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public void Extract(int indexId, int fileId, bool overwrite = false)
        {
            var file = GetFile(indexId, fileId);

            for (var entryId = 0; entryId < file.Entries.Length; entryId++)
            {
                var currentData = file.Entries[entryId];

                // Skip empty entries
                if (currentData.Length < 2)
                {
                    continue;
                }

                var extension = ExtensionGuesser.GuessExtension(currentData);

                // Throw an exception if the output directory is not yet set or does not exist
                if (string.IsNullOrWhiteSpace(OutputDirectory))
                {
                    throw new CacheException("Output directory must be set before file extraction.");
                }

                // Delete existing file (if allowed)
                var existingFilePath = GetFileOutputPath(indexId, fileId, entryId);
                if (existingFilePath != null)
                {
                    if (!overwrite)
                    {
                        CacheBase.Logger.Info(
                            $"Skipped index {indexId} file {fileId}{(entryId > 0 ? $"-{entryId}" : "")} because it is already extracted.");
                        return;
                    }

                    File.Delete(existingFilePath);
                }

                // Construct new path for file
                var newFilePath = $"{OutputDirectory}extracted/{indexId}/{fileId}" + (entryId > 0 ? $"-{entryId}" : "") + (!string.IsNullOrWhiteSpace(extension) ? $".{extension}" : "");

                // Create directories where necessary, before writing to file
                Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
                File.WriteAllBytes(newFilePath, currentData);
                CacheBase.Logger.Info($"Extracted index {indexId} file {fileId}.");
            }
        }

        /// <summary>
        ///     Finds the path for the given extracted file.
        /// </summary>
        /// <param name="indexId"></param>
        /// <param name="fileId"></param>
        /// <param name="entryId"></param>
        /// <returns>Returns the path to the obtained file, or null if it does not exist.</returns>
        public string GetFileOutputPath(int indexId, int fileId, int entryId = 0)
        {
            try
            {
                // Suffix fileId with entryId + 1 if nonzero
                var fileIdString = fileId + (entryId > 0 ? "-" + entryId : "");

                var path = Directory
                    .EnumerateFiles($"{OutputDirectory}extracted/{indexId}/", $"{fileIdString}*")

                    // Check if fileIdString is the full name of the file minus extension, prevents matching 234 with 2345.ext
                    .FirstOrDefault(file => Regex.IsMatch(file, $@"(/|\\){fileIdString}(\..+)?$"));

                return !string.IsNullOrWhiteSpace(path) ? path : null;
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="indexId"></param>
        /// <returns>The path to the directory of the given index, or null if it does not exist.</returns>
        public string GetIndexOutputPath(int indexId)
        {
            string indexPath = $"{OutputDirectory}extracted/{indexId}/";

            return Directory.Exists(indexPath) ? indexPath : null;
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}