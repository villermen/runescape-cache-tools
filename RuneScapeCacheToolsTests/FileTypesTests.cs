using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Villermen.RuneScapeCacheTools.Exceptions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace RuneScapeCacheToolsTests
{
    [Collection(TestCacheCollection.Name)]
    public class FileTypesTests
    {
        private TestCacheFixture Fixture { get; }

        private ITestOutputHelper Output { get; }

        public FileTypesTests(TestCacheFixture fixture, ITestOutputHelper output)
        {
            this.Fixture = fixture;
            this.Output = output;
        }

        [Fact]
        public void TestEntryFile()
        {
            var binaryFile = this.Fixture.RuneTek5Cache.GetFile<BinaryFile>(Index.ItemDefinitions, 155);
            var entryFile = new EntryFile();
            entryFile.FromBinaryFile(binaryFile);

            var binaryFile1 = entryFile.GetEntry<BinaryFile>(0);
            Assert.Equal(242, binaryFile1.Data.Length);

            var itemDefinitionFile = entryFile.GetEntry<ItemDefinitionFile>(0);
            Assert.Equal(2609, itemDefinitionFile.UnknownShort4);

            var itemDefinitionFiles = entryFile.GetEntries<ItemDefinitionFile>();
            Assert.Equal(256, itemDefinitionFiles.Length);
            Assert.Equal(2609, itemDefinitionFiles[0].UnknownShort4);

            Assert.True(entryFile.Encode().SequenceEqual(binaryFile.Data));
        }

        [Theory]
        [InlineData(38900, 53)]
        public void TestJagaFile(int fileId, int expectedNumberOfChunks)
        {
            var jagaFile = this.Fixture.RuneTek5Cache.GetFile<JagaFile>(Index.Music, fileId);

            Assert.Equal(expectedNumberOfChunks, jagaFile.ChunkCount);
        }

        [Theory]
        [InlineData(Index.ItemDefinitions, 155, 134, "Hazelmere's signet ring", 4)]
        [InlineData(Index.ItemDefinitions, 5, 241, "Oak logs", 12)]
        [InlineData(Index.ItemDefinitions, 155, 104, "Attuned crystal teleport seed", 14)]
        public void TestItemDefinitionFile(Index index, int fileId, int entryId, string expectedName, int expectedPropertyCount)
        {
            var itemDefinition = this.Fixture.RuneTek5Cache
                .GetFile<EntryFile>(index, fileId)
                .GetEntry<ItemDefinitionFile>(entryId);

            Assert.Equal(expectedName, itemDefinition.Name);
            Assert.Equal(expectedPropertyCount, itemDefinition.Properties.Count);
        }

        [Fact(
            Skip = "Needs to be integrated in library"
        )]
        public void TestCreateItemCsv()
        {
            var headers = new List<string>();

            using (var tempWriter = new StreamWriter(File.Open("items.csv.tmp", FileMode.Create)))
            {
                foreach (var fileId in this.Fixture.Downloader.GetFileIds(Index.ItemDefinitions))
                {
                    try
                    {
                        var entryFile = this.Fixture.Downloader.GetFile<EntryFile>(Index.ItemDefinitions, fileId);

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