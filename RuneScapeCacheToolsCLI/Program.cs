using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.FileProcessors;
using Villermen.RuneScapeCacheTools.FileProcessors.Enums;

namespace Villermen.RuneScapeCacheTools.CLI
{
    internal class Program
	{
		private const string OutputDirectory = "C:/Data/Temp/rsnxtcache/";

	    private static void Main(string[] args)
		{
			// Directory.CreateDirectory(OutputDirectory);

		    var cache = new RuneTek5Cache();

		    var indexIds = cache.GetIndexIds();
		    var fileIds = cache.GetFileIds(17);

            Console.ReadLine();
		}
	}
}
