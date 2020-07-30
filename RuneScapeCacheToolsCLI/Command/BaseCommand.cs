using System.Collections.Generic;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public abstract class BaseCommand
    {
        protected readonly ArgumentParser ArgumentParser;

        protected BaseCommand(ArgumentParser argumentParser)
        {
            this.ArgumentParser = argumentParser;

            this.ArgumentParser.AddCommon(CommonArgument.Help);
            this.ArgumentParser.AddCommon(CommonArgument.Verbose);
        }

        public IList<string> Configure(IEnumerable<string> arguments)
        {
            return this.ArgumentParser.ParseArguments(arguments);
        }

        public abstract int Run();
    }
}
