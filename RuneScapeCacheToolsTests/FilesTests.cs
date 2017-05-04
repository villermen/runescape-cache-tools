using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.Files;
using Xunit;

namespace RuneScapeCacheToolsTests
{
    [Collection(TestCacheCollection.Name)]
    public class FilesTests
    {
        private TestCacheFixture Fixture { get; }

        public FilesTests(TestCacheFixture fixture)
        {
            this.Fixture = fixture;
        }

        [Theory]
        [InlineData(38900, 53)]
        public void TestJagaFile(int fileId, int expectedNumberOfChunks)
        {
            var jagaFile = this.Fixture.RuneTek5Cache.GetFile<JagaFile>(Index.Music, fileId);

            Assert.Equal(expectedNumberOfChunks, jagaFile.ChunkCount);
        }

        [Theory]
        // [InlineData(Index.Objects, 155, 134, "Hazelmere's signet ring", 1337)]
        [InlineData(Index.Objects, 5, 241, "Oak logs", 12)]
        // [InlineData(Index.Objects, 155, 104, "Attuned crystal teleport seed", 1337)]
        public void TestItemDefinitionFile(Index index, int fileId, int entryId, string expectedName, int expectedPropertyCount)
        {
            var itemDefinition = this.Fixture.RuneTek5Cache.GetFile<ItemDefinition>(index, fileId, entryId);

            Assert.Equal(expectedName, itemDefinition.Name);
            Assert.Equal(expectedPropertyCount, itemDefinition.Properties.Count);
        }
    }
}