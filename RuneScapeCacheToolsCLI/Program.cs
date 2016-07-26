using System;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Audio;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Enums;

namespace Villermen.RuneScapeCacheTools.CLI
{
	internal class Program
	{
		private const string OutputDirectory = "C:/Data/Temp/rscd/data";

		private static void Main(string[] args)
		{
			// Directory.CreateDirectory(OutputDirectory);

			var cache = new RuneTek5Cache();

			var indexIds = cache.GetIndexIds();
			var fileIds = cache.GetFileIds(17);
			var fileData = cache.GetFileData(17, 5);
			var archiveFileCount = cache.GetArchiveFileCount(17, 5);
			var archiveFileData = cache.GetArchiveFileData(17, 5, 65);
			var archiveFilesData = cache.GetArchiveFiles(17, 5);

			var trackNamesEnum = new EnumFile(archiveFileData);

			var soundtrackFileIds = cache.GetFileIds(40);

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