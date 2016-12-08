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
        ///     Returns the requested file.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public abstract CacheFile GetFile(Index index, int fileId);

        public abstract IEnumerable<int> GetFileIds(Index index);

        /// <summary>
        ///     Returns info on the file without actually obtaining the file.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public abstract CacheFileInfo GetFileInfo(Index index, int fileId);

        public void Dispose()
        {
            Dispose(true);
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
            Extract(Indexes, overwrite, progress);
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
                Extract(index, overwrite, progress);
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
            var fileIds = GetFileIds(index);

            Extract(index, fileIds, overwrite, progress);
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
            var fileIdsArray = fileIds.ToArray();

            if (progress != null)
            {
                progress.Total += fileIdsArray.Length;
            }

            Parallel.ForEach(fileIdsArray, fileId =>
            {
                var extractedPath = Extract(index, fileId, overwrite);

                progress?.Report($"extracted {extractedPath}");
            });
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
            if (string.IsNullOrWhiteSpace(OutputDirectory))
            {
                throw new CacheException("Output directory must be set before extraction.");
            }

            var existingEntryPaths = GetExtractedEntryPaths(index, fileId);

            // Don't extract if the file already exists and we are not going to overwrite
            if (!overwrite && existingEntryPaths.Any())
            {
                Logger.Info($"Skipped extracting {index}/{fileId} because it is already extracted.");
                return null;
            }

            var file = GetFile(index, fileId);

            // Delete existing entries. Done after obtaining of new file to prevent existing files from being deleted when GetFile failes
            foreach (var existingEntryPath in existingEntryPaths)
            {
                File.Delete(existingEntryPath);
            }

            // Create index directory for when it did not exist yet
            Directory.CreateDirectory($"{OutputDirectory}extracted/{index}");

            // Extract all entries
            var extractedEntryPaths = new List<string>();
            for (var entryId = 0; entryId < file.Entries.Length; entryId++)
            {
                var currentData = file.Entries[entryId];

                // Skip empty entries
                if (currentData.Length == 0)
                {
                    continue;
                }

                var extension = ExtensionGuesser.GuessExtension(currentData);

                // Construct new path for entry
                var entryPath = $"{OutputDirectory}extracted/{index}/{fileId}" + (entryId > 0 ? $"-{entryId}" : "") + (extension != null ? $".{extension}" : "");

                File.WriteAllBytes(entryPath, currentData);

                Logger.Info($"Extracted {entryPath}.");
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
                return Directory.EnumerateFiles($"{OutputDirectory}extracted/{index}/", $"{fileId}*")
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
            string indexPath = $"{OutputDirectory}extracted/{indexId}/";

            return Directory.Exists(indexPath) ? indexPath : null;
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}