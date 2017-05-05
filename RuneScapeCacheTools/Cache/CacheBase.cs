using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
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
            this.OutputDirectory = "output";
            this.TemporaryDirectory = Path.GetTempPath() + "rsct";
        }

        public abstract IEnumerable<Index> Indexes { get; }

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
        ///     Returns the requested file and tries to convert it to the requested type if possible.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public T GetFile<T>(Index index, int fileId) where T : CacheFile
        {
            // Obtain the file /entry
            var file = this.FetchFile(index, fileId);

            // These we know
            file.Info.Index = index;
            file.Info.FileId = fileId;

            // Return the file as is when a binary file is requested
            if (typeof(T) == typeof(BinaryFile))
            {
                return file as T;
            }

            // Decode the file to the requested type
            var decodedFile = Activator.CreateInstance<T>();
            decodedFile.FromFile(file);
            return decodedFile;
        }

        /// <summary>
        /// Implements the logic for actually retrieving a file from the cache.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        protected abstract BinaryFile FetchFile(Index index, int fileId);

        public abstract IEnumerable<int> GetFileIds(Index index);

        public void PutFile(CacheFile file)
        {
            this.PutFile(file.ToBinaryFile());
        }

        public abstract void PutFile(BinaryFile file);

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
                catch (FileNotFoundException)
                {
                    // Skip failing of file id list retrieval (separate file failures are handled earlier on) if more than one index is requested
                    CacheBase.Logger.Info($"Skipped extracting index {(int)index} because its file list could not be obtained.");
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

                        progress?.Report($"Extracted {(int)index}/{fileId}.");
                    }
                    catch (FileNotFoundException)
                    {
                        // Skip failed extractions if more than one file is specified
                        var logMessage = $"Skipped {(int)index}/{fileId} because it was not found.";
                        CacheBase.Logger.Info(logMessage);
                        progress?.Report(logMessage);
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
                throw new InvalidOperationException("Output directory must be set before extraction.");
            }

            var existingEntryPaths = this.GetExtractedEntryPaths(index, fileId);

            // Don't extract if the file already exists and we are not going to overwrite
            if (!overwrite && existingEntryPaths.Any())
            {
                CacheBase.Logger.Info($"Skipped extracting {(int)index}/{fileId} because it is already extracted.");
                return null;
            }

            var binaryFile = this.GetFile<BinaryFile>(index, fileId);

            // Delete existing entries. Done after obtaining of new file to prevent existing files from being deleted when GetFile failes
            foreach (var existingEntryPath in existingEntryPaths)
            {
                File.Delete(existingEntryPath);
            }

            // Create index directory for when it does not exist yet
            Directory.CreateDirectory($"{this.OutputDirectory}extracted/{(int)index}");

            var extractedFilePaths = new List<string>();

            if (!binaryFile.Info.HasEntries)
            {
                // Extract file
                if (binaryFile.Data.Length > 0)
                {
                    var extension = ExtensionGuesser.GuessExtension(binaryFile.Data);
                    extension = extension != null ? $".{extension}" : "";

                    var filePath = $"{this.OutputDirectory}extracted/{(int)index}/{fileId}{extension}";
                    File.WriteAllBytes(filePath, binaryFile.Data);

                    extractedFilePaths.Add(filePath);

                    CacheBase.Logger.Info($"Extracted {(int)index}/{fileId}.");
                }
                else
                {
                    CacheBase.Logger.Info($"Did not extract {(int)index}/{fileId} because it is empty.");
                }
            }
            else
            {
                // Extract entries
                var entryFile = new EntryFile();
                entryFile.FromFile(binaryFile);

                if (entryFile.Entries.Count > 0)
                {
                    var entryBinaryFiles = entryFile.GetEntries<BinaryFile>();
                    foreach (var entryBinaryFilePair in entryBinaryFiles)
                    {
                        var extension = ExtensionGuesser.GuessExtension(entryBinaryFilePair.Value.Data);
                        extension = extension != null ? $".{extension}" : "";

                        var filePath = $"{this.OutputDirectory}extracted/{(int)index}/{fileId}-{entryBinaryFilePair.Key}{extension}";
                        File.WriteAllBytes(filePath, entryBinaryFilePair.Value.Data);

                        extractedFilePaths.Add(filePath);
                    }

                    CacheBase.Logger.Info($"Extracted {(int)index}/{fileId} ({entryBinaryFiles.Count} entries).");
                }
                else
                {
                    CacheBase.Logger.Info($"Did not extract {(int)index}/{fileId} because it has no entries.");
                }
            }

            return extractedFilePaths;
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
                return Directory.EnumerateFiles($"{this.OutputDirectory}extracted/{(int)index}/", $"{fileId}*")
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
        /// <param name="index"></param>
        /// <returns>The path to the directory of the given index, or null if it has not been created yet.</returns>
        public string GetExtractedIndexPath(Index index)
        {
            string indexPath = $"{this.OutputDirectory}extracted/{(int)index}/";

            return Directory.Exists(indexPath) ? indexPath : null;
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}