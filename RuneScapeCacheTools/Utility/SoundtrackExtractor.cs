using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FlacLibSharp;
using NVorbis;
using Serilog;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Utility
{
    /// <summary>
    /// Contains tools for obtaining and combining audio from the cache.
    /// </summary>
    public class SoundtrackExtractor
    {
        private string _temporaryDirectory;

        /// <summary>
        /// Temporary files used while processing will be stored here.
        /// </summary>
        public string TemporaryDirectory
        {
            get => this._temporaryDirectory;
            set => this._temporaryDirectory = PathExtensions.FixDirectory(value);
        }

        /// <summary>
        /// Temporary files used while processing will be stored here.
        /// </summary>
        public string OutputDirectory
        {
            get => this._outputDirectory;
            set => this._outputDirectory = PathExtensions.FixDirectory(value);
        }

        /// <summary>
        /// Used in the generation of temporary filenames.
        /// </summary>
        private readonly Random _random = new Random();

        private string _outputDirectory;

        public SoundtrackExtractor(ReferenceTableCache cache, string outputDirectory)
        {
            this.Cache = cache;
            this.TemporaryDirectory = Path.GetTempPath() + "rsct";
            this.OutputDirectory = outputDirectory;
        }

        public ReferenceTableCache Cache { get; set; }

        /// <summary>
        /// Combines and exports the soundtracks from the audio chunks in archive 40 into full soundtrack files.
        /// </summary>
        /// <param name="overwrite">
        /// Whether to export and overwrite existing files. Overwriting is always done for files that have a newer version.
        /// </param>
        /// <param name="lossless">Export as FLAC for a (minor) quality improvement.</param>
        /// <param name="includeUnnamed">
        /// Extract files that are not named or have an empty name as their file id.
        /// </param>
        /// <param name="trackNameFilters">
        /// When passed, only soundtrack names that contain one of the given case-insensitive strings will be extracted.
        /// </param>
        /// <param name="parallelism">Maximum amount of jobs to run at once.</param>
        public void ExtractSoundtrack(bool overwrite, bool lossless, bool includeUnnamed, string[] trackNameFilters, int parallelism)
        {
            IEnumerable<KeyValuePair<int, string>> trackNames = this.GetTrackNames(includeUnnamed);

            Log.Information("Obtained soundtrack names and file IDs.");

            if (trackNameFilters.Length > 0)
            {
                trackNames = trackNames.Where(trackNamePair => trackNameFilters.Any(nameFilter =>
                    trackNamePair.Value.IndexOf(nameFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                );
            }

            // Obtain the file's version to check if it has to be extracted _before_ obtaining the actual file.
            trackNames = trackNames.Where(trackNamePair =>
            {
                var fileInfo = this.Cache.GetFileInfo(CacheIndex.Music, trackNamePair.Key);
                var outputPath = this.GetOutputPath(trackNamePair.Value, lossless);
                return this.IsExtractionRequired(outputPath, fileInfo.Version, overwrite);
            });

            Parallel.ForEach(
                trackNames,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = parallelism,
                },
                trackNamePair =>
                {
                    Log.Information($"Combining {trackNamePair.Value}...");
                    var jagaCacheFile = this.Cache.GetFile(CacheIndex.Music, trackNamePair.Key);
                    this.ExtractIfJagaFile(jagaCacheFile, trackNamePair.Value, overwrite, lossless);
                }
            );
        }

        public void ExtractIfJagaFile(CacheFile cacheFile, string trackName, bool overwrite = false, bool lossless = false)
        {
            if (!ExtensionGuesser.DataHasMagicNumber(cacheFile.Data, JagaFile.MagicNumber))
            {
                return;
            }

            var compressionQuality = lossless ? 8 : 6;
            var outputPath = this.GetOutputPath(trackName, lossless);

            if (!this.IsExtractionRequired(outputPath, cacheFile.Info.Version, overwrite))
            {
                Log.Debug($"Skipped {trackName} because it already exists with the same version.");
                return;
            }

            try
            {
                Directory.CreateDirectory(this.OutputDirectory);
                Directory.CreateDirectory(this.TemporaryDirectory);

                var jagaFile = JagaFile.Decode(cacheFile.Data);

                // Obtain random paths for chunk files. We can't use the IDs because we are going full parallel.
                var chunkPaths = this.GetTemporaryFilenames(jagaFile.ChunkCount);

                // Write out the files
                System.IO.File.WriteAllBytes(chunkPaths[0], jagaFile.ContainedChunkData);
                for (var chunkIndex = 1; chunkIndex < jagaFile.ChunkCount; chunkIndex++)
                {
                    var chunkFile = this.Cache.GetFile(CacheIndex.Music, jagaFile.ChunkDescriptors[chunkIndex].FileId);
                    System.IO.File.WriteAllBytes(chunkPaths[chunkIndex], chunkFile.Data);
                }

                // Delete existing file in case combiner application doesn't do overwriting properly.

                // We write to a temporary file and then move it to the output path to be sure the file is fully
                // processed first.
                var temporaryOutputPath = this.GetTemporaryFilenames(1)[0] + (lossless ? ".flac" : ".ogg");

                // Create argument to supply to SoX (http://sox.sourceforge.net/sox.html)
                var soxArguments = new List<string>();
                foreach (var chunkFilename in chunkPaths)
                {
                    soxArguments.Add($"\"{chunkFilename}\"");
                }
                soxArguments.Add($"--add-comment \"VERSION={cacheFile.Info.Version}\"");
                soxArguments.Add($"--add-comment \"TITLE={trackName}\"");
                soxArguments.Add("--add-comment \"ALBUM=RuneScape Original Soundtrack\"");
                soxArguments.Add("--add-comment \"GENRE=Game\"");
                soxArguments.Add("--add-comment \"COMMENT=Extracted by Viller's RuneScape Cache Tools\"");
                soxArguments.Add("--add-comment \"COPYRIGHT=Jagex Ltd.\"");
                soxArguments.Add($"-C {compressionQuality}");
                soxArguments.Add($"\"{temporaryOutputPath}\"");

                // Combine the chunks.
                var combineProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = "sox",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        // RedirectStandardError = true,
                        Arguments = string.Join(" ", soxArguments),
                    }
                };

                Log.Debug($"Running sox with {soxArguments.Count} chunks.");

                combineProcess.Start();

                // combineProcess.ErrorDataReceived += (sender, args) =>
                // {
                //     if (!string.IsNullOrEmpty(args.Data))
                //     {
                //         Log.Debug($"[SoX] {args.Data}");
                //     }
                // };
                // combineProcess.BeginErrorReadLine();

                combineProcess.WaitForExit();

                // Remove temporary files.
                foreach (var temporaryFilename in chunkPaths)
                {
                    System.IO.File.Delete(temporaryFilename);
                }

                if (combineProcess.ExitCode != 0)
                {
                    throw new SoundtrackException($"SoX returned with error code {combineProcess.ExitCode} for {trackName}.")
                    {
                        IsSoxError = true
                    };
                }

                // Move output to correct path.
                System.IO.File.Delete(outputPath);
                System.IO.File.Move(temporaryOutputPath, outputPath);

                Log.Information($"Combined {trackName}.");
            }
            catch (CacheFileNotFoundException)
            {
                Log.Information($"Skipped {trackName} because of incomplete data.");
            }
            catch (Win32Exception exception)
            {
                throw new SoundtrackException(
                    "Could not find or run SoX. Is it installed or added to the PATH?",
                    exception
                );
            }
        }

        /// <summary>
        /// Returns the track names and their corresponding <see cref="JagaFile" /> ID in index 40. Track names are made
        /// filename safe, and empty ones are filtered out.
        /// </summary>
        /// <returns></returns>
        public SortedDictionary<int, string> GetTrackNames(bool includeUnnamed = false)
        {
            // Read out the two enums that, when combined, make up the awesome lookup table
            var enumFile = this.Cache.GetFile(CacheIndex.Enums, 5);

            // int track id : string track name
            var trackNames = EnumFile.Decode(enumFile.Entries[65]);
            // int track id : int jaga file id
            var jagaFileIds = EnumFile.Decode(enumFile.Entries[71]);

            // Result is sorted on key to let duplicate renaming be as consistent as possible
            var result = new SortedDictionary<int, string>();

            // Loop through jaga file ids as opposed to tracknames for when unnamed files are also included
            foreach (var jagaFileIdPair in jagaFileIds)
            {
                var jagaFileId = (int)jagaFileIdPair.Value;
                var trackId = jagaFileIdPair.Key;
                var validName = false;
                var trackName = "";

                // Disregard the default value
                // TODO: With more experience with enums, try to determine if a check can be made in the EnumFile type instead of here
                if (jagaFileId == jagaFileIds.DefaultInteger)
                {
                    continue;
                }

                if (trackNames.ContainsKey(trackId))
                {
                    trackName = (string)trackNames[trackId];

                    // Make obtained name filename-safe
                    foreach (var invalidChar in PathExtensions.InvalidCharacters)
                    {
                        trackName = trackName.Replace(invalidChar.ToString(), "");
                    }

                    // Set to add name if its still valid after cleanup
                    if (!string.IsNullOrWhiteSpace(trackName))
                    {
                        validName = true;
                    }
                }

                // Set name of the track to the JAGA file id if allowed, or skip adding it
                if (!validName)
                {
                    if (!includeUnnamed)
                    {
                        continue;
                    }

                    trackName = jagaFileId.ToString();
                }

                // Log a message if valid and another valid name already maps to this JAGA file, and overwrite the name
                if (result.ContainsKey(jagaFileId))
                {
                    if (validName && trackName != result[jagaFileId])
                    {
                        Log.Warning($"A soundtrack name pointing to the same file has already been added, overwriting {result[jagaFileId]} with {trackName}.");

                        result[jagaFileId] = trackName;
                    }

                    continue;
                }

                result.Add(jagaFileId, trackName);
            }

            // Rename duplicate names, as those are a thing apparently...
            var duplicateNameGroups = result
                .GroupBy(pair => pair.Value)
                .Where(group => group.Count() > 1)
                // Select only the second and up, because the first one doesn't have to be renamed
                .Select(group => group.Skip(1));

            foreach (var duplicateNameGroup in duplicateNameGroups)
            {
                var duplicateId = 2;
                foreach (var duplicateNamePair in duplicateNameGroup)
                {
                    result[duplicateNamePair.Key] = $"{duplicateNamePair.Value} ({duplicateId++})";
                }
            }

            return result;
        }

        public int GetVersionFromExportedTrackFile(string path)
        {
            if (path.EndsWith(".ogg"))
            {
                using var vorbisReader = new VorbisReader(path);

                if (!string.IsNullOrEmpty(vorbisReader.Tags.Version))
                {
                    return int.Parse(vorbisReader.Tags.Version);
                }
            }
            else if (path.EndsWith(".flac"))
            {
                using var flacFile = new FlacFile(path);
                if (flacFile.VorbisComment != null && flacFile.VorbisComment.ContainsField("VERSION"))
                {
                    return int.Parse(flacFile.VorbisComment["VERSION"].Value);
                }
            }

            throw new SoundtrackException("No version comment in specified file.");
        }

        private string[] GetTemporaryFilenames(int amount)
        {
            const string validChars = @"abcdefghijklmnopqrstuvwxyz0123456789-_()&^#@![]{},~=+";
            const int nameLength = 16;
            var result = new string[amount];

            for (var i = 0; i < amount; i++)
            {
                string newPath;
                do
                {
                    newPath =
                        new string(
                            Enumerable.Repeat(validChars, nameLength).Select(s => s[this._random.Next(s.Length)]).ToArray());
                    newPath = $"{this.TemporaryDirectory}{newPath}.ogg";
                }
                while (System.IO.File.Exists(newPath) || result.Contains(newPath));

                result[i] = newPath;
            }

            return result;
        }

        private bool IsExtractionRequired(string outputPath, int? version, bool overwrite)
        {
            // Extraction is always required when overwriting or the file does not exist yet.
            if (overwrite || !System.IO.File.Exists(outputPath))
            {
                return true;
            }

            // Extraction is required only if the file's version does not match.
            var existingVersion = this.GetVersionFromExportedTrackFile(outputPath);
            return existingVersion != version;
        }

        private string GetOutputPath(string trackName, bool lossless)
        {
            return $"{this.OutputDirectory}{trackName}.{(lossless ? "flac" : "ogg")}";
        }
    }
}
