using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Villermen.RuneScapeCacheTools.Cache.FlatFile;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class ExtractCommand : BaseCommand
    {
        public ExtractCommand(ArgumentParser argumentParser) : base(argumentParser)
        {
            this.ArgumentParser.AddCommon(CommonArgument.FileFilter);
            this.ArgumentParser.AddCommon(CommonArgument.SourceJava);
            this.ArgumentParser.AddCommon(CommonArgument.SourceDownload);
            this.ArgumentParser.AddCommon(CommonArgument.Overwrite);
            this.ArgumentParser.AddCommon(CommonArgument.OutputDirectory);
        }

        public override int Run()
        {
            using var sourceCache = this.ArgumentParser.SourceCache;
            if (sourceCache == null)
            {
                Console.WriteLine("No cache source specified.");
                return 2;
            }

            var outputCache = new FlatFileCache(this.ArgumentParser.OutputDirectory);

            var indexes = this.ArgumentParser.FileFilter.Item1.Length > 0
                ? this.ArgumentParser.FileFilter.Item1
                : sourceCache.GetAvailableIndexes();

            foreach (var index in indexes)
            {
                var files = this.ArgumentParser.FileFilter.Item2.Length > 0
                    ? this.ArgumentParser.FileFilter.Item2
                    : sourceCache.GetAvailableFileIds(index);

                try
                {
                    Parallel.ForEach(
                        files,
                        new ParallelOptions
                        {
                            // Not putting a limit here overloads the downloader when used.
                            MaxDegreeOfParallelism = 1, // TODO: Set back to 10 after downloader is made to work with it again.
                        },
                        (fileId) =>
                        {
                            var file = sourceCache.GetFile(index, fileId);
                            outputCache.PutFile(index, fileId, file);
                        }
                    );
                }
                catch (AggregateException exception)
                {
                    // Report with correct stack trace.
                    ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                }
            }

            return 0;
        }
    }
}
