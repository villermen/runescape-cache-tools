using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Tests.Tests.FileTypesTests
{
    [Collection(TestCacheCollection.Name)]
    public class JagaFileTests
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
            var jagaFile = this.Fixture.RuneTek5Cache.GetFile<JagaFile>(Index.Music, fileId);

            Assert.Equal(expectedNumberOfChunks, jagaFile.ChunkCount);
        }
    }
}