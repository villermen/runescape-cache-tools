using System.Collections.Generic;
using System.Threading.Tasks;
using Villermen.RuneScapeCacheTools.Cache.FlatFile;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class ExtractCommand : BaseCommand
    {
        public ExtractCommand(ArgumentParser argumentParser) : base(argumentParser)
        {
        }

        public override IList<string> Configure(IEnumerable<string> arguments)
        {
            this.ArgumentParser.Add(
                ParserOption.FileFilter,
                ParserOption.Java,
                ParserOption.Download,
                ParserOption.OverwriteFiles,
                ParserOption.OutputDirectory
            );

            return this.ArgumentParser.ParseArguments(arguments);
        }

        public override int Run()
        {
            var sourceCache = this.ArgumentParser.SourceCache;
            var outputCache = new FlatFileCache(this.ArgumentParser.OutputDirectory ?? "files");

            var indexes = this.ArgumentParser.FileFilter.Item1.Count > 0
                ? this.ArgumentParser.FileFilter.Item1
                : sourceCache.GetAvailableIndexes();

            foreach (var index in indexes)
            {
                var files = this.ArgumentParser.FileFilter.Item2.Count > 0
                    ? this.ArgumentParser.FileFilter.Item2
                    : sourceCache.GetAvailableFileIds(index);

                Parallel.ForEach(
                    files,
                    new ParallelOptions
                    {
                        // Not putting a limit here overloads the downloader when used.
                        MaxDegreeOfParallelism = 10,
                    },
                    (fileId) =>
                    {
                        var file = sourceCache.GetFile(index, fileId);
                        outputCache.PutFile(index, fileId, file);
                    }
                );
            }

            return 0;
        }
    }
}
