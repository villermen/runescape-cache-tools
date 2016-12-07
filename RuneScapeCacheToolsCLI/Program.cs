using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using NDesk.Options;
using Villermen.RuneScapeCacheTools.Audio;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.Downloader;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;

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
					CacheDirectory = value;
				}
			},

		    {
		        "download|d",
                "Download all requested files straight from Jagex's servers instead of using a local cache.",
		        value =>
		        {
		            Download = value != null;
		        }
		    },

			{
				"output-directory=|o",
				"The directory in which output files (mainly extracted files) will be stored.",
				value =>
				{
					OutputDirectory = value;
				}
			},

			{
				"temporary-directory=|t",
				"The directory in which temporary files will be stored. If unspecified, the system's default directory + \"rsct\" will be used.",
				value =>
				{
					TemporaryDirectory = value;
				}
			},

			{
				"extract|e",
				"Extract all files from the cache. You can specify which files to extract by using the index and file arguments.",
				value =>
				{
					DoExtract = (value != null);
					TriggeredActions++;
				}
			},

			{
				"index=|i", "A index id or range of index ids to extract. E.g. \"1-2,4,6-7\".",
				value =>
				{
					Indexes = ExpandIntegerRangeString(value).Cast<Index>();
				}
			},

			{
				"file=|f", "A file id or range of file ids to extract. E.g. \"1-2,4,6-7\".",
				value =>
				{
					FileIds = ExpandIntegerRangeString(value);
				}
			},

			{
				"overwrite", "Overwrite extracted files if they already exist.",
				value =>
				{
					Overwrite = (value != null);
				}
			},

			{
				"soundtrack-combine:|s", "Extract and name the soundtrack, optionally filtered by the given comma-separated name filters.",
				value =>
				{
				    DoSoundtrackCombine = true;

                    if (!string.IsNullOrWhiteSpace(value))
				    {
				        SoundtrackNameFilter = ExpandListString(value);
				    }

				    TriggeredActions++;
				}
			},

			{
				"help|h|?", "Show this message.", value =>
				{
					ShowHelp();

					TriggeredActions++;
				}
			}
		};

		private static int Main(string[] args)
		{
		    var returnCode = 0;

			try
			{
				var unknownArguments = ArgumentParser.Parse(args);

				// Show supplied argument that could not be parsed
				foreach (var unknownArgument in unknownArguments)
				{
					Console.WriteLine($"Unknown argument \"{unknownArgument}\".");
				}

				if (unknownArguments.Count > 0)
				{
					Console.WriteLine();
				}

				// Show help if "Nothing interesting happens." when arguments are specified, this is considered an error as it is unexpected
				if (TriggeredActions == 0 && args.Length > 0)
				{
					Console.WriteLine("No action arguments specified.");
					Console.WriteLine();
					ShowHelp();

				    returnCode = 1;
				}

                // Initialize the cache
                Cache = Download ? (CacheBase)new CacheDownloader() : new RuneTek5Cache(CacheDirectory);

			    if (OutputDirectory != null)
			    {
			        Cache.OutputDirectory = OutputDirectory;
			    }

			    if (TemporaryDirectory != null)
			    {
			        Cache.TemporaryDirectory = TemporaryDirectory;
                }

				// Perform the specified actions
				if (DoExtract)
				{
					Extract();
				}

				if (DoSoundtrackCombine)
				{
					CombineSoundtrack();
				}
			}
			catch (Exception exception) when (exception is OptionException || exception is CacheException || exception is CLIException)
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

            return returnCode;
        }

        /// <summary>
        ///   Expands the given integer range into an enumerable of all individual integers.
        /// </summary>
        /// <param name="integerRangeString">An integer range, e.g. "0-4,6,34,200-201"</param>
        /// <returns></returns>
        private static IEnumerable<int> ExpandIntegerRangeString(string integerRangeString)
		{
		    var rangeStringParts = ExpandListString(integerRangeString);
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
			ArgumentParser.WriteOptionDescriptions(Console.Out);
		}

		private static void Extract()
		{
			if (Indexes == null && FileIds == null)
			{
				// Extract everything
				Cache.Extract(Overwrite);
			}
			else if (FileIds == null)
			{
				// Extract the given index(es) fully
				Cache.Extract(Indexes, Overwrite);
			}
			else if (Indexes.Count() == 1)
			{
				// Extract specified files from the given index
				Cache.Extract(Indexes.First(), FileIds, Overwrite);
			}
			else
			{
				throw new CLIException("You can only specify multiple files if you specify exactly one index to extract from.");
			}
		}

		private static void CombineSoundtrack()
		{
			var soundtrack = new Soundtrack(Cache);

			soundtrack.Extract(Overwrite, SoundtrackNameFilter?.ToArray() ?? new string[0]);
        }
	}
}