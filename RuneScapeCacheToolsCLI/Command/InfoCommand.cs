using System;
using System.Linq;
using System.Text;
using Villermen.RuneScapeCacheTools.CLI.Argument;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class InfoCommand : BaseCommand
    {
        private Tuple<CacheIndex[], int[]>? _fileFilter;

        public InfoCommand(ArgumentParser argumentParser) : base(argumentParser)
        {
            this.ArgumentParser.AddCommon(CommonArgument.SourceCache);
            this.ArgumentParser.AddCommon(CommonArgument.FileFilter);
            this.ArgumentParser.AddCommon(CommonArgument.Overwrite);
            this.ArgumentParser.AddCommon(CommonArgument.OutputDirectory);
            this.ArgumentParser.AddPositional("index", "The index/file to list information of, like \"15\" or \"15/12\".", (value) => this._fileFilter = ArgumentParser.ParseFileFilter(value));
        }

        public override int Run()
        {
            using var sourceCache = this.ArgumentParser.SourceCache;
            if (sourceCache == null)
            {
                Console.WriteLine("No cache source specified.");
                return 2;
            }
            if (this._fileFilter == null || this._fileFilter.Item1.Length == 0)
            {
                Console.WriteLine("No index/file specified.");
                return 2;
            }
            if (this._fileFilter.Item1.Length > 1 || this._fileFilter.Item2.Length > 1)
            {
                Console.WriteLine("Multiple indexes/files specified.");
                return 2;
            }

            var index = this._fileFilter.Item1[0];
            var fileId = this._fileFilter.Item2.Length > 0 ? this._fileFilter.Item2[0] : (int?)null;

            Console.WriteLine($"Retrieving info for index {(int)index}{(fileId != null ? $" file {fileId}" : "")}...");

            // Index information
            if (fileId == null)
            {
                var indexName = Enum.GetName(typeof(CacheIndex), index) ?? "IHaveNoIdea";
                Console.WriteLine($"Contents: {indexName} (probably)");

                var referenceTable = sourceCache.GetReferenceTable(index);

                var versionAsTime = "";
                if (referenceTable.Version != null)
                {
                    var formattedTime = DateTimeOffset.FromUnixTimeSeconds(referenceTable.Version.Value).ToString("u");
                    versionAsTime = $" ({formattedTime})";
                }
                Console.WriteLine($"Files: {referenceTable.FileIds.Count():N0}");
                Console.WriteLine($"Format: {referenceTable.Format}");
                Console.WriteLine($"Version: {referenceTable.Version}{versionAsTime}");
                Console.WriteLine($"Options: {referenceTable.Options}");

                if (referenceTable.FileIds.Any())
                {
                    var firstFileIds = referenceTable.FileIds.Take(Math.Min(referenceTable.FileIds.Count(), 5));
                    var lastFileIds = referenceTable.FileIds.Reverse().Take(Math.Min(referenceTable.FileIds.Count(), 5)).Reverse();
                    Console.WriteLine($"First files: {string.Join(", ", firstFileIds)}");
                    Console.WriteLine($"Last files: {string.Join(", ", lastFileIds)}");
                }

                return 0;
            }

            // File information
            var file = sourceCache.GetFile(index, fileId.Value);

            Console.WriteLine($"Size: {file.Data.Length:N0}");

            Console.WriteLine($"Compression type: {file.Info.CompressionType}");
            if (file.Info.CompressedSize != null)
            {
                Console.WriteLine($"Compressed size: {file.Info.CompressedSize:N0}");
            }

            if (file.Info.Version != null)
            {
                Console.WriteLine($"Version: {file.Info.Version}");
            }
            if (file.Info.Crc != null)
            {
                Console.WriteLine($"CRC: {file.Info.Crc}");
            }
            if (file.Info.HasEntries)
            {
                Console.WriteLine($"Entries: {file.Info.Entries.Count:N0}");
            }
            if (file.Info.Identifier != null)
            {
                Console.WriteLine($"Identifier: {file.Info.Identifier}");
            }
            if (file.Info.MysteryHash != null)
            {
                Console.WriteLine($"Mystery hash: {file.Info.MysteryHash}");
            }
            if (file.Info.WhirlpoolDigest != null)
            {
                Console.WriteLine($"Whirlpool: {Formatter.BytesToHexString(file.Info.WhirlpoolDigest)}");
            }
            if (file.Info.EncryptionKey != null)
            {
                Console.WriteLine($"Identifier: {Formatter.BytesToHexString(file.Info.EncryptionKey)}");
            }

            if (file.Data.Length < 10)
            {
                var bytes = file.Data;
                Console.WriteLine($"Bytes: {Formatter.BytesToHexString(bytes)} ({Encoding.ASCII.GetString(bytes)})");
            }
            else
            {
                var firstBytes = file.Data.Take(10).ToArray();
                var lastBytes = file.Data.Reverse().Take(10).Reverse().ToArray();
                Console.WriteLine($"First 10 bytes: {Formatter.BytesToHexString(firstBytes)} ({Formatter.BytesToAnsiString(firstBytes)})");
                Console.WriteLine($"Last 10 bytes: {Formatter.BytesToHexString(lastBytes)} ({Formatter.BytesToAnsiString(lastBytes)})");
            }

            // TODO: Dive further into separate entries (like first and last bytes and size)?

            return 0;
        }
    }
}
