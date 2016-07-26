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
			// Directory.CreateDirectory(OutputDirectory);

			var cache = new RuneTek5Cache(CacheDirectory);

			//var indexCount = cache.IndexCount;
			//var fileCount = cache.GetFileCount(17);
			//var fileData = cache.GetFileData(17, 5);
			//var archiveFileCount = cache.GetArchiveFileCount(17, 5);
			//var archiveFileData = cache.GetArchiveFileData(17, 5, 65);
			//var archiveFilesData = cache.GetArchiveFiles(17, 5);

			//var trackNamesEnum = new EnumFile(archiveFileData);

			var soundtrackFileIds = Enumerable.Range(0, cache.GetFileCount(40));

			foreach (var soundtrackFileId in soundtrackFileIds)
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