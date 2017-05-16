using System.Collections.Generic;
using System.IO;
using System.Linq;
using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Villermen.RuneScapeCacheTools.Exceptions;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Tests.Tests
{
    [Collection(TestCacheCollection.Name)]
    public class TemporaryTests
    {
        private TestCacheFixture Fixture { get; }

        public TemporaryTests(TestCacheFixture fixture)
        {
            this.Fixture = fixture;
        }

        [Fact(
            Skip = "Needs to be integrated in library"
        )]
        public void TestCreateItemCsv()
        {
            var headers = new List<string>();

            using (var tempWriter = new StreamWriter(File.Open("items.csv.tmp", FileMode.Create)))
            {
                foreach (var fileId in this.Fixture.DownloaderCache.GetFileIds(Index.ItemDefinitions))
                {
                    try
                    {
                        var entryFile = this.Fixture.DownloaderCache.GetFile<EntryFile>(Index.ItemDefinitions, fileId);

                        var itemDefinitionFiles = entryFile.GetEntries<ItemDefinitionFile>();
                        foreach (var itemDefinitionFile in itemDefinitionFiles)
                        {
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
                    catch (DecodeException exception)
                    {
                    }
                }
            }

            // Prepend headers
            using (var csvWriter = new StreamWriter(File.OpenWrite("items.csv")))
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

                foreach (var line in File.ReadLines("items.csv.tmp"))
                {
                    csvWriter.WriteLine(line);
                }
            }
        }
    }
}