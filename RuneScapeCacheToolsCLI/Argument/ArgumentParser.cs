using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NDesk.Options;
using Villermen.RuneScapeCacheTools.Cache.Downloader;
using Villermen.RuneScapeCacheTools.Cache.JavaClient;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.CLI.Argument
{
    public class ArgumentParser
    {
        public bool Overwrite { get; private set; }

        public bool Flac { get; private set; }

        public RuneTek5Cache? SourceCache { get; private set; }

        public string OutputDirectory { get; private set; } = "files";

        public Tuple<CacheIndex[], int[]> FileFilter { get; private set; } = new Tuple<CacheIndex[], int[]>(
            new CacheIndex[0],
            new int[0]
        );

        public string[] SoundtrackFilter { get; private set; } = {};

        public IEnumerable<string> PositionalArgumentNames => this._positionalArguments.Select(argument => argument.Item1);

        private readonly IList<CommonArgument> _commonArguments = new List<CommonArgument>();

        private readonly OptionSet _optionSet = new OptionSet();

        private readonly IList<Tuple<string, string, Action<string>>> _positionalArguments = new List<Tuple<string, string, Action<string>>>();

        public void Add(string prototype, string description, Action<string> action)
        {
            this._optionSet.Add(prototype, description, action);
        }

        public void AddCommon(CommonArgument commonArgument)
        {
            if (this._commonArguments.Contains(commonArgument))
            {
                return;
            }

            switch (commonArgument)
            {
                case CommonArgument.Overwrite:
                    this.Add("overwrite", "Overwrite files if they already exist.", (value) => {
                        this.Overwrite = true;
                    });
                    break;

                case CommonArgument.Flac:
                    this.Add(
                        "flac",
                        "Use FLAC format instead of original OGG for a (tiny) quality improvement.",
                        (value) => {
                            this.Flac = true;
                        }
                    );
                    break;

                case CommonArgument.SoundtrackFilter:
                    this.Add(
                        "filter=|f",
                        "Process only tracks containing any ofthe given comma-separated names. E.g., \"scape,dark\".",
                        (value) => {
                            this.SoundtrackFilter = value.Split(',');
                        }
                    );
                    break;

                case CommonArgument.OutputDirectory:
                    this.Add(
                        "output=",
                        "Extract files to this directory.",
                        (value) => this.OutputDirectory = value
                    );
                    break;

                case CommonArgument.SourceJava:
                    this.Add(
                        "java:",
                        "Obtain cache files from the Java client. Pass a directory to use a directory different from the default.",
                        (value) => this.SetSourceCache(new JavaClientCache(value))
                    );
                    break;

                case CommonArgument.SourceDownload:
                    this.Add(
                        "download",
                        "Obtain cache files directly from Jagex's servers.",
                        (value) => this.SetSourceCache(new DownloaderCache())
                    );
                    break;

                // case ParserOption.Nxt:
                //     this.Add(
                //         "nxt:",
                //         "Obtain cache files from the NXT client. Pass a directory to use a directory different from the default.",
                //         (value) => this.SetSourceCache(new NxtClientCache(value))
                //     );
                //     break;

                case CommonArgument.FileFilter:
                    this.Add(
                        "filter=|f",
                        "Process only files matching the given pattern. E.g., \"40-42/\" for all files in indexes 40 through 42 or \"5/1,10\" for files 1 and 10 from index 5.",
                        (value) => this.FileFilter = ArgumentParser.ParseFileFilter(value)
                    );
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(commonArgument), commonArgument, null);
            }

            this._commonArguments.Add(commonArgument);
        }

        public void AddPositional(string name, string description, Action<string> action)
        {
            this._positionalArguments.Add(new Tuple<string, string, Action<string>>(name, description, action));
        }

        public IList<string> ParseArguments(IEnumerable<string> arguments)
        {
            var unparsedArguments1 = this._optionSet.Parse(arguments);
            var unparsedArguments2 = new List<string>();

            // Parse positional arguments.
            var positionalArgumentIndex = 0;
            foreach (var unparsedArgument in unparsedArguments1)
            {
                if (unparsedArgument.StartsWith("-"))
                {
                    unparsedArguments2.Add(unparsedArgument);
                    continue;
                }

                this._positionalArguments[positionalArgumentIndex++].Item3(unparsedArgument);
            }

            if (positionalArgumentIndex < this._positionalArguments.Count - 1)
            {
                throw new ArgumentException($"Missing required positionial argument \"{this._positionalArguments[positionalArgumentIndex].Item1}\".");
            }

            return unparsedArguments2;
        }

        public string GetDescription()
        {
            var buffer = new StringWriter();
            if (this._positionalArguments.Any())
            {
                foreach (var positionalArgument in this._positionalArguments)
                {
                    buffer.WriteLine($"      {positionalArgument.Item1.PadRight(23)}{positionalArgument.Item2}");
                    // TODO: Line splitting?
                }
                buffer.WriteLine();
            }

            this._optionSet.WriteOptionDescriptions(buffer);
            return buffer.ToString();
        }

        private static Tuple<CacheIndex[], int[]> ParseFileFilter(string fileFilter)
        {
            var parts = fileFilter.Split('/');
            if (parts.Length < 1 || parts.Length > 2)
            {
                throw new ArgumentException("Invalid file filter format.");
            }

            var indexes = ArgumentParser.ExpandIntegerRangeString(parts[0]).Cast<CacheIndex>().ToArray();
            var files = (parts.Length == 2)
                ? ArgumentParser.ExpandIntegerRangeString(parts[1]).ToArray()
                : new int[0];

            return new Tuple<CacheIndex[], int[]>(indexes, files);
        }

        private static IEnumerable<int> ExpandIntegerRangeString(string integerRangeString)
        {
            var rangeStringParts = integerRangeString.Split(',', ';');
            var result = new List<int>();

            foreach (var rangeStringPart in rangeStringParts)
            {
                if (rangeStringPart.Count(ch => ch == '-') == 1)
                {
                    // Expand the range.
                    var rangeParts = rangeStringPart.Split('-');
                    var rangeStart = int.Parse(rangeParts[0]);
                    var rangeCount = int.Parse(rangeParts[1]) - rangeStart + 1;

                    result.AddRange(Enumerable.Range(rangeStart, rangeCount));
                }
                else
                {
                    if (int.TryParse(rangeStringPart, out var parsedInteger))
                    {
                        result.Add(parsedInteger);
                    }

                    // If it's not a single integer assume it's a wildcard (empty or asterisk) and don't add anything.
                    // This would be weird in combination with other ranges (3,* would result in just 3), but why would
                    // anyone do that anyway?
                }
            }

            // Filter duplicates.
            return result.Distinct();
        }

        private void SetSourceCache(RuneTek5Cache sourceCache)
        {
            if (this.SourceCache != null)
            {
                throw new ArgumentException("Source cache is already defined. Make sure to use only one source argument.");
            }

            this.SourceCache = sourceCache;
        }
    }
}
