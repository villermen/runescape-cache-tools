using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Enums;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5.Audio
{
	/// <summary>
	///   Contains tools for obtaining and combining soundtracks from the cache.
	/// </summary>
	public class Soundtrack
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Soundtrack));

		/// <summary>
		///   Used in the generation of temporary filenames.
		/// </summary>
		private readonly Random _random = new Random();

		public Soundtrack(CacheBase cache)
		{
			Cache = cache;
		}

		public CacheBase Cache { get; set; }

		/// <summary>
		///   Returns the track names and their corresponding jaga file id in index 40.
		///   Track names are made filename-safe, and empty ones are filtered out.
		/// </summary>
		/// <returns></returns>
		public IDictionary<int, string> GetTrackNames()
		{
            // Read out the two enums that, when combined, make up the awesome lookup table
            var trackNames = new EnumFile(Cache.GetFile(17, 5).Entries[65]);
            var jagaFileIds = new EnumFile(Cache.GetFile(17, 5).Entries[71]);

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
                .Select(group => group.Skip(1)); // Select only the second and up, because the first one doesn't have to be renamed

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

		public async Task ExportTracksAsync(bool overwriteExisting = false)
		{
            var trackNames = GetTrackNames();
            var outputDirectory = Cache.OutputDirectory + "soundtrack/";

            Directory.CreateDirectory(outputDirectory);
            Directory.CreateDirectory(Cache.TemporaryDirectory);

            // Remove existing tracks from the dictionary if we should not export them anyway
            if (!overwriteExisting)
            {
                var existingTrackNames = Directory.EnumerateFiles(outputDirectory, "*.ogg").Select(Path.GetFileNameWithoutExtension);

                trackNames = trackNames.Where(pair => !existingTrackNames.Contains(pair.Value))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
            }

            Logger.Info("Done obtaining soundtrack names and file ids.");

            await Task.Run(() => Parallel.ForEach(trackNames, trackNamePair =>
            {
                var outputFilename = $"{trackNamePair.Value}.ogg";

                try
                {
                    var jagaFile = new JagaFile(Cache.GetFile(40, trackNamePair.Key).Entries[0]);

                    // Obtain names for the temporary files. We can't use the id as filename, because we are going full parallel.
                    var randomTemporaryFilenames = GetRandomTemporaryFilenames(jagaFile.ChunkCount);

                    // Write out the files
                    File.WriteAllBytes(randomTemporaryFilenames[0], jagaFile.ContainedChunkData);

                    for (var chunkIndex = 1; chunkIndex < jagaFile.ChunkCount; chunkIndex++)
                    {
                        File.WriteAllBytes(randomTemporaryFilenames[chunkIndex],
                            Cache.GetFile(40, jagaFile.ChunkDescriptors[chunkIndex].FileId).Entries[0]);
                    }

                    // Combine the files using oggCat
                    var combineProcess = new Process
                    {
                        StartInfo =
                        {
                            FileName = "oggCat",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            Arguments = "-x -c \"EXTRACTED_BY=Viller's RuneScape Cache Tools;FILE_VERSION=-1\" " + // TODO: File version
                                $"\"{outputDirectory}{outputFilename}\" " +
                                "\"" + string.Join("\" \"", randomTemporaryFilenames) + "\"",
                        }
                    };

                    var arguments = combineProcess.StartInfo.Arguments;

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
                        Logger.Error(soundtrackException.Message, soundtrackException);
                        throw soundtrackException;
                    }

                    Logger.Info($"Combined {outputFilename}.");
                }
                catch (Exception exception) when (exception is SectorException || exception is CacheException)
                {
                    Logger.Info($"Skipped {outputFilename} because of corrupted or incomplete data.");
                }
            }));

            Logger.Info($"Done combining soundtracks.");
        }

		private string[] GetRandomTemporaryFilenames(int amountOfNames)
		{
			const string validChars = "abcdefghijklmnopqrstuvwxyz0123456789-_()&^%$#@![]{},`~=+";
			const int nameLength = 16;
			var result = new string[amountOfNames];

			for (var i = 0; i < amountOfNames; i++)
			{
				string newPath;
				do
				{
					newPath = new string(Enumerable.Repeat(validChars, nameLength).Select(s => s[_random.Next(s.Length)]).ToArray());
					newPath = Cache.TemporaryDirectory + newPath + ".ogg";
				}
				while (File.Exists(newPath) || result.Contains(newPath));

				result[i] = newPath;
			}

			return result;
		}
	}
}