using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.Downloader;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Villermen.RuneScapeCacheTools.Cache.FlatFile;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;

namespace Villermen.RuneScapeCacheTools.CLI
{
    public class Program
	{
		/// <summary>
		/// Even when not used, this needs to be here to initialize the logging system.
		/// </summary>
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

	    private readonly string[] _arguments;
		private readonly ArgumentParser _argumentParser = new ArgumentParser();
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

		    var returnCode = new Program(arguments).Run();

		    // Following code replaces hundred lines of code I would have had to write to accomplish the same with log4net...
		    // Delete log file if it is still empty when done
		    LogManager.GetRepository().ResetConfiguration();
		    var logFile = new FileInfo("rsct.log");
		    if (logFile.Exists && logFile.Length == 0)
		    {
			    logFile.Delete();
		    }

#if DEBUG
		    Console.ReadLine();
#endif

		    return returnCode;
        }

	    private Program(string[] arguments)
	    {
	        this._arguments = arguments;
	    }

	    private int Run()
	    {
	        return this.Configure()
	            ? this.Execute()
	            : 1;
	    }

		private bool Configure()
		{
			this._argumentParser.Parse(this._arguments);

			if (this._argumentParser.UnparsedArguments.Count > 0)
			{
				foreach (var unparsedArgument in this._argumentParser.UnparsedArguments)
				{
					Console.WriteLine($"Unknown argument \"{unparsedArgument}\".");
				}

			    Console.WriteLine();
			}

			if (this._argumentParser.NoActions)
			{
				Console.WriteLine("No actions specified.");
				Console.WriteLine("Use either the extract or soundtrack options to actually do something.");
				Console.WriteLine();
			}

			if (this._argumentParser.TooManyActions)
			{
				Console.WriteLine("Too many actions specified.");
				Console.WriteLine("Use either the extract or soundtrack option, but not both at the same time.");
				Console.WriteLine();
			}

		    if (this._argumentParser.ShowHelp)
		    {
		        Console.WriteLine($"Usage: {typeof(Program).Assembly.GetName().Name} [OPTION]...");
		        Console.WriteLine("Tools for performing actions on RuneScape's cache.");
		        Console.WriteLine();
		        this._argumentParser.WriteOptionDescriptions(Console.Out);
		    }

			// TODO: Split up into multiple executables. rsct-soundtrack, rsct-extract

		    return this._argumentParser.CanExecute;
		}

		private int Execute()
		{
            this._cache = this._argumentParser.Download ? (CacheBase)new DownloaderCache() : new RuneTek5Cache(this._argumentParser.CacheDirectory, true);

            // Perform the specified actions
            if (this._argumentParser.DoExtract)
            {
                this.Extract();
            }

            if (this._argumentParser.DoSoundtrackCombine)
            {
                this.CombineSoundtrack();
            }

			return 0;
		}

		private void Extract()
		{
			var outputCache = new FlatFileCache(this._argumentParser.OutputDirectory + "files/");

//            // Display progress at bottom of console without creating a new row
//            var progress = new ExtendedProgress();
//            progress.ProgressChanged += (p, message) =>
//            {
//                Console.Write($"Extraction progress: {Math.Round(progress.Percentage)}% ({progress.Current}/{progress.Total})\r");
//            };

			if (this._argumentParser.Indexes == null && this._argumentParser.FileIds != null)
			{
				throw new ArgumentException("If you specify files to extract you must also explicitly specify indexes to extract.");
			}

            foreach (var index in this._argumentParser.Indexes ?? this._cache.GetIndexes())
            {
                // Create a list of files to be extracted (requested if overwriting, missing if not)
                var requestedFileIds = this._argumentParser.FileIds ?? this._cache.GetFileIds(index);
                var existingFileIds = outputCache.GetFileIds(index).ToList();

                var fileIds = this._argumentParser.Overwrite
                    ? requestedFileIds
                    : requestedFileIds.Where(fileId => !existingFileIds.Contains(fileId));

                Parallel.ForEach(
                    fileIds,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 10,
                    },
                    fileId => { this._cache.CopyFile(index, fileId, outputCache); }
                );
			}
		}

		private void CombineSoundtrack()
		{
			var soundtrack = new Soundtrack(this._cache, this._argumentParser.OutputDirectory + "soundtrack/");

			if (this._argumentParser.TemporaryDirectory != null)
			{
				soundtrack.TemporaryDirectory = this._argumentParser.TemporaryDirectory;
			}

			soundtrack.Extract(this._argumentParser.Overwrite, this._argumentParser.Lossless,
				this._argumentParser.IncludeUnnamedSoundtracks, this._argumentParser.SoundtrackNameFilter?.ToArray() ?? new string[0]);
		}
	}
}
