using System;
using System.Diagnostics;
using System.IO;

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

			var archiveIds = cache.getArchiveIds();
			Debug.WriteLine(archiveIds);

			var fileIds = cache.getFileIds(2);
			Debug.WriteLine(fileIds);

			cache.ExtractFile(40, 2628);

			Debug.WriteLine(cache.GetFileOutputPath(40, 2628));
		}
	}
}
