using System;
using Villermen.RuneScapeCacheTools.CLI.Argument;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class ItemsCommand : BaseCommand
    {
        private bool _skip = false;

        public ItemsCommand(ArgumentParser argumentParser) : base(argumentParser)
        {
            this.ArgumentParser.AddCommon(CommonArgument.SourceCache);
            this.ArgumentParser.AddCommon(CommonArgument.OutputDirectory);
            this.ArgumentParser.Add(
                "skip",
                "Skip items that can't be decoded.",
                (value) => { this._skip = true; }
            );
        }

        public override int Run()
        {
            using var sourceCache = this.ArgumentParser.SourceCache;
            if (sourceCache == null)
            {
                Console.WriteLine("No cache source specified.");
                return Program.ExitCodeInvalidArgument;
            }

            var itemDefinitionExtractor = new ItemDefinitionExtractor(
                sourceCache,
                this.ArgumentParser.OutputDirectory ?? "."
            );

            itemDefinitionExtractor.ExtractItemDefinitions(this._skip);
            return Program.ExitCodeOk;
        }
    }
}
