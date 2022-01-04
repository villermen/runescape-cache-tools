using System;
using System.Threading.Tasks;
using Serilog;
using Villermen.RuneScapeCacheTools.CLI.Argument;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class AudioCommand : BaseCommand
    {
        /// <summary>
        /// Limits the amount of SoX instances to not fry your PC.
        /// </summary>
        private const int Parallelism = 10;

        private bool _lossless = false;
        private string[] _trackNameFilters = {};
        private bool _overwrite = false;
        private bool _includeUnnamed;
        private Tuple<CacheIndex[], int[]>? _scrapeFiles;

        public AudioCommand(ArgumentParser argumentParser) : base(argumentParser)
        {
            this.ArgumentParser.AddCommon(CommonArgument.Cache);
            this.ArgumentParser.AddCommon(CommonArgument.Directory);
            this.ArgumentParser.Add(
                "flac",
                "Use FLAC format instead of original OGG for a (tiny) quality improvement.",
                value => { this._lossless = true; }
            );
            this.ArgumentParser.Add(
                "filter=|f",
                "Process only tracks containing any of the given comma-separated names. E.g., \"scape,dark\".",
                value => { this._trackNameFilters = value.Split(','); }
            );
            this.ArgumentParser.Add(
                "overwrite",
                "Overwrite extracted tracks if they already exist. Tracks with a different version will always be overwritten.",
                value => { this._overwrite = true; }
            );
            this.ArgumentParser.Add(
                "unnamed",
                "Include tracks that have an invalid or non-existent name (will use file ID as name).",
                value => { this._includeUnnamed = true; }
            );
            this.ArgumentParser.Add(
                "scrape=",
                "Don't use track names but instead try all the given files to combine JAGA audio.",
                value => { this._scrapeFiles = ArgumentParser.ParseFileFilter(value); }
            );
        }

        public override int Run()
        {
            using var sourceCache = this.ArgumentParser.Cache;
            if (sourceCache == null)
            {
                Console.WriteLine("No cache source specified.");
                return Program.ExitCodeInvalidArgument;
            }

            var soundtrackExtractor = new SoundtrackExtractor(
                sourceCache,
                this.ArgumentParser.Directory ?? "audio"
            );

            if (this._scrapeFiles == null)
            {
                soundtrackExtractor.ExtractSoundtrack(
                    this._overwrite,
                    this._lossless,
                    this._includeUnnamed,
                    this._trackNameFilters,
                    AudioCommand.Parallelism
                );
                Console.WriteLine("Done combining soundtracks.");
                return Program.ExitCodeOk;
            }

            if (this._scrapeFiles.Item1.Length == 0)
            {
                Console.WriteLine("No files to scrape specified.");
                return Program.ExitCodeInvalidArgument;
            }

            foreach (var index in this._scrapeFiles.Item1)
            {
                var fileIds = this._scrapeFiles.Item2.Length > 0
                    ? this._scrapeFiles.Item2
                    : sourceCache.GetAvailableFileIds(index);

                Parallel.ForEach(
                    fileIds,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = AudioCommand.Parallelism,
                    },
                    fileId =>
                    {
                        try
                        {
                            var file = sourceCache.GetFile(index, fileId);
                            soundtrackExtractor.ExtractIfJagaFile(file, $"{(int)index}-{fileId}", this._overwrite, this._lossless);
                        }
                        catch (SoundtrackException exception)
                        {
                            if (!exception.IsSoxError)
                            {
                                throw;
                            }

                            Log.Information($"Failed to combine {(int)index}/{fileId}: {exception.Message}");
                        }
                    }
                );
            }

            Console.WriteLine("Done scraping audio.");
            return Program.ExitCodeOk;
        }
    }
}
