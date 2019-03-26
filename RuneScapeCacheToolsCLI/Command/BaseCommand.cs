using System.Collections.Generic;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public abstract class BaseCommand
    {
        protected readonly ArgumentParser ArgumentParser;

        protected BaseCommand(ArgumentParser argumentParser)
        {
            this.ArgumentParser = argumentParser;
        }

        public abstract IList<string> Configure(IEnumerable<string> arguments);

        public abstract int Run();
    }
}
