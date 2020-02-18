using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Villermen.RuneScapeCacheTools.Extension;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.Cache.FlatFile
{
    public class FlatFileCache : ICache
    {
        private static readonly Regex FileNameRegex = new Regex(@"[/\\](\d+)(\..+)?$");

        private static readonly ILog Logger = LogManager.GetLogger(typeof(FlatFileCache));

        private string _baseDirectory;

        /// <summary>
        /// The base directory where all files will be stored in/retrieved from.
        /// </summary>
        public string BaseDirectory
        {
            get { return this._baseDirectory; }
            set { this._baseDirectory = PathExtensions.FixDirectory(value); }
        }

        public override IEnumerable<CacheIndex> GetIndexes()
        {
            if (!Directory.Exists(this.BaseDirectory))
            {
                return Enumerable.Empty<CacheIndex>();
            }

            return Directory.EnumerateDirectories(this.BaseDirectory)
                .Select(Path.GetFileName)
                .Select(indexIdString =>
                {
                    int value;
                    return int.TryParse(indexIdString, out value) ? value : -1;
                })
                .Where(indexId => indexId != -1)
                .Cast<CacheIndex>();
        }

        public override IEnumerable<int> GetFileIds(CacheIndex cacheIndex)
        {
            var indexDirectory = this.GetIndexDirectory(cacheIndex);

            if (!Directory.Exists(indexDirectory))
            {
                return Enumerable.Empty<int>();
            }

            return Directory.EnumerateFileSystemEntries(this.GetIndexDirectory(cacheIndex))
                .Where(fileSystemEntry =>
                    FlatFileCache.FileNameRegex.IsMatch(fileSystemEntry))
                .Select(fileSystemEntry =>
                    int.Parse(FlatFileCache.FileNameRegex.Match(fileSystemEntry).Groups[1].Value));
        }

        public override CacheFileInfo GetFileInfo(CacheIndex cacheIndex, int fileId)
        {
            var info = new CacheFileInfo
            {
                CacheIndex = cacheIndex,
                FileId = fileId
            };

            var filePath = this.GetExistingFilePaths(cacheIndex, fileId).FirstOrDefault();
            if (filePath != null)
            {
                var filePathInfo = new FileInfo(filePath);

                info.CompressionType = CompressionType.None;
                info.UncompressedSize = (int)filePathInfo.Length;
                info.EntryInfo.Add(0, new CacheFileEntryInfo
                {
                    EntryId = 0
                });

                return info;
            }

            var entryPaths = this.GetExistingEntryPaths(cacheIndex, fileId);
            if (entryPaths.Any())
            {
                foreach (var entryId in entryPaths.Keys)
                {
                    info.EntryInfo.Add(entryId,  new CacheFileEntryInfo
                    {
                        EntryId = entryId
                    });
                }

                return info;
            }

            throw new FileNotFoundException("Requested file does not exist.");
        }

        protected override void PutFileInfo(CacheFileInfo fileInfo)
        {
            // Nothing interesting happens.
        }

        public FlatFileCache(string baseDirectory)
        {
            this.BaseDirectory = baseDirectory;
        }

        protected override RawCacheFile GetFile(CacheFileInfo fileInfo)
        {
            // Single file
            if (!fileInfo.UsesEntries)
            {
                return new RawCacheFile
                {
                    Info = fileInfo,
                    Data = System.IO.File.ReadAllBytes(this.GetExistingFilePaths(fileInfo.CacheIndex, fileInfo.FileId.Value).First())
                };
            }

            // Entries
            var entryFile = new EntryFile
            {
                Info = fileInfo
            };

            foreach (var existingEntryPath in this.GetExistingEntryPaths(fileInfo.CacheIndex, fileInfo.FileId.Value))
            {
                entryFile.AddEntry(existingEntryPath.Key, System.IO.File.ReadAllBytes(existingEntryPath.Value));
            }

            // TODO: Return EntryFile directly so it doesn't have to be needlessly encoded
            return entryFile.ToBinaryFile();
        }

        protected override void PutBinaryFile(RawCacheFile file)
        {
            // Throw an exception if the output directory is not yet set or does not exist
            if (string.IsNullOrWhiteSpace(this.BaseDirectory))
            {
                throw new InvalidOperationException("Base directory must be set before writing files.");
            }

            var indexDirectory = this.GetIndexDirectory(file.Info.Index);

            // Create index directory for when it does not exist yet
            Directory.CreateDirectory(indexDirectory);

            // Clean existing files/entries
            foreach (var existingFilePath in this.GetExistingFilePaths(file.Info.Index, file.Info.FileId.Value))
            {
                System.IO.File.Delete(existingFilePath);
            }

            var entryDirectory = this.GetEntryDirectory(file.Info.Index, file.Info.FileId.Value);

            if (Directory.Exists(entryDirectory))
            {
                Directory.Delete(entryDirectory, true);
            }

            if (!file.Info.UsesEntries)
            {
                // Extract file
                if (file.Data.Length > 0)
                {
                    var extension = ExtensionGuesser.GuessExtension(file.Data);
                    extension = extension != null ? $".{extension}" : "";

                    var filePath = $"{indexDirectory}{file.Info.FileId}{extension}";
                    System.IO.File.WriteAllBytes(filePath, file.Data);

                    FlatFileCache.Logger.Info($"Wrote {(int)file.Info.Index}/{file.Info.FileId}.");
                }
                else
                {
                    FlatFileCache.Logger.Info($"Did not write {(int)file.Info.Index}/{file.Info.FileId} because it is empty.");
                }
            }
            else
            {
                // Extract entries
                var entryFile = new EntryFile();
                entryFile.FromBinaryFile(file);

                if (!entryFile.Empty)
                {
                    Directory.CreateDirectory(entryDirectory);

                    var entryBinaryFiles = entryFile.GetEntries<RawCacheFile>().ToList();
                    foreach (var entryBinaryFile in entryBinaryFiles)
                    {
                        var extension = ExtensionGuesser.GuessExtension(entryBinaryFile.Data);
                        extension = extension != null ? $".{extension}" : "";

                        var entryPath = $"{entryDirectory}{entryBinaryFile.Info.EntryId}{extension}";
                        System.IO.File.WriteAllBytes(entryPath, entryBinaryFile.Data);
                    }

                    FlatFileCache.Logger.Info($"Wrote {(int)file.Info.Index}/{file.Info.FileId} ({entryBinaryFiles.Count} entries).");
                }
                else
                {
                    FlatFileCache.Logger.Info($"Did not write {(int)file.Info.Index}/{file.Info.FileId} because it contains no entries.");
                }
            }
        }

        private IEnumerable<string> GetExistingFilePaths(CacheIndex cacheIndex, int fileId)
        {
            return Directory.EnumerateFiles(this.GetIndexDirectory(cacheIndex), $"{fileId}*")
                // Filter out false-positivies like 2345 when looking for 234.ext
                .Where(matchedFilePath => Regex.IsMatch(matchedFilePath, $@"[/\\]{fileId}(\..+)?$"));
        }

        private SortedDictionary<int, string> GetExistingEntryPaths(CacheIndex cacheIndex, int fileId)
        {
            try
            {
                var unsortedDictionary = Directory.EnumerateFiles(this.GetEntryDirectory(cacheIndex, fileId))
                    .Where(matchedFilePath => FlatFileCache.FileNameRegex.IsMatch(matchedFilePath))
                    .ToDictionary(matchedFilePath =>
                    {
                        var match = FlatFileCache.FileNameRegex.Match(matchedFilePath);
                        return int.Parse(match.Groups[1].Value);
                    }, matchedFilePath => matchedFilePath);

                return new SortedDictionary<int, string>(unsortedDictionary);
            }
            catch (DirectoryNotFoundException exception)
            {
                throw new FileNotFoundException($"Directory for entry {(int)cacheIndex}/{fileId} does not exist.", exception);
            }
        }

        private string GetIndexDirectory(CacheIndex cacheIndex)
        {
            return $"{this.BaseDirectory}{(int)cacheIndex}/";
        }

        private string GetEntryDirectory(CacheIndex cacheIndex, int fileId)
        {
            return this.GetIndexDirectory(cacheIndex) + fileId + "/";
        }
    }
}
