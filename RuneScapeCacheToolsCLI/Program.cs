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
                {"soundtrack", "Combine the in-game listed soundtrack."},
                {"audio", "Combine arbitrary audio files from the cache."},
            }
        );

        private static int Main(string[] arguments)
        {
            // Configure logging.
            var loggingLevelSwitch = new LoggingLevelSwitch();
            loggingLevelSwitch.MinimumLevel = LogEventLevel.Information;
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(loggingLevelSwitch)
                .WriteTo.Console()
                .CreateLogger();

            var argumentParser = new ArgumentParser(loggingLevelSwitch);

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

                BaseCommand command;

                switch (commandArgument)
                {
                    case "extract":
                        command = new ExtractCommand(argumentParser);
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
                Log.Fatal(exception.Message + "\n" + exception.StackTrace);
                return 2;
            }
        }
	}
}
