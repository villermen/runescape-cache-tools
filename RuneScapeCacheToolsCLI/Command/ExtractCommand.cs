using System;
using System.Threading.Tasks;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.CLI.Argument;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class ExtractCommand : BaseCommand
    {
        private bool _preserve = false;

        public ExtractCommand(ArgumentParser argumentParser) : base(argumentParser)
        {
            this.ArgumentParser.AddCommon(CommonArgument.SourceCache);
            this.ArgumentParser.AddCommon(CommonArgument.OutputDirectory);
            this.ArgumentParser.AddCommon(CommonArgument.Files);

            this.ArgumentParser.Add(
                "preserve",
                "Preserve existing files.",
                (value) => { this._preserve = true; }
            );
        }

        public override int Run()
        {
            using var sourceCache = this.ArgumentParser.SourceCache;
            if (sourceCache == null)
            {
                Console.WriteLine("No cache source specified.");
                return 2;
            }

            if (this.ArgumentParser.FileFilter == null || this.ArgumentParser.FileFilter.Item1.Length == 0)
            {
                Console.WriteLine("No files to extract specified.");
                return 2;
            }

            using var outputCache = new FlatFileCache(this.ArgumentParser.OutputDirectory ?? "files")
            {
                OverwriteFiles = !this._preserve,
            };

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
