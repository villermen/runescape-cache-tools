using System;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Audio;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Enums;

namespace Villermen.RuneScapeCacheTools.CLI
{
	internal class Program
	{
		private const string CacheDirectory = "C:/Data/Temp/rscd/data/";

		private static void Main(string[] args)
		{
			CacheBase cache = new RuneTek5Cache(CacheDirectory);

			// var trackFileIds = new EnumFile();
			
			var trackNames = new EnumFile(cache.GetArchiveFileData(17, 5, 65));
			//var sortingTrackNames = new EnumFile(cache.GetArchiveFileData(17, 5, 67));
			var jagaFileIds = new EnumFile(cache.GetArchiveFileData(17, 5, 71));

			var d = jagaFileIds[5];

			foreach (var soundtrackFileId in jagaFileIds) // Add indexing and enumerating to EnumFile
			{
				try
				{
					var soundtrackFileData = cache.GetFileData(40, soundtrackFileId);
					var jagaFile = new JagaFile(soundtrackFileData);


				}
				catch (Exception ex) when (ex is JagaParseException)
				{
				}

			}

			// Console.ReadLine();
		}
	}
}