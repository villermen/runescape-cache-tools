using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Enums;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5.Audio
{
	public class Soundtrack
	{
		public CacheBase Cache { get; set; }

		public Soundtrack(CacheBase cache)
		{
			Cache = cache;
		}

		/// <summary>
		/// Returns the track names and their corresponding jaga file id in index 40.
		/// Track names are made filename-safe, and empty ones are filtered out.
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