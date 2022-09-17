using System;
using System.Threading.Tasks;
using Serilog;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.CLI.Argument;
using Villermen.RuneScapeCacheTools.Exception;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class ImportCommand : BaseCommand
    {
        public ImportCommand(ArgumentParser argumentParser) : base(argumentParser)
        {
            this.ArgumentParser.AddCommon(CommonArgument.Cache);
            this.ArgumentParser.AddCommon(CommonArgument.Directory);
            this.ArgumentParser.AddCommon(CommonArgument.Files);
            this.ArgumentParser.AddCommon(CommonArgument.Preserve);
        }

        public override int Run()
        {
            using var sourceCache = new FlatFileCache(this.ArgumentParser.Directory ?? "files")
            {
            };

            using var outputCache = this.ArgumentParser.Cache;
            if (outputCache == null)
            {
                Console.WriteLine("No target cache specified.");
                return Program.ExitCodeInvalidArgument;
            }

            if (this.ArgumentParser.FileFilter == null || this.ArgumentParser.FileFilter.Item1.Length == 0)
            {
                Console.WriteLine("No files to extract specified.");
                return Program.ExitCodeInvalidArgument;
            }


            Console.WriteLine("Import completed.");
            return Program.ExitCodeOk;
        }
    }
}
