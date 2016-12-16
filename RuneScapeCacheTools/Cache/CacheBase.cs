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
    using Villermen.RuneScapeCacheTools.Cache.CacheFile;

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
            this.OutputDirectory = "output";
            this.TemporaryDirectory = Path.GetTempPath() + "rsct";
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
            get { return this._outputDirectory; }
            set { this._outputDirectory = PathExtensions.FixDirectory(value); }
        }

        /// <summary>
        ///     Temporary files used while processing will be stored here.
        /// </summary>
        public string TemporaryDirectory
        {
            get { return this._temporaryDirectory; }
            set { this._temporaryDirectory = PathExtensions.FixDirectory(value); }
        }

        /// <summary>
        ///     Returns the requested file.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public abstract T GetFile<T>(Index index, int fileId) where T : CacheFile.BaseCacheFile;

        public abstract IEnumerable<int> GetFileIds(Index index);

        public virtual void PutFile(CacheFile.BaseCacheFile file)
        {
            throw new NotSupportedException("Writing of files is not supported for this type of cache");
        }

        /// <summary>
        ///     Returns info on the file without actually obtaining the file.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public abstract CacheFileInfo GetFileInfo(Index index, int fileId);

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Extracts every file from every index.
        /// </summary>
        /// <param name="overwrite"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public void Extract(bool overwrite = false, ExtendedProgress progress = null)
        {
            this.Extract(this.Indexes, overwrite, progress);
        }

        /// <summary>
        ///     Extracts specified indexes fully.
        /// </summary>
        /// <param name="indexes"></param>
        /// <param name="overwrite"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public void Extract(IEnumerable<Index> indexes, bool overwrite = false, ExtendedProgress progress = null)
        {
            foreach (var index in indexes)
            {
                try
                {
                    this.Extract(index, overwrite, progress);
                }
                catch (CacheFileNotFoundException)
                {
                    // Skip failing of file id list retrieval (separate file failures are handled earlier on) if more than one index is requested
                    CacheBase.Logger.Info($"Skipped extracting {index} because its file list could not be obtained.");
                }
            }
        }

        /// <summary>
        ///     Extracts specified index fully.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="overwrite"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public void Extract(Index index, bool overwrite = false, ExtendedProgress progress = null)
        {
            var fileIds = this.GetFileIds(index);

            this.Extract(index, fileIds, overwrite, progress);
        }

        /// <summary>
        ///     Extracts specified files from the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileIds"></param>
        /// <param name="overwrite"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public void Extract(Index index, IEnumerable<int> fileIds, bool overwrite = false, ExtendedProgress progress = null)
        {
            try
            {
                var fileIdsArray = fileIds.ToArray();

                if (progress != null)
                {
                    progress.Total += fileIdsArray.Length;
                }

                Parallel.ForEach(fileIdsArray, fileId =>
                {
                    try
                    {
                        this.Extract(index, fileId, overwrite);

                        progress?.Report($"Extracted {index}/{fileId}.");
                    }
                    catch (CacheFileNotFoundException)
                    {
                        // Skip failed extractions if more than one file is specified
                        var logMessage = $"Skipped {index}/{fileId} because it was not found.";
                        progress?.Report(logMessage);
                        CacheBase.Logger.Info(logMessage);
                    }
                });
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Extracts the entries of the specified file in the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <param name="overwrite"></param>
        /// <returns>Paths of the newly extracted file entries, or null when the file was already extracted and <see cref="overwrite"/> was false.</returns>
        public List<string> Extract(Index index, int fileId, bool overwrite = false)
        {
            // Throw an exception if the output directory is not yet set or does not exist
            if (string.IsNullOrWhiteSpace(this.OutputDirectory))
            {
                throw new CacheException("Output directory must be set before extraction.");
            }

            var existingEntryPaths = this.GetExtractedEntryPaths(index, fileId);

            // Don't extract if the file already exists and we are not going to overwrite
            if (!overwrite && existingEntryPaths.Any())
            {
                Logger.Info($"Skipped extracting {index}/{fileId} because it is already extracted.");
                return null;
            }

            var file = this.GetFile<BaseCacheFile>(index, fileId);

            // Delete existing entries. Done after obtaining of new file to prevent existing files from being deleted when GetFile failes
            foreach (var existingEntryPath in existingEntryPaths)
            {
                File.Delete(existingEntryPath);
            }

            // Create index directory for when it did not exist yet
            Directory.CreateDirectory($"{this.OutputDirectory}extracted/{index}");

            // TODO: Create a better solution for this (GetEntries, or revert back to one class for entries and data?)
            var entries = file is DataCacheFile ? new byte[][] { ((DataCacheFile)file).Data } : ((EntryCacheFile)file).Entries;

            // Extract all entries
            var extension = "";
            var extractedEntries = 0;
            var extractedEntryPaths = new List<string>();
            for (var entryId = 0; entryId < entries.Length; entryId++)
            {
                var currentData = entries[entryId];

                // Skip empty entries
                if (currentData.Length == 0)
                {
                    continue;
                }

                extension = this.ExtensionGuesser.GuessExtension(currentData);
                extension = extension != null ? $".{extension}" : "";

                // Construct new path for entry
                var entryPath = $"{this.OutputDirectory}extracted/{index}/{fileId}{(entryId > 0 ? $"-{entryId}" : "")}{extension}";

                File.WriteAllBytes(entryPath, currentData);

                extractedEntries++;
            }

            if (extractedEntries > 0)
            {
                Logger.Info($"Extracted {index}/{fileId}{extension}{(extractedEntries > 1 ? $"({extractedEntries} entries)" : "")}.");
            }
            else
            {
                Logger.Info($"Did not extract {index}/{fileId} because it was empty.");
            }

            return extractedEntryPaths;
        }

        /// <summary>
        /// Returns paths to existing extracted entries of the file.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public List<string> GetExtractedEntryPaths(Index index, int fileId)
        {
            try
            {
                // Get all files that start with the given fileId
                return Directory.EnumerateFiles($"{this.OutputDirectory}extracted/{index}/", $"{fileId}*")
                    // Filter out false-positivies like 2345.ext when looking for 234.
                    .Where(file => Regex.IsMatch(file, $@"[/\\]{fileId}(\-\d+)?(\..+)?$"))
                    .ToList();
            }
            catch (DirectoryNotFoundException)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="indexId"></param>
        /// <returns>The path to the directory of the given index, or null if it does not exist.</returns>
        public string GetExtractedIndexPath(int indexId)
        {
            string indexPath = $"{this.OutputDirectory}extracted/{indexId}/";

            return Directory.Exists(indexPath) ? indexPath : null;
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}