using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using NVorbis;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Enums;

namespace Villermen.RuneScapeCacheTools.Audio
{
    /// <summary>
    ///     Contains tools for obtaining and combining soundtracks from the cache.
    /// </summary>
    public class Soundtrack
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Soundtrack));

        /// <summary>
        ///     Used in the generation of temporary filenames.
        /// </summary>
        private readonly Random _random = new Random();

        public Soundtrack(CacheBase cache)
        {
            Cache = cache;
        }

        public CacheBase Cache { get; set; }

        /// <summary>
        ///     Combines and exports the soundtracks from the audio chunks in archive 40 into full soundtrack files.
        /// </summary>
        /// <param name="overwriteExisting">
        ///     If true, export and overwrite existing files. Overwriting is done regardless for files
        ///     that have a changed version.
        /// </param>
        /// <param name="nameFilters">
        ///     If non-null, only soundtrack names that contain one the case-insensitive strings will be
        ///     extracted.
        /// </param>
        /// <returns></returns>
        public async Task ExportTracksAsync(bool overwriteExisting = false, IEnumerable<string> nameFilters = null)
        {
            var trackNames = GetTrackNames();
            var outputDirectory = Cache.OutputDirectory + "soundtrack/";

            Directory.CreateDirectory(outputDirectory);
            Directory.CreateDirectory(Cache.TemporaryDirectory);

            Soundtrack.Logger.Info("Done obtaining soundtrack names and file ids.");

            if (nameFilters != null)
            {
                trackNames = trackNames.Where(
                    trackName => nameFilters.Any(
                        nameFilter => trackName.Value.IndexOf(nameFilter, StringComparison.CurrentCultureIgnoreCase) >= 0))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
            }

            await Task.Run(() => Parallel.ForEach(trackNames, trackNamePair =>
            {
                var outputFilename = $"{trackNamePair.Value}.ogg";
                var outputPath = Path.Combine(outputDirectory, outputFilename);

                try
                {
                    var jagaCacheFile = Cache.GetFile(Index.Music, trackNamePair.Key);

                    // Skip file if not overwriting existing and the file exists
                    if (!overwriteExisting && File.Exists(outputPath))
                    {
                        // But only if the version of the file is unchanged
                        var existingVersion = GetVersionFromExportedTrackFile(outputPath);

                        if (existingVersion == jagaCacheFile.Version)
                        {
                            Soundtrack.Logger.Info($"Skipping {outputFilename} because it already exists and version is unchanged.");
                            return;
                        }
                    }

                    var jagaFile = new JagaFile(jagaCacheFile.Data);

                    // Obtain names for the temporary files. We can't use the id as filename, because we are going full parallel.
                    var randomTemporaryFilenames = GetRandomTemporaryFilenames(jagaFile.ChunkCount);

                    // Write out the files
                    File.WriteAllBytes(randomTemporaryFilenames[0], jagaFile.ContainedChunkData);

                    for (var chunkIndex = 1; chunkIndex < jagaFile.ChunkCount; chunkIndex++)
                    {
                        File.WriteAllBytes(randomTemporaryFilenames[chunkIndex],
                            Cache.GetFile(Index.Music, jagaFile.ChunkDescriptors[chunkIndex].FileId).Data);
                    }

                    // Delete existing file because oggCat doesn't do overwriting properly
                    File.Delete(outputPath);

                    // Combine the files using oggCat
                    var combineProcess = new Process
                    {
                        StartInfo =
                        {
                            FileName = "oggCat",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            Arguments =
                                $"-c \"EXTRACTED_BY=Viller\\'s RuneScape Cache Tools;VERSION={jagaCacheFile.Version}\" " +
                                $"\"{outputPath}\" " +
                                "\"" + string.Join("\" \"", randomTemporaryFilenames) + "\""
                        }
                    };

                    combineProcess.Start();
                    combineProcess.WaitForExit();

                    // Remove temporary files
                    foreach (var randomTemporaryFilename in randomTemporaryFilenames)
                    {
                        File.Delete(randomTemporaryFilename);
                    }

                    if (combineProcess.ExitCode != 0)
                    {
                        var soundtrackException =
                            new SoundtrackException(
                                $"oggCat returned with error code {combineProcess.ExitCode} for {outputFilename}.");
                        Soundtrack.Logger.Error(soundtrackException.Message, soundtrackException);
                        throw soundtrackException;
                    }

                    Soundtrack.Logger.Info($"Combined {outputFilename}.");
                }
                catch (CacheException)
                {
                    Soundtrack.Logger.Info($"Skipped {outputFilename} because of corrupted or incomplete data.");
                }
            }));

            Soundtrack.Logger.Info("Done combining soundtracks.");
        }

        /// <summary>
        ///     Returns the track names and their corresponding jaga file id in index 40.
        ///     Track names are made filename-safe, and empty ones are filtered out.
        /// </summary>
        /// <returns></returns>
        public IDictionary<int, string> GetTrackNames()
        {
            // Read out the two enums that, when combined, make up the awesome lookup table
            var trackNames = new EnumFile(Cache.GetFile(Index.Enums, 5).Entries[65]);
            var jagaFileIds = new EnumFile(Cache.GetFile(Index.Enums, 5).Entries[71]);

            // Sorted on key, because then duplicate renaming will be as consistent as possible when names are added
            var result = new SortedDictionary<int, string>();
            foreach (var trackNamePair in trackNames)
            {
                var trackName = (string)trackNamePair.Value;

                if (!jagaFileIds.ContainsKey(trackNamePair.Key))
                {
                    continue;
                }

                var trackFileId = (int)jagaFileIds[trackNamePair.Key];

                // Make trackName filename-safe
                foreach (var invalidChar in Path.GetInvalidFileNameChars())
                {
                    trackName = trackName.Replace(invalidChar.ToString(), "");
                }

                // Don't add empty filenames to the array
                if (string.IsNullOrWhiteSpace(trackName))
                {
                    continue;
                }

                if (!result.ContainsKey(trackFileId))
                {
                    result.Add(trackFileId, trackName);
                }
                else
                {
                    result[trackFileId] = trackName;
                }
            }

            // Rename duplicate names, as those are a thing apparently...
            var duplicateNameGroups = result
                .GroupBy(pair => pair.Value)
                .Where(group => group.Count() > 1)
                .Select(group => group.Skip(1));
            // Select only the second and up, because the first one doesn't have to be renamed

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
            var vorbisReader = new VorbisReader(path);

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

            throw new SoundtrackException("No version comment in specified file.");
        }

        private string[] GetRandomTemporaryFilenames(int amountOfNames)
        {
            const string validChars = @"abcdefghijklmnopqrstuvwxyz0123456789-_()&^%$#@![]{},`~=+";
            const int nameLength = 16;
            var result = new string[amountOfNames];

            for (var i = 0; i < amountOfNames; i++)
            {
                string newPath;
                do
                {
                    newPath =
                        new string(
                            Enumerable.Repeat(validChars, nameLength).Select(s => s[_random.Next(s.Length)]).ToArray());
                    newPath = Cache.TemporaryDirectory + newPath + ".ogg";
                }
                while (File.Exists(newPath) || result.Contains(newPath));

                result[i] = newPath;
            }

            return result;
        }
    }
}