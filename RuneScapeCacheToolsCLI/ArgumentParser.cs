using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NDesk.Options;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.Downloader;
using Villermen.RuneScapeCacheTools.Cache.FlatFile;
using Villermen.RuneScapeCacheTools.Cache.JavaClient;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.CLI
{
    public class ArgumentParser
    {
        public bool Verbose { get; private set; }

        public bool Help { get; private set; }

        public bool Overwrite { get; private set; }

        public bool Flac { get; private set; }

        public ICache<RuneTek5CacheFile>? SourceCache { get; private set; }

        public string? OutputDirectory { get; private set; }

        public Tuple<IList<CacheIndex>, IList<int>> FileFilter { get; private set; } = new Tuple<IList<CacheIndex>, IList<int>>(
            new List<CacheIndex>(),
            new List<int>()
        );

        public string[] SoundtrackFilter { get; private set; } = {};

        private readonly IList<ParserOption> _configuredOptions = new List<ParserOption>();

        private readonly OptionSet _optionSet = new OptionSet();

        private readonly LoggingLevelSwitch _loggingLevelSwitch;

        public ArgumentParser(LoggingLevelSwitch loggingLevelSwitch)
        {
            this._loggingLevelSwitch = loggingLevelSwitch;

            this.Add(ParserOption.Help, ParserOption.Verbose);
        }

        public void Add(ParserOption parserOption)
        {
            if (this._configuredOptions.Contains(parserOption))
            {
                return;
            }

            switch (parserOption)
            {
                // Simple options
                case ParserOption.Help:
                    this._optionSet.Add("help|version|?", "Show this message.", (value) => {
                        this.Help = true;
                    });
                    break;

                case ParserOption.OverwriteFiles:
                    this._optionSet.Add("overwrite", "Overwrite files if they already exist.", (value) => {
                        this.Overwrite = true;
                    });
                    break;

                case ParserOption.OverWriteAudio:
                    this._optionSet.Add("overwrite", "Overwrite tracks if they already exist.", (value) => {
                        this.Overwrite = true;
                    });
                    break;

                case ParserOption.Flac:
                    this._optionSet.Add(
                        "flac",
                        "Use FLAC format instead of original OGG for a (tiny) quality improvement.",
                        (value) => {
                            this.Flac = true;
                        }
                    );
                    break;

                case ParserOption.SoundtrackFilter:
                    this._optionSet.Add(
                        "filter=|f",
                        "Process only tracks containing any ofthe given comma-separated names. E.g., \"scape,dark\".",
                        (value) => {
                            this.SoundtrackFilter = value.Split(',');
                        }
                    );
                    break;

                case ParserOption.OutputDirectory:
                    this._optionSet.Add(
                        "output=",
                        "Extract files to this directory.",
                        (value) => this.OutputDirectory = value
                    );
                    break;

                // More complex options start here
                case ParserOption.Verbose:
                    // Applicationwide arguments
                    this._optionSet.Add("verbose|v", "Increase amount of log messages.", (value) =>
                    {
                        this._loggingLevelSwitch.MinimumLevel = LogEventLevel.Debug;
                        this.Verbose = true;
                    });
                    break;

                case ParserOption.Java:
                    this._optionSet.Add(
                        "java:",
                        "Obtain cache files from the Java client. Pass a directory to use a directory different from the default.",
                        (value) => this.SetSourceCache(new JavaClientCache(value))
                    );
                    break;

                case ParserOption.Download:
                    this._optionSet.Add(
                        "download",
                        "Obtain cache files directly from Jagex's servers.",
                        (value) => this.SetSourceCache(new DownloaderCache())
                    );
                    break;

                // case ParserOption.Nxt:
                //     this._optionSet.Add(
                //         "nxt:",
                //         "Obtain cache files from the NXT client. Pass a directory to use a directory different from the default.",
                //         (value) => this.SetSourceCache(new NxtClientCache(value))
                //     );
                //     break;

                case ParserOption.FileFilter:
                    this._optionSet.Add(
                        "filter=|f",
                        "Process only files matching the given pattern. E.g., \"40-41/*\" or \"*/1,10\".",
                        (value) => {
                            var parts = value.Split('/');
                            if (parts.Length < 1 || parts.Length > 2)
                            {
                                throw new ArgumentException("Invalid file filter format.");
                            }

                            var indexes = ArgumentParser.ExpandIntegerRangeString(parts[0]).Cast<CacheIndex>().ToList();
                            var files = (parts.Length == 2)
                                ? ArgumentParser.ExpandIntegerRangeString(parts[1]).ToList()
                                : new List<int>();

                            this.FileFilter = new Tuple<IList<CacheIndex>, IList<int>>(indexes, files);
                        }
                    );
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(parserOption), parserOption, null);
            }

            this._configuredOptions.Add(parserOption);
        }

        public void Add(params ParserOption[] parserOptions)
        {
            foreach (var parserOption in parserOptions)
            {
                this.Add(parserOption);
            }
        }

        public IList<string> ParseArguments(IEnumerable<string> arguments)
        {
            return this._optionSet.Parse(arguments);
        }

        public string GetDescription()
        {
            var buffer = new StringWriter();
            this._optionSet.WriteOptionDescriptions(buffer);
            return buffer.ToString();
        }

        private static IEnumerable<int> ExpandIntegerRangeString(string integerRangeString)
        {
            var rangeStringParts = integerRangeString.Split(',', ';');
            var result = new List<int>();

            foreach (var rangeStringPart in rangeStringParts)
            {
                if (rangeStringPart.Count(ch => ch == '-') == 1)
                {
                    // Expand the range
                    var rangeParts = rangeStringPart.Split('-');
                    var rangeStart = int.Parse(rangeParts[0]);
                    var rangeCount = int.Parse(rangeParts[1]) - rangeStart + 1;

                    result.AddRange(Enumerable.Range(rangeStart, rangeCount));
                }
                else
                {
                    // It should be a single integer
                    result.Add(int.Parse(rangeStringPart));
                }
            }

            // Filter duplicates
            return result.Distinct();
        }

        private void SetSourceCache(ICache<RuneTek5CacheFile> sourceCache)
        {
            if (this.SourceCache != null)
            {
                throw new ArgumentException("Source cache is already defined. Make sure to use only one source argument.");
            }

            this.SourceCache = sourceCache;
        }
    }
}
