using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Villermen.RuneScapeCacheTools.Extension;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.Cache.FlatFile
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
            // This might match more than one file (e.g., one with a different extension). Just take the first one.
            var path = this.GetExistingFilePaths(index, fileId).First();
            return new CacheFile(System.IO.File.ReadAllBytes(path));
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
                System.IO.File.Delete(existingFilePath);
            }

            var extension = ExtensionGuesser.GuessExtension(file.Data);
            extension = extension != null ? $".{extension}" : "";

            var filePath = $"{indexDirectory}{fileId}{extension}";
            System.IO.File.WriteAllBytes(filePath, file.Data);
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
    }
}
