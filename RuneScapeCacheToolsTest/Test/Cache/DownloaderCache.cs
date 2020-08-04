using System.Linq;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Test.Fixture;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Test.Cache
{
    [Collection(TestCacheCollection.Name)]
    public class DownloaderCache : BaseTests
    {
        private TestCacheFixture Fixture { get; }

        public DownloaderCache(TestCacheFixture fixture)
        {
            this.Fixture = fixture;
        }

        [Fact]
        public void TestDownloadReferenceTable()
        {
            this.Fixture.DownloaderCache.GetReferenceTable(CacheIndex.ClientScripts);
            this.Fixture.DownloaderCache.GetReferenceTable(CacheIndex.Music);
            this.Fixture.DownloaderCache.GetFile(CacheIndex.ReferenceTables, (int)CacheIndex.Enums);

            var referenceTable17 = this.Fixture.DownloaderCache.GetReferenceTable(CacheIndex.Enums);

            Assert.InRange(referenceTable17.FileIds.Count(), 48, 1000);
        }

        [Theory]
        [InlineData(57, 42)]
        public void TestDownloadMasterReferenceTable(int expectedTableCount, int expectedAvailableTableCount)
        {
            var masterReferenceTable = this.Fixture.DownloaderCache.GetMasterReferenceTable();

            Assert.Equal(expectedTableCount, masterReferenceTable.ReferenceTableInfos.Count);
            Assert.Equal(expectedAvailableTableCount, masterReferenceTable.AvailableReferenceTables.Count());
        }

        [Theory]
        [InlineData(CacheIndex.Enums, 47)]
        public void TestGetFileIds(CacheIndex index, int expectedFileCount)
        {
            var reportedFileCount = this.Fixture.DownloaderCache.GetAvailableFileIds(index).Count();

            Assert.InRange(reportedFileCount, expectedFileCount, 1000);
        }

        [Fact]
        public void TestHttpInterface()
        {
            var httpFile = this.Fixture.DownloaderCache.GetFile(CacheIndex.Music, 30498);

            Assert.True(httpFile.Data.Length > 0);
        }

        [Fact]
        public void TestReferenceTableCaching()
        {
            var referenceTable1 = this.Fixture.DownloaderCache.GetReferenceTable(CacheIndex.Enums);
            var referenceTable2 = this.Fixture.DownloaderCache.GetReferenceTable(CacheIndex.Enums);

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
