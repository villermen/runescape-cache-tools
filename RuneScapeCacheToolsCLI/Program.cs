using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using log4net;
using Villermen.RuneScapeCacheTools.CLI.Command;

namespace Villermen.RuneScapeCacheTools.CLI
{
    public class Program
    {
        public static readonly IDictionary<string, string> Commands = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>
            {
                {"export", "Extract cache files from various sources into an easily explorable directory structure."},
                // {"import", "Insert exported files into a cache in Jagex's format."},
                {"soundtrack", "Combine the in-game listed soundtrack."},
                {"audio", "Combine arbitrary audio files from the cache."},
            }
        );

		/// <summary>
		/// Even when not used, this needs to be here to initialize the logging system.
		/// </summary>
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        private static int Main(string[] arguments)
        {
            var returnCode = Program.Run(arguments);

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

	    private static int Run(string[] arguments)
	    {
            var argumentParser = new ArgumentParser();

            // Handle all exceptions by showing them in the console
            try
            {
                // Show help on modules
                if (arguments.Length == 0 || arguments[0] == "help" || !Program.Commands.ContainsKey(arguments[0]))
                {
                    return new HelpCommand(null, null).Run();
                }

                var commandArgument = arguments[0];
                var otherArguments = arguments.Skip(1);

                argumentParser.Add(ParserOption.Help, ParserOption.Verbose);

                BaseCommand command;

                switch (commandArgument)
                {
                    case "export":
                        command = new ExportCommand(argumentParser);
                        break;

                    // case "audio":
                    //     // command = new AudioCommand();
                    //     break;
                    //
                    // case "soundtrack":
                    //     break;

                    default:
                        // Should not happen because of ealier Program.Commands check
                        throw new InvalidOperationException("Command argument was unhandled.");
                }

                var unparsedArguments = command.Configure(otherArguments);

                // Do not accept invalid arguments because they usually indicate faulty usage
                if (unparsedArguments.Count > 0)
                {
                    foreach (var unparsedArgument in unparsedArguments)
                    {
                        Console.WriteLine($"Unknown argument \"{unparsedArgument}\".");
                    }

                    return 1;
                }

                // We don't care about the command if help was requested
                if (argumentParser.Help)
                {
                    command = new HelpCommand(commandArgument, argumentParser);
                }

                return command.Run();
            }
            catch (System.Exception exception)
            {
                Console.WriteLine(argumentParser.Verbose ? exception.ToString() : exception.Message);
                return 2;
            }
        }
	}
}
