using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Tests.Tests.FileTypesTests
{
    [Collection(TestCacheCollection.Name)]
    public class ItemDefinitionFileTests
    {
        private TestCacheFixture Fixture { get; }

        public ItemDefinitionFileTests(TestCacheFixture fixture)
        {
            this.Fixture = fixture;
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
    }
}