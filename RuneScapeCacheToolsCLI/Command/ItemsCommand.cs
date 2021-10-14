using System;
using System.Threading.Tasks;
using Serilog;
using Villermen.RuneScapeCacheTools.CLI.Argument;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class ItemsCommand : BaseCommand
    {
        public ItemsCommand(ArgumentParser argumentParser) : base(argumentParser)
        {
            this.ArgumentParser.AddCommon(CommonArgument.SourceCache);
            this.ArgumentParser.AddCommon(CommonArgument.OutputDirectory);
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

            itemDefinitionExtractor.ExtractItemDefinitions();
            return Program.ExitCodeOk;
        }
    }
}
