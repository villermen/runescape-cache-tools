using System;
using System.Threading.Tasks;
using Villermen.RuneScapeCacheTools.CLI.Argument;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class ExtractCommand : BaseCommand
    {
        public ExtractCommand(ArgumentParser argumentParser) : base(argumentParser)
        {
            this.ArgumentParser.AddCommon(CommonArgument.SourceCache);
            this.ArgumentParser.AddCommon(CommonArgument.FileFilter);
            this.ArgumentParser.AddCommon(CommonArgument.OutputCache);
        }

        public override int Run()
        {
            using var sourceCache = this.ArgumentParser.SourceCache;
            if (sourceCache == null)
            {
                Console.WriteLine("No cache source specified.");
                return 2;
            }

            var outputCache = this.ArgumentParser.GetOutputCache();

            var indexes = this.ArgumentParser.FileFilter.Item1.Length > 0
                ? this.ArgumentParser.FileFilter.Item1
                : sourceCache.GetAvailableIndexes();

            foreach (var index in indexes)
            {
                var files = this.ArgumentParser.FileFilter.Item2.Length > 0
                    ? this.ArgumentParser.FileFilter.Item2
                    : sourceCache.GetAvailableFileIds(index);

                Parallel.ForEach(files, (fileId) =>
                {
                    var file = sourceCache.GetFile(index, fileId);
                    outputCache.PutFile(index, fileId, file);
                });
            }

            return 0;
        }
    }
}
