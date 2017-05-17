using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.FlatFile
{
    [Obsolete("Unfinished")]
    public class FlatFileCache : ReferenceTableCache
    {
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

        public override IEnumerable<Index> GetIndexes()
        {
            return Directory.EnumerateDirectories(this._baseDirectory)
                .Select(Path.GetFileName)
                .Select(indexIdString =>
                {
                    int value;
                    return int.TryParse(indexIdString, out value) ? value : -1;
                })
                .Where(indexId => indexId != -1)
                .Cast<Index>();
        }

        public FlatFileCache(string baseDirectory)
        {
            this.BaseDirectory = baseDirectory;
        }

        protected override BinaryFile GetBinaryFile(CacheFileInfo fileInfo)
        {
            // TODO: Add some fallback mechanic for when a file but no info is added

            // Single file
            if (!fileInfo.HasEntries)
            {
                return new BinaryFile
                {
                    Info = fileInfo,
                    Data = File.ReadAllBytes(this.GetExistingFilePaths(fileInfo.Index, fileInfo.FileId).First())
                };
            }

            // Entries
            var entryFile = new EntryFile
            {
                Info = fileInfo
            };

            foreach (var existingEntryPath in this.GetExistingEntryPaths(fileInfo.Index, fileInfo.FileId))
            {
                var entryFileEntry = new BinaryFile
                {
                    Data = File.ReadAllBytes(existingEntryPath.Value)
                };

                entryFile.AddEntry(existingEntryPath.Key, entryFileEntry);
            }

            // TODO: Return EntryFile directly so it doesn't have to be needlessly encoded
            return entryFile.ToBinaryFile();
        }

        protected override void PutBinaryFile(BinaryFile file)
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
            foreach (var existingFilePath in this.GetExistingFilePaths(file.Info.Index, file.Info.FileId))
            {
                File.Delete(existingFilePath);
            }

            var entryDirectory = this.GetEntryDirectory(file.Info.Index, file.Info.FileId);

            if (Directory.Exists(entryDirectory))
            {
                Directory.Delete(entryDirectory, true);
            }

            if (!file.Info.HasEntries)
            {
                // Extract file
                if (file.Data.Length > 0)
                {
                    var extension = ExtensionGuesser.GuessExtension(file.Data);
                    extension = extension != null ? $".{extension}" : "";

                    var filePath = $"{indexDirectory}{file.Info.FileId}{extension}";
                    File.WriteAllBytes(filePath, file.Data);

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

                if (entryFile.EntryCount > 0)
                {
                    Directory.CreateDirectory(entryDirectory);

                    var entryBinaryFiles = entryFile.GetEntries<BinaryFile>();
                    foreach (var entryBinaryFile in entryBinaryFiles)
                    {
                        var extension = ExtensionGuesser.GuessExtension(entryBinaryFile.Data);
                        extension = extension != null ? $".{extension}" : "";

                        var entryPath = $"{entryDirectory}/{entryBinaryFile.Info.EntryId}{extension}";
                        File.WriteAllBytes(entryPath, entryBinaryFile.Data);
                    }

                    FlatFileCache.Logger.Info($"Wrote {(int)file.Info.Index}/{file.Info.FileId} ({entryBinaryFiles.Length} entries).");
                }
                else
                {
                    FlatFileCache.Logger.Info($"Did not write {(int)file.Info.Index}/{file.Info.FileId} because it contains no entries.");
                }
            }
        }

        private IEnumerable<string> GetExistingFilePaths(Index index, int fileId)
        {
            return Directory.EnumerateFiles(this.GetIndexDirectory(index), $"{fileId}*")
                // Filter out false-positivies like 2345 when looking for 234.ext
                .Where(matchedFilePath => Regex.IsMatch(matchedFilePath, $@"[/\\]{fileId}(\..+)?$"));
        }

        private SortedDictionary<int, string> GetExistingEntryPaths(Index index, int fileId)
        {
            var unsortedDictionary = Directory.EnumerateFiles(this.GetEntryDirectory(index, fileId))
                .Where(matchedFilePath => Regex.IsMatch(matchedFilePath, @"[/\\]\d+(\..+)?$"))
                .ToDictionary(matchedFilePath =>
                {
                    var matches = Regex.Matches(matchedFilePath, @"[/\\](\d+)(\..+)?$");

                    return int.Parse(matches[1].Value);
                }, matchedFilePath => matchedFilePath);

            return new SortedDictionary<int, string>(unsortedDictionary);
        }

        private string GetIndexDirectory(Index index)
        {
            return $"{this.BaseDirectory}{(int)index}/";
        }

        private string GetEntryDirectory(Index index, int fileId)
        {
            return this.GetIndexDirectory(index) + fileId + "/";
        }
    }
}
