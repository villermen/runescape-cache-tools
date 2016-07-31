using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using log4net.Appender;
using log4net.Config;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Audio;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Enums;

namespace Villermen.RuneScapeCacheTools.CLI
{
	internal class Program
	{
		private const string CacheDirectory = "C:/Data/Temp/rscd/data/";
		private const string OutputDirectory = "C:/Data/Temp/rscache/";

		private static void Main(string[] args)
		{
		    BasicConfigurator.Configure();

			CacheBase cache = new RuneTek5Cache(CacheDirectory)
			{
				OutputDirectory = OutputDirectory
			};

			var soundtrack = new Soundtrack(cache);

		    soundtrack.ExportTracksAsync().Wait();

		    Console.ReadLine();
		}
	}
}
