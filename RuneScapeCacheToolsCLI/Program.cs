using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Villermen.RuneScapeCacheTools.CLI.Command;

namespace Villermen.RuneScapeCacheTools.CLI
{
    public class Program
    {
        public static readonly IDictionary<string, string> Commands = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>
            {
                {"extract", "Extract cache files from various sources into an easily explorable directory structure."},
                {"info", "Obtain information about a stored cache index."},
                // {"soundtrack", "Combine the in-game listed soundtrack."},
                // {"audio", "Combine arbitrary audio files from the cache."},
            }
        );

        private static int Main(string[] arguments)
        {
            // Configure logging.
            var loggingLevelSwitch = new LoggingLevelSwitch
            {
                MinimumLevel = LogEventLevel.Information
            };
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(loggingLevelSwitch)
                .WriteTo.Console()
                .CreateLogger();

            // Handle all exceptions by showing them in the console
            try
            {
                var argumentParser = new ArgumentParser(loggingLevelSwitch);

                // Show help on modules
                if (arguments.Length == 0 || arguments[0] == "help" || !Program.Commands.ContainsKey(arguments[0]))
                {
                    return new HelpCommand(argumentParser, null).Run();
                }

                var commandArgument = arguments[0];

                BaseCommand command;
                switch (commandArgument)
                {
                    case "extract":
                        command = new ExtractCommand(argumentParser);
                        break;

                    case "info":
                        command = new InfoCommand(argumentParser);
                        break;

                    // case "audio":
                    //     // command = new AudioCommand();
                    //     break;
                    //
                    // case "soundtrack":
                    //     break;

                    default:
                        // Should not happen because of earlier Program.Commands check
                        throw new InvalidOperationException("Command argument was unhandled.");
                }

                var unparsedArguments = command.Configure(arguments.Skip(1));

                // We don't care about the command if help was requested
                if (argumentParser.Help)
                {
                    command = new HelpCommand(argumentParser, commandArgument);
                }
                else if (unparsedArguments.Count > 0)
                {
                    // Do not accept invalid arguments because they usually indicate faulty usage
                    foreach (var unparsedArgument in unparsedArguments)
                    {
                        Console.WriteLine($"Unknown argument \"{unparsedArgument}\".");
                    }

                    return 1;
                }

                return command.Run();
            }
            catch (System.Exception exception)
            {
                Log.Fatal(exception.Message + "\n" + exception.StackTrace);
                return 2;
            }
        }
	}
}
