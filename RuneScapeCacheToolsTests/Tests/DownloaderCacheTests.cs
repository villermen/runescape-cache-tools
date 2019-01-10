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
            var archiveFile = this.Fixture.DownloaderCache.GetFile<EntryFile>(Index.Enums, 5);

            Assert.Equal(249, archiveFile.EntryCount);
        }

        [Fact]
        public void TestDownloadReferenceTable()
        {
            this.Fixture.DownloaderCache.GetReferenceTable(Index.ClientScripts);
            this.Fixture.DownloaderCache.GetReferenceTable(Index.Music);
            this.Fixture.DownloaderCache.GetFile<BinaryFile>(Index.ReferenceTables, (int)Index.Enums);

            var referenceTable17 = this.Fixture.DownloaderCache.GetReferenceTable(Index.Enums);

            Assert.InRange(referenceTable17.FileIds.Length, 48, 1000);
        }

        [Theory]
        [InlineData(57, 42)]
        public void TestDownloadMasterReferenceTable(int expectedTableCount, int expectedAvailableTableCount)
        {
            var masterReferenceTable = this.Fixture.DownloaderCache.GetMasterReferenceTable();

            Assert.Equal(expectedTableCount, masterReferenceTable.ReferenceTables.Count);
            Assert.Equal(expectedAvailableTableCount, masterReferenceTable.GetAvailableReferenceTables().Count);
        }

        [Theory]
        [InlineData(Index.Enums, 47)]
        public void TestGetFileIds(Index index, int expectedFileCount)
        {
            var reportedFileCount = this.Fixture.DownloaderCache.GetFileIds(index).Count();

            Assert.InRange(reportedFileCount, expectedFileCount, 1000);
        }

        [Fact]
        public void TestHttpInterface()
        {
            var httpFile = this.Fixture.DownloaderCache.GetFile<BinaryFile>(Index.Music, 30498);

            Assert.True(httpFile.Data.Length > 0);
        }

        [Fact]
        public void TestReferenceTableCaching()
        {
            var referenceTable1 = this.Fixture.DownloaderCache.GetReferenceTable(Index.Enums);
            var referenceTable2 = this.Fixture.DownloaderCache.GetReferenceTable(Index.Enums);

            Assert.Same(referenceTable1, referenceTable2);
        }

        [Fact]
        public void TestMasterReferenceTableCaching()
        {
            var masterReferenceTable1 = this.Fixture.DownloaderCache.GetMasterReferenceTable();
            var masterReferenceTable2 = this.Fixture.DownloaderCache.GetMasterReferenceTable();

            Assert.Same(masterReferenceTable1, masterReferenceTable2);
        }
    }
}
