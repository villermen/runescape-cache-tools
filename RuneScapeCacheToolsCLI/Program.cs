using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

	    private static int Main(string[] arguments)
	    {
#if DEBUG
		    Console.WriteLine("RSCT DEBUG BUILD");
		    Console.WriteLine();

		    // Set log4net log level to debug
		    ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).Root.Level = Level.Debug;
		    ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);
#endif
		    
		    var program = new Program();

		    var returnCode = program.Configure(arguments);
		    
		    if (returnCode != 0)
		    {
			    returnCode = program.Execute();
		    }
			
#if DEBUG
		    Console.ReadLine();
#endif
		    
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
			}
			
			// TODO: Split up into multiple executables. rsct-soundtrack, rsct-extract

			return returnCode;
		}

		public int Execute()
		{
			if (this._arguments.ShowHelp)
			{
				Console.WriteLine($"Usage: {typeof(Program).Assembly.GetName().Name} [OPTION]...");
				Console.WriteLine("Tools for performing actions on RuneScape's cache.");
				Console.WriteLine();
				this._arguments.WriteOptionDescriptions(Console.Out);
			}
			else
			{

				try
				{
					var cache = this._arguments.Download ? (CacheBase)new DownloaderCache() : new RuneTek5Cache(this._arguments.CacheDirectory, true);



					// Perform the action if everything is ok
					if (run && !Program._doHelp)
					{
						// Initialize the cache
						Program._cache = Program._download ? (CacheBase)new DownloaderCache() : new RuneTek5Cache(Program._cacheDirectory);

						// Perform the specified actions
						if (Program._doExtract)
						{
							Program.Extract();
						}

						if (Program._doSoundtrackCombine)
						{
							Program.CombineSoundtrack();
						}

						returnCode = 0;
					}
					else
					{
						// Show help if something went wrong during argument parsing
						Program.ShowHelp();

						returnCode = 1;
					}
				}
				catch (Exception exception) when (exception is OptionException || exception is CLIException)
				{
					Console.WriteLine(exception);

					returnCode = 1;
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception);

					returnCode = 1;
				}
			}



			return returnCode;
		}

        /// <summary>
        ///   Expands the given integer range into an enumerable of all individual integers.
        /// </summary>
        /// <param name="integerRangeString">An integer range, e.g. "0-4,6,34,200-201"</param>
        /// <returns></returns>
        private static IEnumerable<int> ExpandIntegerRangeString(string integerRangeString)
		{
		    var rangeStringParts = Program.ExpandListString(integerRangeString);
			var result = new List<int>();

			foreach (var rangeStringPart in rangeStringParts)
			{
				if (rangeStringPart.Count(ch => ch == '-') == 1)
				{
					// Expand the range
					var rangeParts = rangeStringPart.Split('-');
					var rangeStart = int.Parse(rangeParts[0]);
					var rangeCount = int.Parse(rangeParts[1]) - rangeStart + 1;

					result.AddRange(Enumerable.Range(rangeStart, rangeCount));
				}
				else
				{
					// It should be a single integer
					result.Add(int.Parse(rangeStringPart));
				}
			}

			// Filter duplicates
			return result.Distinct();
		}

	    private static IEnumerable<string> ExpandListString(string listString)
	    {
            return listString.Split(',', ';');
        }

		private static void Extract(CacheBase inputCache, FlatFileCache outputCache, IEnumerable<Index> indexes, IEnumerable<int> fileIds)
		{
            // Display progress at bottom of console without creating a new row
            var progress = new ExtendedProgress();
            progress.ProgressChanged += (p, message) =>
            {
                Console.Write($"Extraction progress: {Math.Round(progress.Percentage)}% ({progress.Current}/{progress.Total})\r");
            };

            if (indexes == null && fileIds == null)
			{
				// Extract everything
				Program._cache.Extract(Program._overwrite, progress);
			}
			else if (fileIds == null)
			{
				// Extract the given index(es) fully
				Program._cache.Extract(Program.GetIndexes, Program._overwrite, progress);
			}
			else if (Program._indexes.Count() == 1)
			{
				// Extract specified files from the given index
				Program._cache.Extract(Program.GetIndexes.First(), Program._fileIds, Program._overwrite, progress);
			}
			else
			{
				throw new CLIException("You can only specify multiple files if you specify exactly one index to extract from.");
			}

            Console.WriteLine();
		}

		private static void CombineSoundtrack(CacheBase cache, string soundtrackDirectory, string temporaryDirectory,
			bool overwrite, bool lossless, bool includeUnnamedSoundtracks, IEnumerable<string> filter)
		{
			var soundtrack = new Soundtrack(cache, soundtrackDirectory);

			if (temporaryDirectory != null)
			{
				soundtrack.TemporaryDirectory = temporaryDirectory;
			}

			soundtrack.Extract(overwrite, lossless, includeUnnamedSoundtracks, filter?.ToArray() ?? new string[0]);
		}
	}
}