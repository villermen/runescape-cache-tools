using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FlacLibSharp;
using log4net;
using NVorbis;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Extension;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Utility
{
    /// <summary>
    ///     Contains tools for obtaining and combining soundtracks from the cache.
    /// </summary>
    public class Soundtrack
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Soundtrack));

        private string _temporaryDirectory;

        /// <summary>
        ///     Temporary files used while processing will be stored here.
        /// </summary>
        public string TemporaryDirectory
        {
            get { return this._temporaryDirectory; }
            set { this._temporaryDirectory = PathExtensions.FixDirectory(value); }
        }

        /// <summary>
        ///     Temporary files used while processing will be stored here.
        /// </summary>
        public string OutputDirectory
        {
            get { return this._outputDirectory; }
            set { this._outputDirectory = PathExtensions.FixDirectory(value); }
        }

        /// <summary>
        ///     Used in the generation of temporary filenames.
        /// </summary>
        private readonly Random _random = new Random();

        private string _outputDirectory;

        public Soundtrack(BaseCache cache, string outputDirectory)
        {
            this.Cache = cache;
            this.TemporaryDirectory = Path.GetTempPath() + "rsct";
            this.OutputDirectory = outputDirectory;
        }

        public BaseCache Cache { get; set; }

        /// <summary>
        ///     Combines and exports the soundtracks from the audio chunks in archive 40 into full soundtrack files.
        /// </summary>
        /// <param name="overwriteExisting">
        ///     If true, export and overwrite existing files. Overwriting is done regardless for files
        ///     that have a changed version.
        /// </param>
        /// <param name="lossless"></param>
        /// <param name="includeUnnamed">If this is set to true, files that are not named or have an empty name are named after their file id.</param>
        /// <param name="nameFilters">
        ///     If non-null, only soundtrack names that contain one of the given case-insensitive strings will be
        ///     extracted.
        /// </param>
        /// <returns></returns>
        public void Extract(bool overwriteExisting = false, bool lossless = false, bool includeUnnamed = false, params string[] nameFilters)
        {
            var trackNames = this.GetTrackNames(includeUnnamed);
            var outputExtension = lossless ? "flac" : "ogg";
            var compressionQuality = lossless ? 8 : 6;

            Directory.CreateDirectory(this.OutputDirectory);
            Directory.CreateDirectory(this.TemporaryDirectory);

            Soundtrack.Logger.Info("Done obtaining soundtrack names and file ids.");

            if (nameFilters.Length > 0)
            {
                trackNames = trackNames.Where(
                        trackName => nameFilters.Any(
                            nameFilter => trackName.Value.IndexOf(nameFilter, StringComparison.CurrentCultureIgnoreCase) >= 0))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
            }

            try
            {
                Parallel.ForEach(
                    trackNames,
                    new ParallelOptions
                    {
                        // At a certain point (~16 threads?) the server will eventually start dropping requests (HTTP)
                        MaxDegreeOfParallelism = 10,
                    },
                    trackNamePair =>
                    {
                        var outputFilename = $"{trackNamePair.Value}.{outputExtension}";
                        var outputPath = this.OutputDirectory + outputFilename;

                        try
                        {
                            // Get file info first, for a less IO intensive availability check
                            var jagaFileInfo = this.Cache.GetFileInfo(Index.Music, trackNamePair.Key);

                            // Skip file if not overwriting existing and the file exists
                            if (!overwriteExisting && System.IO.File.Exists(outputPath))
                            {
                                // But only if the version of the file is unchanged
                                var existingVersion = this.GetVersionFromExportedTrackFile(outputPath);

                                if (existingVersion == jagaFileInfo.Version)
                                {
                                    var logMethod = nameFilters.Length > 0 ? (Action<string>)Soundtrack.Logger.Info : Soundtrack.Logger.Debug;

                                    logMethod($"Skipped {outputFilename} because it already exists and version is unchanged.");
                                    return;
                                }
                            }

                            var jagaFile = this.Cache.GetFile<JagaFile>(Index.Music, trackNamePair.Key);

                            // Obtain names for the temporary files. We can't use the id as filename, because we are going full parallel.
                            var randomTemporaryFilenames = this.GetRandomTemporaryFilenames(jagaFile.ChunkCount);

                            // Write out the files
                            System.IO.File.WriteAllBytes(randomTemporaryFilenames[0], jagaFile.ContainedChunkData);

                            for (var chunkIndex = 1; chunkIndex < jagaFile.ChunkCount; chunkIndex++)
                            {
                                System.IO.File.WriteAllBytes(randomTemporaryFilenames[chunkIndex], this.Cache.GetFile<BinaryFile>(Index.Music, jagaFile.ChunkDescriptors[chunkIndex].FileId).Data);
                            }

                            // Delete existing file in case combiner application doesn't do overwriting properly
                            System.IO.File.Delete(outputPath);

                            // Create argument to supply to SoX (http://sox.sourceforge.net/sox.html)
                            var soxArguments = $"\"{string.Join("\" \"", randomTemporaryFilenames)}\" " +
                                               $"--add-comment \"TITLE={trackNamePair.Value}\" " +
                                               $"--add-comment \"VERSION={jagaFileInfo.Version}\" " +
                                               "--add-comment \"ALBUM=RuneScape Original Soundtrack\" " +
                                               "--add-comment \"GENRE=Game\" " +
                                               "--add-comment \"COMMENT=Extracted by Viller's RuneScape Cache Tools\" " +
                                               "--add-comment \"COPYRIGHT=Jagex Games Studio\" " +
                                               $"-C {compressionQuality} " +
                                               $" \"{outputPath}\"";

                            // Combine the files
                            var combineProcess = new Process
                            {
                                StartInfo =
                                {
                                    FileName = "sox",
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
#if DEBUG
                                    RedirectStandardError = true,
#endif
                                    Arguments = soxArguments
                                }
                            };

                            Soundtrack.Logger.Debug("sox " + soxArguments);

                            combineProcess.Start();

#if DEBUG
                            combineProcess.ErrorDataReceived += (sender, args) =>
                            {
                                if (!string.IsNullOrEmpty(args.Data))
                                {
                                    Soundtrack.Logger.Debug($"[SoX] {args.Data}");
                                }
                            };
                            combineProcess.BeginErrorReadLine();
#endif

                            combineProcess.WaitForExit();

                            // Remove temporary files
                            foreach (var randomTemporaryFilename in randomTemporaryFilenames)
                            {
                                System.IO.File.Delete(randomTemporaryFilename);
                            }

                            if (combineProcess.ExitCode != 0)
                            {
                                throw new SoundtrackException($"SoX returned with error code {combineProcess.ExitCode} for {outputFilename}.");
                            }

                            Soundtrack.Logger.Info($"Combined {outputFilename}.");
                        }
                        catch (FileNotFoundException)
                        {
                            Soundtrack.Logger.Info($"Skipped {outputFilename} because of incomplete data.");
                        }
                    });
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is Win32Exception)
                {
                    throw new FileNotFoundException("Could not find or run SoX. Is it installed or added to the PATH?", ex.InnerException);
                }

                throw ex.InnerException;
            }

            Soundtrack.Logger.Info("Done combining soundtracks.");
        }

        /// <summary>
        ///     Returns the track names and their corresponding jaga file id in index 40.
        ///     Track names are made filename-safe, and empty ones are filtered out.
        /// </summary>
        /// <returns></returns>
        public IDictionary<int, string> GetTrackNames(bool includeUnnamed = false)
        {
            // Read out the two enums that, when combined, make up the awesome lookup table

            var enumEntries = this.Cache.GetFile<EntryFile>(Index.Enums, 5);

            // int track id : string track name
            var trackNames = enumEntries.GetEntry<EnumFile>(65);
            // int track id : int jaga file id
            var jagaFileIds = enumEntries.GetEntry<EnumFile>(71);

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
                        Soundtrack.Logger.Warn($"A soundtrack name pointing to the same file has already been added, overwriting {result[jagaFileId]} with {trackName}");
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
                using (var vorbisReader = new VorbisReader(path))
                {
                    foreach (var comment in vorbisReader.Comments)
                    {
                        if (!comment.StartsWith("VERSION=", true, null))
                        {
                            continue;
                        }

                        var value = comment.Split('=')[1];
                        var version = int.Parse(value);

                        return version;
                    }
                }
            }
            else if (path.EndsWith(".flac"))
            {
                using (var flacFile = new FlacFile(path))
                {
                    if (flacFile.VorbisComment.ContainsField("VERSION"))
                    {
                        return int.Parse(flacFile.VorbisComment["VERSION"].Value);
                    }
                }
            }

            throw new SoundtrackException("No version comment in specified file.");
        }

        private string[] GetRandomTemporaryFilenames(int amountOfNames)
        {
            const string validChars = @"abcdefghijklmnopqrstuvwxyz0123456789-_()&^#@![]{},~=+";
            const int nameLength = 16;
            var result = new string[amountOfNames];

            for (var i = 0; i < amountOfNames; i++)
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
    }
}
