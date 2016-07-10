using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Villermen.RuneScapeCacheTools.FileProcessors;
using Villermen.RuneScapeCacheTools.FileProcessors.Enums;

namespace Villermen.RuneScapeCacheTools.CLI
{
	class Program
	{
		private const string OutputDirectory = "C:/Data/Temp/rsnxtcache/";

		static void Main(string[] args)
		{
			Directory.CreateDirectory(OutputDirectory);

			NXTCache cache = new NXTCache();
			cache.OutputDirectory = OutputDirectory;

			//var archiveIds = cache.getArchiveIds();
			//Debug.WriteLine(archiveIds);

			//var fileIds = cache.getFileIds(2);
			//Debug.WriteLine(fileIds);

			//cache.ExtractFile(40, 2628);

			//Debug.WriteLine(cache.GetFileOutputPath(40, 2628));

			//cache.ExtractAllAsync().Wait();

			var enumFile = new EnumFileProcessor(cache.GetFileOutputPath(17, 5, true));
			var metadata = enumFile.GetMetadata().Where((pair) => pair.Value.Type == EnumType.IntInt);

			var enums = enumFile.GetEnums();

			Console.ReadLine();

			// Find indexes for the ones I used for the soundtrack
		}
	}
}
