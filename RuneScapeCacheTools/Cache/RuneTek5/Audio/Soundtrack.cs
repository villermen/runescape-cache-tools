using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Enums;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5.Audio
{
	/// <summary>
	///   Contains tools for obtaining and combining soundtracks from the cache.
	/// </summary>
	public class Soundtrack
	{
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
			var trackNames = new EnumFile(Cache.GetArchiveFileData(17, 5, 65));
			var jagaFileIds = new EnumFile(Cache.GetArchiveFileData(17, 5, 71));

			var result = new Dictionary<int, string>();
			foreach (var trackNamePair in trackNames)
			{
				var trackName = (string) trackNamePair.Value;

				if (!jagaFileIds.ContainsKey(trackNamePair.Key))
				{
					continue;
				}

				var trackFileId = (int) jagaFileIds[trackNamePair.Key];

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

			return result;
		}

		public async Task ExportTracks(bool overwriteExisting = false)
		{
			var trackNames = GetTrackNames();
			var outputDirectory = Cache.OutputDirectory + "soundtrack/";

			Directory.CreateDirectory(outputDirectory);
			Directory.CreateDirectory(Cache.TemporaryDirectory);

			// Remove existing tracks from the dictionary if we should not export them anyway
			if (!overwriteExisting)
			{
				var existingTrackNames = Directory.EnumerateFiles(outputDirectory, "*.ogg").Select(Path.GetFileNameWithoutExtension);

				trackNames = trackNames.Where(pair => !existingTrackNames.Contains(pair.Value)).ToDictionary(pair => pair.Key, pair => pair.Value);
			}

			// TODO: Parallel.ForEach
			foreach (var trackNamePair in trackNames)
			{
				var jagaFile = new JagaFile(Cache.GetFileData(40, trackNamePair.Key));

				// TODO: Implement

				//File.WriteAllBytes(Cache.TemporaryDirectory + "0.ogg", jagaFile.ContainedChunkData);

				//for (var chunkIndex = 1; chunkIndex < jagaFile.ChunkCount; chunkIndex++)
				//{
				//	File.WriteAllBytes($"{Cache.TemporaryDirectory}{chunkIndex}.ogg", cache.GetFileData(40, jagaFile.ChunkDescriptors[chunkIndex].FileId));
				//}

				//var name = (string)trackNames[soundtrackFileIdPair.Key];

				//var combineProcess = new Process
				//{
				//	StartInfo =
				//		{
				//			FileName = "lib/oggCat",
				//			UseShellExecute = false,
				//			CreateNoWindow = true
				//		}
				//};

				//combineProcess.StartInfo.Arguments = "output.ogg" + string.Join("", Enumerable.Range(0, jagaFile.ChunkCount - 1).Select(oggId => $" {oggId}.ogg"));

				//combineProcess.Start();
				//combineProcess.WaitForExit();
			}
		}
	}
}