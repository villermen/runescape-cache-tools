using System;
using System.Linq;
using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache;
using Xunit;
using Xunit.Abstractions;

namespace RuneScapeCacheToolsTests
{
    [Collection("TestCache")]
    public class CacheDownloaderTests : IDisposable
    {
        private ITestOutputHelper Output { get; }
        private CacheFixture Fixture { get; }

        public CacheDownloaderTests(ITestOutputHelper output, CacheFixture fixture)
        {
            Output = output;
            Fixture = fixture;
        }

        [Fact]
        public void TestGetFileVersusDownloadFileAsync()
        {
            var file1 = Fixture.Downloader.GetFile(Index.ClientScripts, 3);
            var file2 = Fixture.Downloader.GetFile(Index.ClientScripts, 3);

            Assert.True(file1.Data.Length == file2.Data.Length, "Two of the downloaded files with the same id did not have the same size.");
        }

        [Fact]
        public void TestGetFileWithEntries()
        {
            var archiveFile = Fixture.Downloader.GetFile(Index.Enums, 5);

            Assert.True(archiveFile.Entries.Length == 256, $"File 5 in archive 17 has {archiveFile.Entries.Length} entries instead of the expected 256.");
        }

        [Fact]
        public void TestDownloadReferenceTable()
        {
            var referenceTable12 = Fixture.Downloader.GetReferenceTable(Index.ClientScripts);
            var referenceTable40 = Fixture.Downloader.GetReferenceTable(Index.Music);

            var rawReferenceTable = Fixture.Downloader.GetFile(Index.ReferenceTables, (int)Index.Enums);

            var referenceTable17 = Fixture.Downloader.GetReferenceTable(Index.Enums);

            Output.WriteLine($"Files in reference table for index 17: {referenceTable17.Files.Count}.");

            Assert.True(referenceTable17.Files.Count == 47, $"Reference table for index 17 reported having {referenceTable17.Files.Count} files instead of the expected 46.");
        }

        [Theory]
        [InlineData(52)]
        public void TestDownloadMasterReferenceTable(int expectedTableCount)
        {
            var masterReferenceTable = Fixture.Downloader.GetMasterReferenceTable();

            Assert.True(masterReferenceTable.ReferenceTableFiles.Count == expectedTableCount, $"Master reference table reported having {masterReferenceTable.ReferenceTableFiles.Count} files instead of the expected {expectedTableCount}.");
        }

        [Theory]
        [InlineData(Index.Enums, 47)]
        public void TestGetFileIds(Index index, int expectedFileCount)
        {
            var reportedFileCount = Fixture.Downloader.GetFileIds(index).Count();

            Assert.True(reportedFileCount == expectedFileCount, $"Downloader reported {reportedFileCount} files in index {index}, {expectedFileCount} expected.");
        }

        [Theory]
        [InlineData(52)]
        public void TestIndexIds(int expectedIndexCount)
        {
            var reportedIndexCount = Fixture.Downloader.Indexes.Count();

            Assert.True(reportedIndexCount == expectedIndexCount, $"Downloader reported {reportedIndexCount} indexes, {expectedIndexCount} expected.");
        }

        [Fact]
        public void TestHttpInterface()
        {
            var httpFile = Fixture.Downloader.GetFile(Index.Music, 30498);
        }

        [Fact]
        public void TestReferenceTableCaching()
        {
            var referenceTable1 = Fixture.Downloader.GetReferenceTable(Index.Enums);
            var referenceTable2 = Fixture.Downloader.GetReferenceTable(Index.Enums);

            Assert.Same(referenceTable1, referenceTable2);
        }

        [Fact]
        public void TestMasterReferenceTableCaching()
        {
            var masterReferenceTable1 = Fixture.Downloader.GetMasterReferenceTable();
            var masterReferenceTable2 = Fixture.Downloader.GetMasterReferenceTable();

            Assert.Same(masterReferenceTable1, masterReferenceTable2);
        }

        public void Dispose()
        {
        }
    }
}
