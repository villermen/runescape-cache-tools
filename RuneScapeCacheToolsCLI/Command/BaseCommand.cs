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

        public IList<string> Configure(IEnumerable<string> arguments)
        {
            return this.ArgumentParser.ParseArguments(arguments);
        }

        public abstract int Run();
    }
}
