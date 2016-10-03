using System;
using System.Linq;
using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Xunit;
using Xunit.Abstractions;

namespace RuneScapeCacheToolsTests
{
    [Collection("TestCache")]
    public class DownloaderTests : IDisposable
    {
        private ITestOutputHelper Output { get; }
        private CacheFixture Fixture { get; }

        public DownloaderTests(ITestOutputHelper output, CacheFixture fixture)
        {
            Output = output;
            Fixture = fixture;
        }

        [Fact]
        public void TestGetFileVersusDownloadFileAsync()
        {
            var file1 = Fixture.Downloader.GetFile(12, 3);
            var file2 = Fixture.Downloader.DownloadFileAsync(12, 3).Result;

            Assert.True(file1.Data.Length == file2.Data.Length, "Two of the downloaded files with the same id did not have the same size.");
        }

        [Fact]
        public void TestGetFileWithEntries()
        {
            var archiveFile = Fixture.Downloader.GetFile(17, 5);

            Assert.True(archiveFile.Entries.Length == 256, $"File 5 in archive 17 has {archiveFile.Entries.Length} entries instead of the expected 256.");
        }

        [Fact]
        public void TestDownloadReferenceTable()
        {
            var referenceTable12 = Fixture.Downloader.DownloadReferenceTable(12);
            var referenceTable40 = Fixture.Downloader.DownloadReferenceTable(40);

            var rawReferenceTable = Fixture.Downloader.GetFile(RuneTek5Cache.MetadataIndexId, 17);

            var referenceTable17 = Fixture.Downloader.DownloadReferenceTable(17);

            Output.WriteLine($"Files in reference table for index 17: {referenceTable17.Files.Count}.");

            Assert.True(referenceTable17.Files.Count == 46, $"Reference table for index 17 reported having {referenceTable17.Files.Count} files instead of the expected 46.");
        }

        [Fact]
        public void TestDownloadMasterReferenceTable()
        {
            var masterReferenceTable = Fixture.Downloader.DownloadMasterReferenceTable();

            Assert.True(masterReferenceTable.ReferenceTableFiles.Count == 50, $"Master reference table reported having {masterReferenceTable.ReferenceTableFiles.Count} files instead of the expected 50.");
        }

        [Theory]
        [InlineData(17, 46)]
        public void TestGetFileIds(int indexId, int expectedFileCount)
        {
            var reportedFileCount = Fixture.Downloader.GetFileIds(indexId).Count();

            Assert.True(reportedFileCount == expectedFileCount, $"Downloader reported {reportedFileCount} files in index {indexId}, {expectedFileCount} expected.");
        }

        [Theory]
        [InlineData(50)]
        public void TestIndexIds(int expectedIndexCount)
        {
            var reportedIndexCount = Fixture.Downloader.IndexIds.Count();

            Assert.True(reportedIndexCount == expectedIndexCount, $"Downloader reported {reportedIndexCount} indexes, {expectedIndexCount} expected.");
        }

        [Fact]
        public void TestHttpInterface()
        {
            var httpFile = Fixture.Downloader.GetFile(40, 30498);
        }

        public void Dispose()
        {
        }
    }
}
