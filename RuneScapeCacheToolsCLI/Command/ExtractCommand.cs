using System;
using System.Threading.Tasks;
using Serilog;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.CLI.Argument;
using Villermen.RuneScapeCacheTools.Exception;

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
                return Program.ExitCodeInvalidArgument;
            }

            if (this.ArgumentParser.FileFilter == null || this.ArgumentParser.FileFilter.Item1.Length == 0)
            {
                Console.WriteLine("No files to extract specified.");
                return Program.ExitCodeInvalidArgument;
            }

            using var outputCache = new FlatFileCache(this.ArgumentParser.OutputDirectory ?? "files")
            {
                OverwriteFiles = !this._preserve,
            };

            foreach (var index in this.ArgumentParser.FileFilter.Item1)
            {
                var fileIds = this.ArgumentParser.FileFilter.Item2.Length > 0
                    ? this.ArgumentParser.FileFilter.Item2
                    : sourceCache.GetAvailableFileIds(index);

                Parallel.ForEach(fileIds, (fileId) =>
                {
                    try
                    {
                        var file = sourceCache.GetFile(index, fileId);
                        outputCache.PutFile(index, fileId, file);

                        Log.Information($"File {(int)index}/{fileId}: Extracted.");
                    }
                    catch (CacheFileNotFoundException exception)
                    {
                        Log.Information($"File {(int)index}/{fileId}: {exception.Message}");
                    }
                });
            }

            Console.WriteLine("Extraction completed.");
            return Program.ExitCodeOk;
        }
    }
}
