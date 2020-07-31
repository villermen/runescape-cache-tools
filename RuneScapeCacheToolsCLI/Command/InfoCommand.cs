using System;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.CLI.Argument;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class InfoCommand : BaseCommand
    {
        private CacheIndex? _index;

        public InfoCommand(ArgumentParser argumentParser) : base(argumentParser)
        {
            this.ArgumentParser.AddCommon(CommonArgument.SourceJava);
            this.ArgumentParser.AddCommon(CommonArgument.SourceDownload);
            this.ArgumentParser.AddCommon(CommonArgument.FileFilter);
            this.ArgumentParser.AddCommon(CommonArgument.Overwrite);
            this.ArgumentParser.AddCommon(CommonArgument.OutputDirectory);
            this.ArgumentParser.AddPositional("index", "The index ID to list information of.", (value) => this._index = (CacheIndex)int.Parse(value));
        }

        public override int Run()
        {
            using var sourceCache = this.ArgumentParser.SourceCache;
            if (sourceCache == null)
            {
                Console.WriteLine("No cache source specified.");
                return 2;
            }
            if (this._index == null)
            {
                Console.WriteLine("No index specified.");
                return 2;
            }

            var referenceTable = sourceCache.GetReferenceTable(this._index.Value);

            // TODO: List info

            return 0;
        }
    }
}
