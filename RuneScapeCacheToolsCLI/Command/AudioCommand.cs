using System;
using Villermen.RuneScapeCacheTools.CLI.Argument;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class AudioCommand : BaseCommand
    {
        private bool _lossless = false;
        private string[] _trackNameFilters = {};
        private bool _overwrite = false;
        private bool _includeUnnamed;

        public AudioCommand(ArgumentParser argumentParser) : base(argumentParser)
        {
            this.ArgumentParser.AddCommon(CommonArgument.SourceCache);
            this.ArgumentParser.AddCommon(CommonArgument.OutputDirectory);
            // TODO: this.ArgumentParser.AddCommon(CommonArgument.Files); for scraping
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
        }

        public override int Run()
        {
            using var sourceCache = this.ArgumentParser.SourceCache;
            if (sourceCache == null)
            {
                Console.WriteLine("No cache source specified.");
                return Program.ExitCodeInvalidArgument;
            }

            var soundtrackExtractor = new SoundtrackExtractor(
                sourceCache,
                this.ArgumentParser.OutputDirectory ?? "soundtrack"
            );
            soundtrackExtractor.ExtractSoundtrack(this._overwrite, this._lossless, this._includeUnnamed, this._trackNameFilters);

            return Program.ExitCodeOk;
        }
    }
}
