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
    using System.Text;

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
        public void Extract(bool overwriteExisting = false, params string[] nameFilters)
        {
            var trackNames = GetTrackNames();
            var outputDirectory = Cache.OutputDirectory + "soundtrack/";

            Directory.CreateDirectory(outputDirectory);
            Directory.CreateDirectory(Cache.TemporaryDirectory);

            Logger.Info("Done obtaining soundtrack names and file ids.");

            if (nameFilters.Length > 0)
            {
                trackNames = trackNames.Where(
                        trackName => nameFilters.Any(
                            nameFilter => trackName.Value.IndexOf(nameFilter, StringComparison.CurrentCultureIgnoreCase) >= 0))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
            }

            Parallel.ForEach(trackNames, trackNamePair =>
            {
                var outputFilename = $"{trackNamePair.Value}.ogg";
                var outputPath = Path.Combine(outputDirectory, outputFilename);

                try
                {
                    var jagaFileInfo = Cache.GetFileInfo(Index.Music, trackNamePair.Key);

                    // Skip file if not overwriting existing and the file exists
                    if (!overwriteExisting && File.Exists(outputPath))
                    {
                        // But only if the version of the file is unchanged
                        var existingVersion = GetVersionFromExportedTrackFile(outputPath);

                        if (existingVersion == jagaFileInfo.Version)
                        {
                            var logMethod = nameFilters.Length > 0 ? (Action<string>)Logger.Info : Logger.Debug;

                            logMethod($"Skipped {outputFilename} because it already exists and version is unchanged.");
                            return;
                        }
                    }

                    var jagaFile = new JagaFile(Cache.GetFile(Index.Music, trackNamePair.Key).Data);

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

                    // Create argument to supply to FFmpeg (https://trac.ffmpeg.org/wiki/Concatenate#filter)
                    var ffmpegArgument = randomTemporaryFilenames.Aggregate("", (result, value) => $"{result}-i \"{value}\" ") +
                        Enumerable.Range(0, randomTemporaryFilenames.Length).Aggregate("-filter_complex \"", (result, streamIndex) => $"{result}[{streamIndex}:a:0] ") +
                        $"concat=n={randomTemporaryFilenames.Length}:v=0:a=1 [a]\" -map \"[a]\" " +
                        $"-metadata title=\"{trackNamePair.Value}\" " +
                        $"-metadata version=\"{jagaFileInfo.Version}\" " +
                        "-metadata album=\"RuneScape Original Soundtrack\" " + 
                        "-metadata genre=\"Game\" " +
                        "-metadata comment=\"Extracted by Viller's RuneScape Cache Tools\" " +
                        "-metadata copyright=\"Jagex Games Studio\" " +
                        outputPath;

                    // Combine the files using FFmpeg
                    var combineProcess = new Process
                    {
                        StartInfo =
                        {
                            FileName = "ffmpeg",
                            UseShellExecute = false,
                            CreateNoWindow = true,
#if DEBUG
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
#endif
                            Arguments = ffmpegArgument
                        }
                    };

                    combineProcess.Start();

#if DEBUG
                    combineProcess.OutputDataReceived += (sender, args) => Console.WriteLine($"oggCat output: {args.Data}");
                    combineProcess.ErrorDataReceived += (sender, args) => Console.WriteLine($"oggCat error: {args.Data}");
                    combineProcess.BeginOutputReadLine();
                    combineProcess.BeginErrorReadLine();
#endif

                    combineProcess.WaitForExit();

                    // Remove temporary files
                    foreach (var randomTemporaryFilename in randomTemporaryFilenames)
                    {
                        File.Delete(randomTemporaryFilename);
                    }

                    if (combineProcess.ExitCode != 0)
                    {
                        throw new SoundtrackException($"oggCat returned with error code {combineProcess.ExitCode} for {outputFilename}.");
                    }

                    Logger.Info($"Combined {outputFilename}.");
                }
                catch (CacheException)
                {
                    Logger.Info($"Skipped {outputFilename} because of corrupted or incomplete data.");
                }
            });

            Logger.Info("Done combining soundtracks.");
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
                            Enumerable.Repeat(validChars, nameLength).Select(s => s[_random.Next(s.Length)]).ToArray());
                    newPath = /* TODO: Cache.TemporaryDirectory */ "derr/" + newPath + ".ogg";
                }
                while (File.Exists(newPath) || result.Contains(newPath));

                result[i] = newPath;
            }

            return result;
        }
    }
}