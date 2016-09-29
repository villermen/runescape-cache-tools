using System;
using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Download;
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

        [Fact(Skip = "Not implemented.")]
        public void TestDownloadFile()
        {
            Fixture.Downloader.DownloadFile(12, 3); // TODO: Error in reference table decoding, see TestDownloadReferenceTable

            // TODO: HTTP worker or something similar for
            // Fixture.Downloader.DownloadFile(40, 30468);

            // TODO: Verify result
        }

        [Fact(Skip = "Not implemented.")]
        public void TestDownloadFileEntries()
        {
            var archiveFile = Fixture.Downloader.DownloadFile(17, 5);

            Assert.True(archiveFile.Entries.Length == 256, $"File 5 in archive 17 has {archiveFile.Entries.Length} entries instead of the expected 256.");
        }

        [Fact]
        public void TestDownloadReferenceTable()
        {
            // TODO: There are some issues with these ones, investigate
            // var referenceTable12 = Fixture.Downloader.DownloadReferenceTable(12); // Trying to add item with same key on file entry
            // var referenceTable40 = Fixture.Downloader.DownloadReferenceTable(40); // Not being able to decode bzip stream

            var rawReferenceTable = Fixture.Downloader.DownloadFile(RuneTek5Cache.MetadataIndexId, 17);

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

        public void Dispose()
        {
        }
    }
}
