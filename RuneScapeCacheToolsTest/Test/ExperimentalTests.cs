using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Test.Fixture;
using Villermen.RuneScapeCacheTools.Utility;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Test
{
    [Collection(TestCacheCollection.Name)]
    public class ExperimentalTests : BaseTests
    {
        private TestCacheFixture Fixture { get; }

        public ExperimentalTests(TestCacheFixture fixture)
        {
            this.Fixture = fixture;
        }

        [Fact(
            Skip = "Needs to be integrated in library"
        )]
        public void TestCreateItemCsv()
        {
            var headers = new List<string>();

            using (var tempWriter = new StreamWriter(System.IO.File.Open("items.csv.tmp", FileMode.Create)))
            {
                foreach (var fileId in this.Fixture.DownloaderCache.GetAvailableFileIds(CacheIndex.ItemDefinitions))
                {
                    try
                    {
                        var entryFile = EntryFile.DecodeFromCacheFile(this.Fixture.DownloaderCache.GetFile(CacheIndex.ItemDefinitions, fileId));

                        foreach (var entry in entryFile.Entries.Values)
                        {
                            var itemDefinitionFile = ItemDefinitionFile.Decode(entry);

                            var row = new Dictionary<int, string>();

                            foreach (var field in itemDefinitionFile.GetFields())
                            {
                                if (!headers.Contains(field.Key))
                                {
                                    headers.Add(field.Key);
                                }

                                row.Add(headers.IndexOf(field.Key), field.Value);
                            }

                            var lastIndex = row.Keys.Max();
                            for (var rowIndex = 0; rowIndex < lastIndex; rowIndex++)
                            {
                                if (rowIndex > 0)
                                {
                                    tempWriter.Write(",");
                                }

                                if (row.ContainsKey(rowIndex))
                                {
                                    tempWriter.Write($"\"{row[rowIndex]?.Replace("\"", "\"\"")}\"");
                                }
                            }

                            tempWriter.WriteLine();
                        }
                    }
                    catch (DecodeException)
                    {
                    }
                }
            }

            // Prepend headers
            using (var csvWriter = new StreamWriter(System.IO.File.OpenWrite("items.csv")))
            {
                var headerCount = headers.Count;
                for (var headerIndex = 0; headerIndex < headerCount; headerIndex++)
                {
                    if (headerIndex > 0)
                    {
                        csvWriter.Write(",");
                    }

                    csvWriter.Write($"\"{headers[headerIndex]}\"");
                }

                csvWriter.WriteLine();

                foreach (var line in System.IO.File.ReadLines("items.csv.tmp"))
                {
                    csvWriter.WriteLine(line);
                }
            }
        }

        [Fact]
        public void TextNxt()
        {
            using var cache = new DownloaderCache();
            var whatKindaDataAreYou = cache.GetFile(CacheIndex.Enums, 5);

            System.IO.File.WriteAllBytes("17-5.javaentries", whatKindaDataAreYou.Data);

            // Entries seem to be different for NXT.
            var entryFile = EntryFile.DecodeFromCacheFile(whatKindaDataAreYou);
            var enumFile = EnumFile.Decode(entryFile.Entries[65]);
        }
    }
}
