using System;
using System.Collections.Generic;
using NDesk.Options;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;

namespace Villermen.RuneScapeCacheTools.CLI
{
	internal static class Program
	{
		private static readonly CacheBase Cache = new RuneTek5Cache();

		private static int TriggeredActions { get; set; }

		private static IList<int> IndexIds { get; set; }

		private static IList<int> FileIds { get; set; }

		private static bool DoExtract { get; set; }

		private static bool Overwrite { get; set; }

		private static bool DoSoundtrackCombine { get; set; }

		private static readonly OptionSet ArgumentParser = new OptionSet
		{
			{
				"cache-directory=|c",
				"The directory in which RuneScape's cache files are located. If unspecified, the default directory will be attempted.",
				value =>
				{
					Cache.CacheDirectory = ParseDirectory(value);
				}
			},

			{
				"output-directory=|o",
				"The directory in which output files (mainly extracted files) will be stored.",
				value =>
				{
					Cache.OutputDirectory = ParseDirectory(value);
				}
			},

			{
				"temporary-directory=|t",
				"The directory in which temporary files will be stored. If unspecified, the system's default directory + \"rsct\" will be used.",
				value =>
				{
					Cache.TemporaryDirectory = ParseDirectory(value);
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
					IndexIds = ExpandIntegerRangeString(value);
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
				"soundtrack-combine|s", "DoExtract and name the entire soundtrack.",
				value =>
				{
					DoSoundtrackCombine = (value != null);
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

				// Show help if "Nothing interesting happens.", this is considered an error for automation
				if (TriggeredActions == 0)
				{
					ShowHelp();
					return 1;
				}
			}
			catch (OptionException optionException)
			{
				Console.WriteLine(optionException.Message);
				return 1;
			}

			// Perform the specified actions
			try
			{
				if (DoExtract)
				{
					Extract();
				}

				if (DoSoundtrackCombine)
				{
					CombineSoundtrack();
				}

				return 0;
			}
			catch (Exception actionException)
			{
				Console.WriteLine(actionException);
				return 1;
			}
		}

		/// <summary>
		/// Formats the given directory for use in the cache tools, and expands environment variables where given.
		/// </summary>
		/// <param name="directoryPath"></param>
		/// <returns></returns>
		private static string ParseDirectory(string directoryPath)
		{
			// Expand environment variables
			var result = Environment.ExpandEnvironmentVariables(directoryPath);

			// Replace backslashes with forward slashes
			result = result.Replace('\\', '/');

			// Add trailing slash
			if (!result.EndsWith("/"))
			{
				result += "/";
			}

			return result;
		}

		/// <summary>
		///   Expands the given integer range into an enumerable of all individual integers.
		/// </summary>
		/// <param name="integerRange">An integer range, e.g. "0-4,6,34,200-201"</param>
		/// <returns></returns>
		private static IList<int> ExpandIntegerRangeString(string integerRangeString)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		private static void CombineSoundtrack()
		{
			throw new NotImplementedException();
		}
	}
}