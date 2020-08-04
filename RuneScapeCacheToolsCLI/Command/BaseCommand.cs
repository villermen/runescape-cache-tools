using System.Collections.Generic;
using Villermen.RuneScapeCacheTools.CLI.Argument;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public abstract class BaseCommand
    {
        protected readonly ArgumentParser ArgumentParser;

        protected BaseCommand(ArgumentParser argumentParser)
        {
            this.ArgumentParser = argumentParser;
        }

        public void Configure(IEnumerable<string> arguments)
        {
            this.ArgumentParser.ParseArguments(arguments);
        }

        public abstract int Run();
    }
}
