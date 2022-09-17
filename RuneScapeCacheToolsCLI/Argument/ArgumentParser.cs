using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NDesk.Options;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.CLI.Argument
{
    public class ArgumentParser
    {
        public ReferenceTableCache? Cache { get; private set; }

        public Tuple<CacheIndex[], int[]>? FileFilter { get; private set; }

        public string[] UnparsedArguments { get; private set; } = new string[0];

        public IEnumerable<string> PositionalArgumentNames => this._positionalArguments.Select(argument => argument.Item1);

        public string? Directory { get; private set; }

        private readonly IList<CommonArgument> _commonArguments = new List<CommonArgument>();

        private readonly OptionSet _optionSet = new OptionSet();

        private readonly IList<Tuple<string, string, Action<string>>> _positionalArguments = new List<Tuple<string, string, Action<string>>>();

        public bool Preserve { get; private set; } = false;

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
                case CommonArgument.Directory:
                    this.Add(
                        "directory=|dir=|output=",
                        "Write to or read from this directory.",
                        (value) => this.Directory = value
                    );
                    break;

                case CommonArgument.Cache:
                    this.Add(
                        "java:",
                        "Use cache files of the Java client. Pass a directory to use a directory different from the default.",
                        (value) => this.SetSourceCache(new JavaClientCache(value))
                    );
                    this.Add(
                        "nxt:",
                        "Use cache files of the NXT client. Pass a directory to use a directory different from the default.",
                        (value) => this.SetSourceCache(new NxtClientCache(value))
                    );
                    this.Add(
                        "download",
                        "Obtain cache files directly from Jagex's servers.",
                        (value) => this.SetSourceCache(new DownloaderCache())
                    );
                    break;

                case CommonArgument.Files:
                    this.AddPositional(
                        "files",
                        "Index(es)/file(s) to process. E.g., \"15\", \"15/12\" or \"15,40/1-100\".",
                        (value) => this.FileFilter = ArgumentParser.ParseFileFilter(value)
                    );
                    break;

                case CommonArgument.Preserve:
                    this.Add(
                        "preserve",
                        "Preserve existing files.",
                        (value) => { this.Preserve = true; }
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

        public void ParseArguments(IEnumerable<string> arguments)
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

            this.UnparsedArguments = unparsedArguments2.ToArray();
        }

        public string GetDescription()
        {
            var buffer = new StringWriter();
            if (this._positionalArguments.Any())
            {
                buffer.WriteLine("Arguments:");
                var splitRegex = new Regex(@"^(.{0,50})(?:[ $](.{1,48}))*$");
                foreach (var positionalArgument in this._positionalArguments)
                {
                    var match = splitRegex.Match(positionalArgument.Item2);
                    if (!match.Success)
                    {
                        throw new System.Exception("Please inform me that I don't know how to do regex.");
                    }

                    var splitDescription = match.Groups[1].Value;
                    foreach (var capture in match.Groups[2].Captures)
                    {
                        splitDescription += $"\n                               {capture}";
                    }

                    buffer.WriteLine($"      {positionalArgument.Item1.PadRight(23)}{splitDescription}");
                }
            }

            if (this._optionSet.Any())
            {
                buffer.WriteLine("Options:");
                this._optionSet.WriteOptionDescriptions(buffer);
            }

            return buffer.ToString();
        }

        public static Tuple<CacheIndex[], int[]> ParseFileFilter(string fileFilter)
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

        public static IEnumerable<int> ExpandIntegerRangeString(string integerRangeString)
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

        private void SetSourceCache(ReferenceTableCache sourceCache)
        {
            if (this.Cache != null)
            {
                throw new ArgumentException("Source cache is already defined. Make sure to use only one source argument.");
            }

            this.Cache = sourceCache;
        }
    }
}
