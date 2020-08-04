using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Test.Fixture;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Test.File
{
    [Collection(TestCacheCollection.Name)]
    public class JagaFileTests : BaseTests
    {
        private TestCacheFixture Fixture { get; }

        public JagaFileTests(TestCacheFixture fixture)
        {
            this.Fixture = fixture;
        }

        [Theory]
        [InlineData(38900, 53)]
        public void TestDecode(int fileId, int expectedNumberOfChunks)
        {
            var jagaFile = JagaFile.Decode(this.Fixture.JavaClientCache.GetFile(CacheIndex.Music, fileId).Data);

            Assert.Equal(expectedNumberOfChunks, jagaFile.ChunkCount);
        }
    }
}
