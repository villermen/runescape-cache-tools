using System;
using Villermen.RuneScapeCacheTools.CLI.Argument;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class ItemsCommand : BaseCommand
    {
        private bool _skip = false;
        private string _file = "items.json";
        private string? _print = null;
        private bool _force = false;

        public ItemsCommand(ArgumentParser argumentParser) : base(argumentParser)
        {
            this.ArgumentParser.AddCommon(CommonArgument.Cache);
            this.ArgumentParser.Add(
                "file=",
                "Save item JSON to the given file (instead of \"items.json\").",
                (value) => { this._file = value; }
            );
            this.ArgumentParser.Add(
                "skip",
                "Skip items that can't be decoded.",
                (value) => { this._skip = true; }
            );
            this.ArgumentParser.Add(
                "force",
                "Force fresh extraction of JSON even when JSON version matches.",
                (value) => { this._force = true; }
            );
            this.ArgumentParser.Add(
                "print=",
                "Prints items matching the given filter. E.g., \"name:kwuarm*,properties.unknown2195\" for items whose name start with kwuarm and have properties.unknown2195 set.",
                (value) => this._print=value
            );
        }

        public override int Run()
        {
            var itemDefinitionExtractor = new ItemDefinitionExtractor();

            // Try to extract only when source is specified.
            using var sourceCache = this.ArgumentParser.Cache;
            if (sourceCache != null)
            {
                if (this._force || !itemDefinitionExtractor.JsonMatchesCache(sourceCache, this._file))
                {
                    itemDefinitionExtractor.ExtractItemDefinitions(sourceCache, this._file, this._skip);
                }
                else
                {
                    Console.WriteLine("Skipping extraction because JSON is up to date with cache.");
                }
            }
            else
            {
                if (this._force)
                {
                    Console.WriteLine("A source cache is required when forcing item extraction.");
                    return Program.ExitCodeInvalidArgument;
                }
            }

            if (this._print != null)
            {
                itemDefinitionExtractor.PrintItemDefinitions(this._file, this._print, Console.Out);
            }

            return Program.ExitCodeOk;
        }
    }
}
