using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Villermen.RuneScapeCacheTools.CLI.Argument;
using Villermen.RuneScapeCacheTools.CLI.Command;

namespace Villermen.RuneScapeCacheTools.CLI
{
    public class Program
    {
        public const int ExitCodeOk = 0;
        public const int ExitCodeError = 1;
        public const int ExitCodeInvalidArgument = 2;

        public static readonly IDictionary<string, string> Commands = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>
            {
                {"extract", "Extract cache files from various sources into an easily explorable directory structure."},
                {"audio", "Extract and combine the game's soundtrack."},
                {"items", "Extract and filter item definitions."},
                {"info", "Obtain information about a stored cache index or its files."},
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

                    case "audio":
                        command = new AudioCommand(argumentParser);
                        break;

                    case "items":
                        command = new ItemsCommand(argumentParser);
                        break;

                    case "help":
                    case "--help":
                    case "--version":
                        commandArgument = "help";
                        break;
                }

                if (command == null)
                {
                    Program.ShowHelp(argumentParser, commandArgument);
                    return commandArgument == "help" ? Program.ExitCodeOk : Program.ExitCodeInvalidArgument;
                }

                command.Configure(arguments.Skip(1));

                // Show help, now with configured argument parser for command-specific help.
                if (showHelp)
                {
                    Program.ShowHelp(argumentParser, commandArgument);
                    return Program.ExitCodeOk;
                }

                if (argumentParser.UnparsedArguments.Any())
                {
                    // Do not accept invalid arguments because they usually indicate faulty usage
                    foreach (var unparsedArgument in argumentParser.UnparsedArguments)
                    {
                        Console.WriteLine($"Unknown argument \"{unparsedArgument}\".");
                    }

                    Console.WriteLine();
                    Program.ShowHelp(argumentParser, commandArgument);
                    return Program.ExitCodeInvalidArgument;
                }

                var exitCode = command.Run();
                if (exitCode == Program.ExitCodeInvalidArgument)
                {
                    Console.WriteLine();
                    Program.ShowHelp(argumentParser, commandArgument);
                }

                return exitCode;
            }
            catch (System.Exception exception)
            {
                // Use first exception in aggregates for easier debugging.
                while (exception is AggregateException && exception.InnerException != null)
                {
                    exception = exception.InnerException;
                }

                // Rethrow for debugger to process.
                if (Debugger.IsAttached)
                {
                    throw;
                }

                Log.Fatal(exception.Message + "\n" + exception.StackTrace);
                return Program.ExitCodeError;
            }
        }

        private static void ShowHelp(ArgumentParser argumentParser, string commandArgument)
        {
            var assembly = Assembly.GetExecutingAssembly();

            if (!Program.Commands.ContainsKey(commandArgument))
            {
                if (commandArgument == "help")
                {
                    // Show program info.
                    var version = $"{assembly.GetName().Version.Major}.{assembly.GetName().Version.Minor}";
                    var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

                    Console.WriteLine($"Viller's RuneScape Cache Tools v{version}.");
                    Console.WriteLine(description);
                    Console.WriteLine();
                }
                else
                {
                    // Show invalid command message.
                    Console.WriteLine($"Invalid command \"{commandArgument}\".");
                    Console.WriteLine();
                }

                // Show generic help.
                Console.WriteLine("Usage: rsct.exe [command] [...options]");

                // Show help for all available commands.
                foreach (var pair in Program.Commands)
                {
                    Console.WriteLine($"      {pair.Key.PadRight(23)}{pair.Value}");
                }
                Console.WriteLine();
                Console.WriteLine("Run rsct.exe [command] --help for available options for a command.");
                return;
            }

            // Show help for specific command.
            var positionalHelp = "";
            if (argumentParser.PositionalArgumentNames.Any())
            {
                positionalHelp = $"[{string.Join("] [", argumentParser.PositionalArgumentNames)}]";
            }

            Console.WriteLine($"Usage: rsct.exe {commandArgument} [...options] {positionalHelp}");
            Console.WriteLine(Program.Commands[commandArgument]);
            Console.WriteLine(argumentParser.GetDescription());
        }
	}
}
