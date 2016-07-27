using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
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

			foreach (var soundtrackFileIdPair in jagaFileIds)
			{
				try
				{
					var soundtrackFileData = cache.GetFileData(40, (int) soundtrackFileIdPair.Value);
					var jagaFile = new JagaFile(soundtrackFileData);

					File.WriteAllBytes("0.ogg", jagaFile.ContainedChunkData);

					for (var chunkIndex = 1; chunkIndex < jagaFile.ChunkCount; chunkIndex++)
					{
						File.WriteAllBytes($"{chunkIndex}.ogg", cache.GetFileData(40, jagaFile.ChunkDescriptors[chunkIndex].FileId));
					}
				}
				catch (Exception ex) when (ex is JagaParseException)
				{
				}
			}

			// Console.ReadLine();
		}
	}
}