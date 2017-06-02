using System.Collections.Generic;
using System.IO;
using System.Linq;
using NDesk.Options;
using Villermen.RuneScapeCacheTools.Cache;

namespace Villermen.RuneScapeCacheTools.CLI
{
    public class ArgumentParser
    {
        public string CacheDirectory { get; set; }
        public string OutputDirectory { get; set; }
        public string TemporaryDirectory { get; set; }
        public int Actions { get; set; }
        public IEnumerable<Index> Indexes { get; set; }
        public IEnumerable<int> FileIds { get; set; }
        public bool DoExtract { get; set; }
        public bool Overwrite { get; set; }
        public bool Lossless { get; set; }
        public bool DoSoundtrackCombine { get; set; }
        public IEnumerable<string> SoundtrackNameFilter { get; set; }
        public bool Download { get; set; }
        public bool ShowHelp { get; set; }
        public bool IncludeUnnamedSoundtracks { get; set; }
        public List<string> UnparsedArguments { get; set; }
        public bool TooManyActions { get; set; }
        public bool NoActions { get; set; }
        public bool CanExecute { get; set; }

        private OptionSet _optionSet;

        public void Parse(string[] arguments)
        {
            this._optionSet = new OptionSet
            {
                {
                    "cache-directory=|c",
                    "The directory in which RuneScape's cache files are located. If unspecified, the default directory will be attempted.",
                    value =>
                    {
                        this.CacheDirectory = value;
                    }
                },
                {
                    "download|d",
                    "Download all requested files straight from Jagex's servers instead of using a local cache.",
                    value =>
                    {
                        this.Download = value != null;
                    }
                },
                {
                    "output-directory=|o",
                    "The directory in which output files (mainly extracted files) will be stored.",
                    value =>
                    {
                        this.OutputDirectory = value;
                    }
                },
                {
                    "temporary-directory=|t",
                    "The directory in which temporary files will be stored. If unspecified, the system's default directory + \"rsct\" will be used.",
                    value =>
                    {
                        this.TemporaryDirectory = value;
                    }
                },
                {
                    "extract|e",
                    "Extract all files from the cache. You can specify which files to extract by using the index and file arguments.",
                    value =>
                    {
                        this.DoExtract = value != null;
                        this.Actions++;
                    }
                },
                {
                    "index=|i",
                    "A index id or range of index ids to extract. E.g. \"1-2,4,6-7\".",
                    value =>
                    {
                        this.Indexes = ArgumentParser.ExpandIntegerRangeString(value).Cast<Index>();
                    }
                },
                {
                    "file=|f",
                    "A file id or range of file ids to extract. E.g. \"1-2,4,6-7\".",
                    value =>
                    {
                        this.FileIds = ArgumentParser.ExpandIntegerRangeString(value);
                    }
                },
                {
                    "overwrite",
                    "Overwrite extracted files if they already exist.",
                    value =>
                    {
                        this.Overwrite = value != null;
                    }
                },
                {
                    "soundtrack-combine:|s",
                    "Extract and name the soundtrack, optionally filtered by the given comma-separated name filters.",
                    value =>
                    {
                        this.DoSoundtrackCombine = true;

                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            this.SoundtrackNameFilter = ArgumentParser.ExpandListString(value);
                        }

                        this.Actions++;
                    }
                },
                {
                    "unnamed-tracks",
                    "Also include soundtracks with an invalid or non-existent name." ,
                    value =>
                    {
                        this.IncludeUnnamedSoundtracks = value != null;
                    }
                },
                {
                    "lossless",
                    "Use FLAC instead of OGG as audio format when combining, preventing quality loss.",
                    value =>
                    {
                        this.Lossless = value != null;
                    }
                },
                {
                    "help|h|?",
                    "Show this message.",
                    value =>
                    {
                        this.ShowHelp = true;
                    }
                }
            };

            this.UnparsedArguments = this._optionSet.Parse(arguments);

            if (arguments.Length == 0)
            {
                this.ShowHelp = true;
            }

            this.TooManyActions = this.Actions > 1 && !this.ShowHelp;

            this.NoActions = this.Actions == 0 && !this.ShowHelp && this.UnparsedArguments.Count == 0;

            this.CanExecute = !this.ShowHelp &&
                !this.TooManyActions &&
                !this.NoActions &&
                this.UnparsedArguments.Count == 0;
        }

        public void WriteOptionDescriptions(TextWriter writer)
        {
            this._optionSet.WriteOptionDescriptions(writer);
        }

        /// <summary>
        /// Expands the given integer range into an enumerable of all individual integers.
        /// </summary>
        /// <param name="integerRangeString">An integer range, e.g. "0-4,6,34,200-201"</param>
        /// <returns></returns>
        private static IEnumerable<int> ExpandIntegerRangeString(string integerRangeString)
        {
            var rangeStringParts = ArgumentParser.ExpandListString(integerRangeString);
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

        private static IEnumerable<string> ExpandListString(string listString)
        {
            return listString.Split(',', ';');
        }
    }
}
