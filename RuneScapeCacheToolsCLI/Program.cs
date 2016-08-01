using System;
using System.Collections.Generic;
using NDesk.Options;

namespace Villermen.RuneScapeCacheTools.CLI
{
	internal static class Program
	{
		private static readonly OptionSet ArgumentParser = new OptionSet
		{
			{
				"cache-directory=|c",
				"The directory in which RuneScape's cache files are located. If unspecified, the default directory will be attempted.",
				value =>
				{

				}
			},

			{
				"output-directory=|o",
				"The directory in which output files (mainly extracted files) will be stored.",
				value =>
				{

				}
			},

			{
				"temporary-directory=|t",
				"The directory in which temporary files will be stored. If unspecified, the system's default directory + \"rsct\" will be used.",
				value =>
				{

				}
			},

			{
				"extract|e",
				"Extract all files from the cache. You can specify which files to extract by using the index and file arguments.",
				value =>
				{
					_triggeredActions++;
				}
			},

			{
				"index=|i", "A index id or range of index ids to extract (\"1-2,4,6-7\").",
				value =>
				{
					
				}
			},

			{
				"file=|f", "A file id or range of file ids to extract (\"1-2,4,6-7\").",
				value =>
				{
					
				}
			},

			{
				"overwrite", "Overwrite extracted files if they already exist.",
				value =>
				{
					
				}
			},

			{
				"soundtrack-combine|s", "Extract and name the entire soundtrack.",
				value =>
				{
					_triggeredActions++;
				}
			},

			{
				"help|h|?", "Show this message.", value =>
				{
					ShowHelp();

					_triggeredActions++;
				}
			}
		};

		private static int _triggeredActions;

		private static int Main(string[] args)
		{
			try
			{
				var unknownArguments = ArgumentParser.Parse(args);

				foreach (var unknownArgument in unknownArguments)
				{
					Console.WriteLine($"Unknown argument \"{unknownArgument}\".");
				}

				if (unknownArguments.Count > 0)
				{
					Console.WriteLine();
				}

				if (_triggeredActions == 0)
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

			return 0;
		}

		private static string ParseDirectory(string directoryPath)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Expands the given integer range into an enumerable of all individual integers.
		/// </summary>
		/// <param name="integerRange">An integer range, e.g. "0-4,6,34,200-201"</param>
		/// <returns></returns>
		private static IEnumerable<int> ParseIntegerRange(string integerRange)
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
	}
}