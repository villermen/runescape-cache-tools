using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace RuneScapeCacheTools
{
	public static class Soundtrack
	{
		/// <summary>
		/// Tries to parse archive 17 file 5 to obtain a list of music and their corresponding index file id in archive 40.
		/// Returns a dictionary that can resolve index file id to the name of the track as it appears in-game.
		/// </summary>
		public static Dictionary<int, string> GetTrackNames()
		{
			//the following is based on even more assumptions than normal made while comparing 2 extracted caches, it's therefore probably the first thing to break
			//4B magic number (0x00016902) - 2B a file id? - 2B amount of files (higher than actual entries sometimes) - 2B amount of files

			string resolveFileName = Cache.GetFile(17, 5, true);

			using (FileStream resolveFile = File.OpenRead(resolveFileName))
			{
				Dictionary<int, string> trackIdNames = new Dictionary<int, string>();
				Dictionary<uint, int> fileIdTrackIds = new Dictionary<uint, int>();

				//to locate the start of names and file ids tables
				byte[] namesMagicNumber = { 0x00, 0x66, 0x24, 0x07 };
				byte[] filesMagicNumber = { 0x00, 0x66, 0x0b, 0x08 };

				long namesStartPos = resolveFile.IndexOf(namesMagicNumber);
				long filesStartPos = resolveFile.IndexOf(filesMagicNumber);

				if (namesStartPos == -1 || filesStartPos == -1)
					return trackIdNames;

				resolveFile.Position = namesStartPos + 6;
				uint musicCount = resolveFile.ReadBytes(2);

				//construct trackIdNames
				Regex regex = new Regex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]");
				for (int i = 0; i < musicCount; i++)
				{
					int trackId = (int)resolveFile.ReadBytes(2);
					string trackName = resolveFile.ReadNullTerminatedString();

					//replace characters that can't be used in files from trackName
					trackName = regex.Replace(trackName, "_");

					//add only if the string is of any use
					if (!string.IsNullOrWhiteSpace(trackName))
						trackIdNames.Add(trackId, trackName);
				}

				//construct fileIdTracks
				resolveFile.Position = filesStartPos + 6;
				uint fileCount = resolveFile.ReadBytes(2);
				for (int i = 0; i < fileCount; i++)
				{
					int trackId = (int)resolveFile.ReadBytes(2);
					uint fileId = resolveFile.ReadBytes(4);

					//only add if it doesn't exist already
					if (!fileIdTrackIds.ContainsKey(fileId))
						fileIdTrackIds.Add(fileId, trackId);
				}

				return trackIdNames;
			}
		}
	}
}
