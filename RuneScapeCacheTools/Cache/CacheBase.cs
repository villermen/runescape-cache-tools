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

        protected CacheBase()
        {
            OutputDirectory = "output";
            TemporaryDirectory = Path.GetTempPath() + "rsct";
        }

        public abstract IEnumerable<Index> Indexes { get; }

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
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public abstract CacheFile GetFile(Index index, int fileId);

        public async Task<CacheFile> GetFileAsync(Index index, int fileId)
        {
            return await Task.Run(() => GetFile(index, fileId));
        }

        public abstract IEnumerable<int> GetFileIds(Index index);

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
            Parallel.ForEach(Indexes, index => { Extract(index, overwrite); });
        }

        /// <summary>
        ///     Extracts specified indexes fully.
        /// </summary>
        /// <param name="indexes"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public void Extract(IEnumerable<Index> indexes, bool overwrite = false)
        {
            Parallel.ForEach(indexes, index => { Extract(index, overwrite); });
        }

        /// <summary>
        ///     Extracts specified index fully.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public void Extract(Index index, bool overwrite = false)
        {
            var fileIds = GetFileIds(index);
            Parallel.ForEach(fileIds, fileId => { ExtractAsync(index, fileId, overwrite).Wait(); });
        }

        /// <summary>
        ///     Extracts specified files from the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileIds"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public void Extract(Index index, IEnumerable<int> fileIds, bool overwrite = false)
        {
            Parallel.ForEach(fileIds, fileId => { ExtractAsync(index, fileId, overwrite).Wait(); });
        }

        /// <summary>
        ///     Extracts the specified file from the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public async Task ExtractAsync(Index index, int fileId, bool overwrite = false)
        {
            var file = await GetFileAsync(index, fileId);

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
                var existingFilePath = GetExtractedFilePath(index, fileId, entryId);
                if (existingFilePath != null)
                {
                    if (!overwrite)
                    {
                        CacheBase.Logger.Info(
                            $"Skipped index {index} file {fileId}{(entryId > 0 ? $"-{entryId}" : "")} because it is already extracted.");
                        return;
                    }

                    File.Delete(existingFilePath);
                }

                // Construct new path for file
                var newFilePath = $"{OutputDirectory}extracted/{index}/{fileId}" + (entryId > 0 ? $"-{entryId}" : "") + (!string.IsNullOrWhiteSpace(extension) ? $".{extension}" : "");

                // Create directories where necessary, before writing to file
                Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
                File.WriteAllBytes(newFilePath, currentData);
                CacheBase.Logger.Info($"Extracted index {index} file {fileId}.");
            }
        }

        public void Extract(Index index, int fileId, bool overwrite = false)
        {
            ExtractAsync(index, fileId, overwrite).Wait();
        }

        /// <summary>
        ///     Finds the path for the given extracted file.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <param name="entryId"></param>
        /// <returns>Returns the path to the obtained file, or null if it does not exist.</returns>
        public string GetExtractedFilePath(Index index, int fileId, int entryId = 0)
        {
            try
            {
                // Suffix fileId with entryId + 1 if nonzero
                var fileIdString = fileId + (entryId > 0 ? "-" + entryId : "");

                var path = Directory
                    .EnumerateFiles($"{OutputDirectory}extracted/{index}/", $"{fileIdString}*")

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
        public string GetExtractedIndexPath(int indexId)
        {
            string indexPath = $"{OutputDirectory}extracted/{indexId}/";

            return Directory.Exists(indexPath) ? indexPath : null;
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}