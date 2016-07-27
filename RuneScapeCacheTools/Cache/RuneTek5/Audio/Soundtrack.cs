using System.Collections.Generic;
using System.IO;
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
	}
}