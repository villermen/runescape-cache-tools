using System;
using System.Collections.Generic;
using System.Reflection;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class HelpCommand : BaseCommand
    {
        private readonly string _command;

        public HelpCommand(string command, ArgumentParser argumentParser) : base (argumentParser)
        {
            if (command == null ^ argumentParser == null)
            {
                throw new ArgumentException("ArgumentParser must be passed if command is passed and vice versa.");
            }

            if (command != null && !Program.Commands.ContainsKey(command))
            {
                throw new ArgumentException("Passed command must be valid.");
            }

            this._command = command;
        }

        public override IList<string> Configure(IEnumerable<string> arguments)
        {
            // Help command does not configure anything as it could use the already configured passed ArgumentParser
            return new List<string>();
        }

        public override int Run()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var name = assembly.GetName().Name;
            var version = $"{assembly.GetName().Version.Major}.{assembly.GetName().Version.Minor}";
            var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

            Console.WriteLine($"Viller's RuneScape Cache Tools v{version}.");
            Console.WriteLine(description);
            Console.WriteLine();
            Console.WriteLine($"Usage: {name} {this._command ?? "[command]"} [...options]");

            // Show help for passed command
            if (this._command != null)
            {
                Console.WriteLine(Program.Commands[this._command]);
                Console.WriteLine();
                Console.WriteLine(this.ArgumentParser.GetDescription());
                return 1;
            }

            // Show help for available commands
            Console.WriteLine();
            foreach (var pair in Program.Commands)
            {
                Console.WriteLine("      " + pair.Key.PadRight(23) + pair.Value);
            }
            Console.WriteLine();
            Console.WriteLine($"Run {name} help [command] --help for available options for a command.");

            return 1;
        }
    }
}
