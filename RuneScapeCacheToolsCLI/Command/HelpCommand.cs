using System;
using System.Linq;
using System.Reflection;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class HelpCommand : BaseCommand
    {
        private readonly string? _command;

        public HelpCommand(ArgumentParser argumentParser, string? command) : base (argumentParser)
        {
            if (command != null && !Program.Commands.ContainsKey(command))
            {
                throw new ArgumentException("Passed command must be valid.");
            }

            this._command = command;
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

            if (this._command == null)
            {
                // Show generic help.
                Console.WriteLine($"Usage: {name} [command] [...options]");

                // Show help for all available commands.
                Console.WriteLine();
                foreach (var pair in Program.Commands)
                {
                    Console.WriteLine("      " + pair.Key.PadRight(23) + pair.Value);
                }
                Console.WriteLine();
                Console.WriteLine($"Run {name} [command] --help for available options for a command.");
                return 1;
            }

            // Show help for specific command.
            var positionalHelp = "";
            if (this.ArgumentParser.PositionalArgumentNames.Any())
            {
                positionalHelp = $"[{String.Join("] [", this.ArgumentParser.PositionalArgumentNames)}]";
            }

            Console.WriteLine($"Usage: {name} {this._command} [...options] {positionalHelp}");
            Console.WriteLine(Program.Commands[this._command]);
            Console.WriteLine();
            Console.WriteLine(this.ArgumentParser.GetDescription());

            return 1;
        }
    }
}
