using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.FlatFile
{
    public class FlatFileCache : CacheBase
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

        public override IEnumerable<int> GetFileIds(Index index)
        {
            return Directory.EnumerateFileSystemEntries(this.GetIndexDirectory(index))
                .Where(fileSystemEntry =>
                    FlatFileCache.FileNameRegex.IsMatch(fileSystemEntry))
                .Select(fileSystemEntry => 
                    int.Parse(FlatFileCache.FileNameRegex.Match(fileSystemEntry).Groups[1].Value));
        }

        public override CacheFileInfo GetFileInfo(Index index, int fileId)
        {
            var info = new CacheFileInfo
            {
                Index = index,
                FileId = fileId
            };
            
            var filePath = this.GetExistingFilePaths(index, fileId).FirstOrDefault();
            if (filePath != null)
            {
                var filePathInfo = new FileInfo(filePath);

                info.CompressionType = CompressionType.None;
                info.UncompressedSize = (int)filePathInfo.Length;

                return info;
            }

            var entryPaths = this.GetExistingEntryPaths(index, fileId);
            if (entryPaths.Any())
            {
                // Just add as many entries without identifiers to the info as the capacity dictates
                var capacity = this.GetEntryCapacity(index, fileId);

                if (capacity != null)
                {
                    info.Entries = new CacheFileEntryInfo[capacity.Value];
                }
                else
                {
                    info.Entries = new CacheFileEntryInfo[entryPaths.Keys.Max() + 1];
                }

                for (var entryId = 0; entryId < info.Entries.Length; entryId++)
                {
                    info.Entries[entryId] = new CacheFileEntryInfo
                    {
                        EntryId = entryId
                    };
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

        protected override BinaryFile GetBinaryFile(CacheFileInfo fileInfo)
        {
            // Single file
            if (fileInfo.Entries == null)
            {
                return new BinaryFile
                {
                    Info = fileInfo,
                    Data = File.ReadAllBytes(this.GetExistingFilePaths(fileInfo.Index, fileInfo.FileId.Value).First())
                };
            }

            // Entries
            var entryFile = new EntryFile
            {
                Info = fileInfo
            };

            foreach (var existingEntryPath in this.GetExistingEntryPaths(fileInfo.Index, fileInfo.FileId.Value))
            {
                entryFile.AddEntry(existingEntryPath.Key, File.ReadAllBytes(existingEntryPath.Value));
            }

            var capacity = this.GetEntryCapacity(fileInfo.Index, fileInfo.FileId.Value);
            
            if (capacity != null)
            {
                entryFile.Capacity = capacity.Value;
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
            foreach (var existingFilePath in this.GetExistingFilePaths(file.Info.Index, file.Info.FileId.Value))
            {
                File.Delete(existingFilePath);
            }

            var entryDirectory = this.GetEntryDirectory(file.Info.Index, file.Info.FileId.Value);

            if (Directory.Exists(entryDirectory))
            {
                Directory.Delete(entryDirectory, true);
            }

            if (file.Info.Entries == null)
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

                if (!entryFile.Empty)
                {
                    Directory.CreateDirectory(entryDirectory);

                    var entryBinaryFiles = entryFile.GetEntries<BinaryFile>().ToList();
                    foreach (var entryBinaryFile in entryBinaryFiles)
                    {
                        var extension = ExtensionGuesser.GuessExtension(entryBinaryFile.Data);
                        extension = extension != null ? $".{extension}" : "";

                        var entryPath = $"{entryDirectory}{entryBinaryFile.Info.EntryId}{extension}";
                        File.WriteAllBytes(entryPath, entryBinaryFile.Data);
                    }
                    
                    // Write the capacity
                    File.WriteAllText($"{entryDirectory}.capacity", entryFile.Capacity.ToString());

                    FlatFileCache.Logger.Info($"Wrote {(int)file.Info.Index}/{file.Info.FileId} ({entryBinaryFiles.Count} entries).");
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
            try
            {
                var unsortedDictionary = Directory.EnumerateFiles(this.GetEntryDirectory(index, fileId))
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
                throw new FileNotFoundException($"Directory for entry {(int)index}/{fileId} does not exist.", exception);
            }
        }

        private string GetIndexDirectory(Index index)
        {
            return $"{this.BaseDirectory}{(int)index}/";
        }

        private string GetEntryDirectory(Index index, int fileId)
        {
            return this.GetIndexDirectory(index) + fileId + "/";
        }

        private int? GetEntryCapacity(Index index, int fileId)
        {
            var capacityFile = this.GetEntryDirectory(index, fileId) + ".capacity";
            if (File.Exists(capacityFile))
            {
                return int.Parse(File.ReadAllText(capacityFile));
            }

            return null;
        }
    }
}
