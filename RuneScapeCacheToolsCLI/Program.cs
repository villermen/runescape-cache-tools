using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using log4net.Core;
using NDesk.Options;
using Villermen.RuneScapeCacheTools.Audio;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.Downloader;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.CLI
{
    internal static class Program
	{
		/// <summary>
		/// Even when not used, this needs to be here to initialize the logging system.
		/// </summary>
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

		private static CacheBase Cache { get; set; }

        private static string CacheDirectory { get; set; }

        private static string OutputDirectory { get; set; }

        private static string TemporaryDirectory { get; set; }

        private static int TriggeredActions { get; set; }

		private static IEnumerable<Index> Indexes { get; set; }

		private static IEnumerable<int> FileIds { get; set; }

		private static bool DoExtract { get; set; }

		private static bool Overwrite { get; set; }

		private static bool Lossless { get; set; }

        private static bool DoSoundtrackCombine { get; set; }

        private static IEnumerable<string> SoundtrackNameFilter { get; set; }

        private static bool Download { get; set; }

		private static readonly OptionSet ArgumentParser = new OptionSet
		{
			{
				"cache-directory=|c",
				"The directory in which RuneScape's cache files are located. If unspecified, the default directory will be attempted.",
				value =>
				{
					Program.CacheDirectory = value;
				}
			},
		    {
		        "download|d",
                "Download all requested files straight from Jagex's servers instead of using a local cache.",
		        value =>
		        {
		            Program.Download = value != null;
		        }
		    },
			{
				"output-directory=|o",
				"The directory in which output files (mainly extracted files) will be stored.",
				value =>
				{
					Program.OutputDirectory = value;
				}
			},
			{
				"temporary-directory=|t",
				"The directory in which temporary files will be stored. If unspecified, the system's default directory + \"rsct\" will be used.",
				value =>
				{
					Program.TemporaryDirectory = value;
				}
			},
			{
				"extract|e",
				"Extract all files from the cache. You can specify which files to extract by using the index and file arguments.",
				value =>
				{
					Program.DoExtract = (value != null);
					Program.TriggeredActions++;
				}
			},
			{
				"index=|i",
                "A index id or range of index ids to extract. E.g. \"1-2,4,6-7\".",
				value =>
				{
					Program.Indexes = Program.ExpandIntegerRangeString(value).Cast<Index>();
				}
			},
			{
				"file=|f",
                "A file id or range of file ids to extract. E.g. \"1-2,4,6-7\".",
				value =>
				{
					Program.FileIds = Program.ExpandIntegerRangeString(value);
				}
			},
			{
				"overwrite",
                "Overwrite extracted files if they already exist.",
				value =>
				{
					Program.Overwrite = (value != null);
				}
			},
			{
				"soundtrack-combine:|s",
                "Extract and name the soundtrack, optionally filtered by the given comma-separated name filters.",
				value =>
				{
				    Program.DoSoundtrackCombine = true;

                    if (!string.IsNullOrWhiteSpace(value))
				    {
				        Program.SoundtrackNameFilter = Program.ExpandListString(value);
				    }

				    Program.TriggeredActions++;
				}
			},
		    {
		        "lossless",
                "Use FLAC instead of OGG as audio format when combining, preventing quality loss.",
		        value =>
		        {
		            Program.Lossless = (value != null);
		        }
		    },
			{
				"help|h|?",
                "Show this message.",
                value =>
				{
					Program.ShowHelp();

					Program.TriggeredActions++;
				}
			}
		};

		private static int Main(string[] args)
		{
#if DEBUG
		    Console.WriteLine("RSCT DEBUG BUILD");
            Console.WriteLine();

            // Set log4net log level to debug
            ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).Root.Level = Level.Debug;
            ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);
#endif
            int returnCode;

			try
			{
				var unknownArguments = Program.ArgumentParser.Parse(args);
			    var run = true;

                // Show supplied argument that could not be parsed
				if (unknownArguments.Count > 0)
				{
                    run = false;

                    foreach (var unknownArgument in unknownArguments)
                    {
                        Console.WriteLine($"Unknown argument \"{unknownArgument}\".");
                        Console.WriteLine();
                    }
				}

				// Show help when no action arguments are specified. This is considered an error as it is unexpected.
			    if (Program.TriggeredActions == 0)
			    {
                    run = false;

                    if (args.Length > 0)
			        {
                        Console.WriteLine("No action specified.");
			            Console.WriteLine();
			        }
			    }

                // Perform the action if everything is ok
                if (run)
			    {
			        // Initialize the cache
			        Program.Cache = Program.Download ? (CacheBase)new DownloaderCache() : new RuneTek5Cache(Program.CacheDirectory);

			        if (Program.OutputDirectory != null)
			        {
			            Program.Cache.OutputDirectory = Program.OutputDirectory;
			        }

			        if (Program.TemporaryDirectory != null)
			        {
			            Program.Cache.TemporaryDirectory = Program.TemporaryDirectory;
			        }

			        // Perform the specified actions
			        if (Program.DoExtract)
			        {
			            Program.Extract();
			        }

			        if (Program.DoSoundtrackCombine)
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

#if DEBUG
            Console.ReadLine();
#endif

            // Following code replaces hundred lines of code I would have had to write to accomplish the same with log4net...
            // Delete log file if it is still empty upon exit
            LogManager.GetRepository().ResetConfiguration();
		    var logFile = new FileInfo("rsct.log");
            if (logFile.Exists && logFile.Length == 0)
		    {
                logFile.Delete();
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

		private static void ShowHelp()
		{
			Console.WriteLine($"Usage: {typeof(Program).Assembly.GetName().Name} [OPTION]...");
			Console.WriteLine("Tools for performing actions on RuneScape's cache.");
			Console.WriteLine();
			Program.ArgumentParser.WriteOptionDescriptions(Console.Out);
		}

		private static void Extract()
		{
            // Display progress at bottom of console without creating a new row
            var progress = new ExtendedProgress();
            progress.ProgressChanged += (p, message) =>
            {
                Console.Write($"Extraction progress: {Math.Round(progress.Percentage)}% ({progress.Current}/{progress.Total})\r");
            };

            if (Program.Indexes == null && Program.FileIds == null)
			{
				// Extract everything
				Program.Cache.Extract(Program.Overwrite, progress);
			}
			else if (Program.FileIds == null)
			{
				// Extract the given index(es) fully
				Program.Cache.Extract(Program.Indexes, Program.Overwrite, progress);
			}
			else if (Program.Indexes.Count() == 1)
			{
				// Extract specified files from the given index
                Program.Cache.Extract(Program.Indexes.First(), Program.FileIds, Program.Overwrite, progress);
			}
			else
			{
				throw new CLIException("You can only specify multiple files if you specify exactly one index to extract from.");
			}

            Console.WriteLine();
		}

		private static void CombineSoundtrack()
		{
			var soundtrack = new Soundtrack(Program.Cache);

			soundtrack.Extract(Program.Overwrite, Program.Lossless, Program.SoundtrackNameFilter?.ToArray() ?? new string[0]);
        }
	}
}