using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.FlatFile
{
    [Obsolete("Unfinished")]
    public class FlatFileCache : CacheBase
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

        protected override BinaryFile GetFile(Index index, int fileId)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<int> GetFileIds(Index index)
        {
            throw new NotImplementedException();
        }

        protected override void PutFile(BinaryFile file)
        {
            // Throw an exception if the output directory is not yet set or does not exist
            if (string.IsNullOrWhiteSpace(this.BaseDirectory))
            {
                throw new InvalidOperationException("Base directory must be set before writing files.");
            }

            var indexDirectory = $"{this.BaseDirectory}{(int)file.Info.Index}/";
            var entryDirectory = $"{indexDirectory}{file.Info.FileId}/";

            // Create index directory for when it does not exist yet
            Directory.CreateDirectory(indexDirectory);

            // Clean existing files/entries

            // Get all files that start with the given fileId
            var matchedExistingFiles = Directory.EnumerateFiles(indexDirectory, $"{file.Info.FileId}*")
                // Filter out false-positivies like 2345 when looking for 234.ext
                .Where(matchedFile => Regex.IsMatch(matchedFile, $@"[/\\]{file.Info.FileId}(\..+)?$"))
                .ToList();

            foreach (var matchedExistingFile in matchedExistingFiles)
            {
                File.Delete(matchedExistingFile);
            }

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

        public override CacheFileInfo GetFileInfo(Index index, int fileId)
        {
            throw new NotImplementedException();
        }
    }
}