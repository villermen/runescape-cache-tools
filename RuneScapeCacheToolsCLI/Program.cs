using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using log4net.Core;
using NDesk.Options;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.CLI
{
    public class Program
    {
		/// <summary>
		/// Even when not used, this needs to be here to initialize the logging system.
		/// </summary>
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        private static int Main(string[] arguments)
        {
            var returnCode = new Program().Run(arguments);

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

        protected readonly IDictionary<string, string> Commands = new Dictionary<string, string>
        {
            {"export", "Extract cache files from various sources into an easily explorable directory structure."},
            // {"import", "Insert exported files into a cache in Jagex's format."},
            {"soundtrack", "Combine the in-game listed soundtrack."},
            {"audio", "Combine arbitrary audio files from the cache."},
        };

	    private int Run(string[] arguments)
	    {
            // Show help on modules
            if (arguments.Length == 0 || !this.Commands.ContainsKey(arguments[0]))
            {
                return this.WriteHelp(null, null);
            }

            var command = arguments[0];

            var writeHelp = false;
            var overwrite = false;
            var verbose = false;
            var flac = false;
            string filter = null;
            string source = null;
            string output = null;

            var optionSet = new OptionSet();

            // Options available for all commands
            optionSet.Add("verbose|v", "Increase amount of log messages.", (value) => {
                // Lower log output level
                ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).Root.Level = Level.Debug;
                ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);

                verbose = true;
            });
            optionSet.Add("help|version|?", "Show this message.", (value) => {
                // Show help after adding all options so the usage tables are complete
                writeHelp = true;
            });
            optionSet.Add("overwrite", "Overwrite files if they already exist.", (value) => {
                overwrite = true;
            });
            optionSet.Add(
                "filter=|f",
                "Process only files matching the given pattern. E.g., \"40/*\" or \"*/1-1000\".",
                (value) => {
                    filter = value;
                }
             );
            optionSet.Add("source=", "Process only files matching the given pattern. E.g., \"40/*\" or \"*/1-1000\".", (value) => {
                source = value;

                if (source == "download")
                {
                    return;
                }

                source = PathExtensions.FixDirectory(source);
                if (!Directory.Exists(source))
                {
                    throw new ArgumentException($"Source directory \"{source}\" does not exist.");
                }
            });
            optionSet.Add("output=", "Store processed files in this directory.", (value) => {
                output = PathExtensions.FixDirectory(value);
            });

            if (command == "audio" || command == "soundtrack")
            {
                optionSet.Add(
                    "flac",
                    "Use FLAC format instead of original OGG for a tiny quality improvement.",
                    (value) => {
                        flac = true;
                    }
                );
            }

            // Handle all exceptions from here by showing them in the console
            try
            {
                var unparsedOptions = optionSet.Parse(arguments.Skip(1));

                // Do not accept invalid options because they usually indicate faulty usage
                if (unparsedOptions.Count > 0)
                {
                    foreach (var unparsedOption in unparsedOptions)
                    {
                        Console.WriteLine($"Unknown option \"{unparsedOption}\".");
                    }

                    return 1;
                }

                if (writeHelp)
                {
                    return this.WriteHelp(command, optionSet);
                }

                switch (command)
                {
                    case "export":
                        // return new ExportCommand().Run(stuff...);
                        break;

                    case "audio":
                        // return new AudioCommand().Run(stuff...);
                        break;

                    case "soundtrack":
                        // return new SoundtrackCommand().Run(stuff...);
                        break;
                }

                throw new InvalidOperationException($"Command \"{command}\" was not handled.");
            }
            catch (Exception exception)
            {
                Console.WriteLine(verbose ? exception.ToString() : exception.Message);
                return 2;
            }
        }

        private int WriteHelp(string command, OptionSet optionSet)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var name = assembly.GetName().Name;
            var version = $"{assembly.GetName().Version.Major}.{assembly.GetName().Version.Minor}";
            var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

            Console.WriteLine($"Viller's RuneScape Cache Tools v{version}.");
            Console.WriteLine(description);
            Console.WriteLine();
            Console.WriteLine($"Usage: {name} {command ?? "[command]"} [...options]");

            if (command == null)
            {
                Console.WriteLine();
                foreach (var pair in this.Commands)
                {
                    Console.WriteLine("      " + pair.Key.PadRight(23) + pair.Value);
                }
                Console.WriteLine();
                Console.WriteLine($"Run {name} help [command] --help for available options for a command.");
            }
            else
            {
                Console.WriteLine(this.Commands[command]);
                Console.WriteLine();
                optionSet.WriteOptionDescriptions(Console.Out);
            }

            return 1;
        }
	}
}
