using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using log4net;
using log4net.Core;
using NDesk.Options;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.Downloader;
using Villermen.RuneScapeCacheTools.Cache.FlatFile;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.CLI
{
    public class Program
	{
		/// <summary>
		/// Even when not used, this needs to be here to initialize the logging system.
		/// </summary>
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

		private Arguments _arguments;
		private CacheBase _cache;

	    private static int Main(string[] arguments)
	    {
#if DEBUG
		    Console.WriteLine();
		    Console.WriteLine("RSCT DEVELOPMENT BUILD");
		    Console.WriteLine();

		    // Set log4net log level to debug
		    ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).Root.Level = Level.Debug;
		    ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);
#endif
		    
		    var program = new Program();

		    var returnCode = program.Configure(arguments);
		    
		    if (returnCode == 0)
		    {
			    returnCode = program.Execute();
		    }
		    
		    // Following code replaces hundred lines of code I would have had to write to accomplish the same with log4net...
		    // Delete log file if it is still empty when done
		    LogManager.GetRepository().ResetConfiguration();
		    var logFile = new FileInfo("rsct.log");
		    if (logFile.Exists && logFile.Length == 0)
		    {
			    logFile.Delete();
		    }

		    return returnCode;
        }

		private int Configure(string[] stringArguments)
		{
			this._arguments = new Arguments();
			this._arguments.Parse(stringArguments);

			var returnCode = 0;

			// Show supplied arguments that could not be parsed
			if (this._arguments.UnparsedArguments.Count > 0)
			{
				foreach (var unparsedArgument in this._arguments.UnparsedArguments)
				{
					Console.WriteLine($"Unknown argument \"{unparsedArgument}\".");
					Console.WriteLine();
				}

				returnCode = 1;
			}

			// Show help when no action arguments are specified. This is considered an error as it is unexpected.
			if (this._arguments.Actions == 0)
			{
				Console.WriteLine("No actions specified.");
				Console.WriteLine("Use either the extract or soundtrack options to actually do something.");
				Console.WriteLine();

				returnCode = 1;
			}

			if (this._arguments.Actions > 1)
			{
				Console.WriteLine("Too many actions specified.");
				Console.WriteLine("Use either the extract or soundtrack option, but not both at the same time.");
				Console.WriteLine();

				returnCode = 1;
			}
			
			// TODO: Split up into multiple executables. rsct-soundtrack, rsct-extract

			return returnCode;
		}

		public int Execute()
		{
			var returnCode = 0;
			
			if (!this._arguments.ShowHelp)
			{
				this._cache = this._arguments.Download ? (CacheBase)new DownloaderCache() : new RuneTek5Cache(this._arguments.CacheDirectory, true);

				// Perform the specified actions
				if (this._arguments.DoExtract)
				{
					this.Extract();
				}

				if (this._arguments.DoSoundtrackCombine)
				{
					this.CombineSoundtrack();
				}
			}
			else
			{
				Console.WriteLine($"Usage: {typeof(Program).Assembly.GetName().Name} [OPTION]...");
				Console.WriteLine("Tools for performing actions on RuneScape's cache.");
				Console.WriteLine();
				this._arguments.WriteOptionDescriptions(Console.Out);

				returnCode = 1;
			}
			
			return returnCode;
		}

		private void Extract()
		{
			var outputCache = new FlatFileCache(this._arguments.OutputDirectory + "files/");
			
//            // Display progress at bottom of console without creating a new row
//            var progress = new ExtendedProgress();
//            progress.ProgressChanged += (p, message) =>
//            {
//                Console.Write($"Extraction progress: {Math.Round(progress.Percentage)}% ({progress.Current}/{progress.Total})\r");
//            };

			if (this._arguments.Indexes == null && this._arguments.FileIds != null)
			{
				throw new ArgumentException("If you specify files to extract you must also explicitly specify indexes to extract.");
			}

            foreach (var index in this._arguments.Indexes ?? this._cache.GetIndexes())
            {
	            foreach (var fileId in this._arguments.FileIds ?? this._cache.GetFileIds(index))
	            {
		            this._cache.CopyFile(index, fileId, outputCache);
	            }
			}
		}

		private void CombineSoundtrack()
		{
			var soundtrack = new Soundtrack(this._cache, this._arguments.OutputDirectory + "soundtrack/");

			if (this._arguments.TemporaryDirectory != null)
			{
				soundtrack.TemporaryDirectory = this._arguments.TemporaryDirectory;
			}

			soundtrack.Extract(this._arguments.Overwrite, this._arguments.Lossless,
				this._arguments.IncludeUnnamedSoundtracks, this._arguments.SoundtrackNameFilter?.ToArray() ?? new string[0]);
		}
	}
}