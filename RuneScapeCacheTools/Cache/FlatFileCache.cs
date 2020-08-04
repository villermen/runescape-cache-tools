using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    /// A cache that manages files in a simple directory and file structure.
    /// </summary>
    public class FlatFileCache : ICache
    {
        /// <summary>
        /// The base directory where all files will be stored in/retrieved from.
        /// </summary>
        public string BaseDirectory { get; }

        /// <summary>
        /// Whether to delete existing files before writing new ones.
        /// </summary>
        public bool OverwriteFiles { get; set; } = true;

        public FlatFileCache(string baseDirectory)
        {
            this.BaseDirectory = PathExtensions.FixDirectory(baseDirectory);
        }

        public IEnumerable<CacheIndex> GetAvailableIndexes()
        {
            if (!Directory.Exists(this.BaseDirectory))
            {
                return Enumerable.Empty<CacheIndex>();
            }

            return Directory
                .EnumerateDirectories(this.BaseDirectory)
                .Select(Path.GetFileName)
                .Select(indexIdString => int.TryParse(indexIdString, out var indexId) ? indexId : -1)
                .Where(indexId => indexId != -1)
                .Cast<CacheIndex>();
        }

        public IEnumerable<int> GetAvailableFileIds(CacheIndex index)
        {
            var indexDirectory = this.GetIndexDirectory(index);

            if (!Directory.Exists(indexDirectory))
            {
                return Enumerable.Empty<int>();
            }

            return Directory
                .EnumerateFiles(indexDirectory)
                .Select(Path.GetFileNameWithoutExtension)
                .Select(fileIdString => int.TryParse(fileIdString, out var fileId) ? fileId : -1)
                .Where(fileId => fileId != -1);
        }

        public CacheFile GetFile(CacheIndex index, int fileId)
        {
            var paths = this.GetExistingFilePaths(index, fileId).ToArray();

            if (paths.Length == 0)
            {
                throw new CacheFileNotFoundException($"File {(int)index}/{fileId} does not exist.");
            }
            if (paths.Length > 1)
            {
                Log.Warning($"Multiple valid files where found for {(int)index}/{fileId}. Returning the first one.");
            }

            return new CacheFile(System.IO.File.ReadAllBytes(paths[0]));
        }

        public void PutFile(CacheIndex index, int fileId, CacheFile file)
        {
            // Throw an exception if the output directory is not yet set or does not exist.
            if (string.IsNullOrWhiteSpace(this.BaseDirectory))
            {
                throw new InvalidOperationException("Base directory must be set before writing files.");
            }

            if (file.Data.Length == 0)
            {
                throw new ArgumentException("You cannot put an empty file into a FlatFileCache.");
            }

            var indexDirectory = this.GetIndexDirectory(index);

            // Create index directory when it does not exist yet.
            Directory.CreateDirectory(indexDirectory);

            // Clean existing files (to make sure no variants with different extensions exist).
            foreach (var existingFilePath in this.GetExistingFilePaths(index, fileId))
            {
                if (!this.OverwriteFiles)
                {
                    Log.Debug($"{(int)index}/{fileId} already exists.");
                    return;
                }

                Log.Debug($"Deleting existing {existingFilePath}.");
                System.IO.File.Delete(existingFilePath);
            }

            string? extension;
            if (file.Info.HasEntries)
            {
                // TODO: Add an option to extract entries individually (which generates tons of files but is more useful).
                extension = "entries";
            }
            else
            {
                extension = ExtensionGuesser.GuessExtension(file.Data);
            }
            extension = extension != null ? $".{extension}" : "";

            var filePath = $"{indexDirectory}{fileId}{extension}";
            System.IO.File.WriteAllBytes(filePath, file.Data);

            Log.Information($"Wrote {(int)index}/{fileId}{extension}.");
        }

        private IEnumerable<string> GetExistingFilePaths(CacheIndex cacheIndex, int fileId)
        {
            return Directory
                .EnumerateFiles(this.GetIndexDirectory(cacheIndex), $"{fileId}*")
                // Filter out false-positivies like 2345 when looking for 234.ext
                .Where(matchedFilePath => Regex.IsMatch(matchedFilePath, $@"[/\\]{fileId}(\..+)?$"));
        }

        private string GetIndexDirectory(CacheIndex cacheIndex)
        {
            return $"{this.BaseDirectory}{(int)cacheIndex}/";
        }

        public void Dispose()
        {
        }
    }
}
