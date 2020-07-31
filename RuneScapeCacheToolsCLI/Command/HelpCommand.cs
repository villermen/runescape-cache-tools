using System;
using System.Linq;
using System.Reflection;
using Villermen.RuneScapeCacheTools.CLI.Argument;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class HelpCommand : BaseCommand
    {
        private readonly string _commandArgument;

        public HelpCommand(ArgumentParser argumentParser, string commandArgument) : base (argumentParser)
        {
            this._commandArgument = commandArgument;
        }

        public override int Run()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var name = assembly.GetName().Name;

            if (!Program.Commands.ContainsKey(this._commandArgument))
            {
                if (this._commandArgument == "help")
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
                    Console.WriteLine($"Invalid command \"{this._commandArgument}\".");
                    Console.WriteLine();
                }

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
                positionalHelp = $"[{string.Join("] [", this.ArgumentParser.PositionalArgumentNames)}]";
            }

            Console.WriteLine($"Usage: {name} {this._commandArgument} [...options] {positionalHelp}");
            Console.WriteLine(Program.Commands[this._commandArgument]);
            Console.WriteLine();
            Console.WriteLine(this.ArgumentParser.GetDescription());

            return 1;
        }
    }
}
