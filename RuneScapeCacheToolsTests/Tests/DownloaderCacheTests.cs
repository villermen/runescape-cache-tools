using System.Linq;
using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Tests.Tests
{
    [Collection(TestCacheCollection.Name)]
    public class DownloaderCacheTests
    {
        private TestCacheFixture Fixture { get; }

        public DownloaderCacheTests(TestCacheFixture fixture)
        {
            this.Fixture = fixture;
        }

        [Fact]
        public void TestGetFileWithEntries()
        {
            var archiveFile = this.Fixture.Downloader.GetFile<EntryFile>(Index.Enums, 5);

            Assert.Equal(256, archiveFile.EntryCount);
        }

        [Fact]
        public void TestDownloadReferenceTable()
        {
            this.Fixture.Downloader.GetReferenceTable(Index.ClientScripts);
            this.Fixture.Downloader.GetReferenceTable(Index.Music);
            this.Fixture.Downloader.GetFile<BinaryFile>(Index.ReferenceTables, (int)Index.Enums);

            var referenceTable17 = this.Fixture.Downloader.GetReferenceTable(Index.Enums);

            Assert.InRange(referenceTable17.FileIds.Length, 48, 1000);
        }

        [Theory]
        [InlineData(52)]
        public void TestDownloadMasterReferenceTable(int expectedTableCount)
        {
            var masterReferenceTable = this.Fixture.Downloader.GetMasterReferenceTable();

            Assert.InRange(masterReferenceTable.ReferenceTableFiles.Count, expectedTableCount, 253);
        }

        [Theory]
        [InlineData(Index.Enums, 47)]
        public void TestGetFileIds(Index index, int expectedFileCount)
        {
            var reportedFileCount = this.Fixture.Downloader.GetFileIds(index).Count();

            Assert.InRange(reportedFileCount, expectedFileCount, 1000);
        }

        [Theory]
        [InlineData(52)]
        public void TestIndexIds(int expectedIndexCount)
        {
            var reportedIndexCount = this.Fixture.Downloader.GetIndexes().Count();

            Assert.InRange(reportedIndexCount, expectedIndexCount, 253);
        }

        [Fact]
        public void TestHttpInterface()
        {
            var httpFile = this.Fixture.Downloader.GetFile<BinaryFile>(Index.Music, 30498);

            Assert.True(httpFile.Data.Length > 0);
        }

        [Fact]
        public void TestReferenceTableCaching()
        {
            var referenceTable1 = this.Fixture.Downloader.GetReferenceTable(Index.Enums);
            var referenceTable2 = this.Fixture.Downloader.GetReferenceTable(Index.Enums);

            Assert.Same(referenceTable1, referenceTable2);
        }

        [Fact]
        public void TestMasterReferenceTableCaching()
        {
            var masterReferenceTable1 = this.Fixture.Downloader.GetMasterReferenceTable();
            var masterReferenceTable2 = this.Fixture.Downloader.GetMasterReferenceTable();

            Assert.Same(masterReferenceTable1, masterReferenceTable2);
        }
    }
}
