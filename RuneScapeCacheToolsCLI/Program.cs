using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Villermen.RuneScapeCacheTools.CLI.Argument;
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

            var showHelp = false;
            var argumentParser = new ArgumentParser();
            argumentParser.Add("help|version|?", "Show this message.", (value) => { showHelp = true; });
            argumentParser.Add("verbose|v", "Increase amount of log messages.", (value) => loggingLevelSwitch.MinimumLevel = LogEventLevel.Debug);

            // Handle all exceptions by showing them in the console
            try
            {
                var commandArgument = arguments.Length > 0 ? arguments[0] : "help";

                BaseCommand? command = null;
                switch (commandArgument)
                {
                    case "extract":
                        command = new ExtractCommand(argumentParser);
                        break;

                    case "info":
                        command = new InfoCommand(argumentParser);
                        break;
                }

                IList<string> unparsedArguments = new List<string>();
                if (command != null)
                {
                    unparsedArguments = command.Configure(arguments.Skip(1));
                }

                // Can be true because of switch or because of --help argument. ArgumentParser is configured for the
                // other command so we can list command-specific help.
                if (command == null || showHelp)
                {
                    command = new HelpCommand(argumentParser, commandArgument);
                }
                else if (unparsedArguments.Any())
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
