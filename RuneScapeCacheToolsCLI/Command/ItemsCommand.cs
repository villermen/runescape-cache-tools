using System;
using Villermen.RuneScapeCacheTools.CLI.Argument;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class ItemsCommand : BaseCommand
    {
        private bool _skip = false;
        private string? _save = null;
        private string? _filter = null; // TODO: Verify format.

        public ItemsCommand(ArgumentParser argumentParser) : base(argumentParser)
        {
            this.ArgumentParser.AddCommon(CommonArgument.SourceCache);
            this.ArgumentParser.Add(
                "save:",
                "Save the resulting JSON to the given file (or \"items.json\" when no value is supplied).",
                (value) => { this._save = value; }
            );
            this.ArgumentParser.Add(
                "skip",
                "Skip items that can't be decoded.",
                (value) => { this._skip = true; }
            );
            this.ArgumentParser.Add(
                "filter=",
                "Filter items by their properties. E.g., \"name:kwuarm\" or \"properties.unknown2195\".",
                (value) => this._filter=value
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

            var itemDefinitionExtractor = new ItemDefinitionExtractor(sourceCache, this._save);
            itemDefinitionExtractor.ExtractItemDefinitions(this._filter, this._skip);

            return Program.ExitCodeOk;
        }
    }
}
