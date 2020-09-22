using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Test.Fixture;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Test.File
{
    [Collection(TestCacheCollection.Name)]
    public class ItemDefinitionFileTests : BaseTests
    {
        private TestCacheFixture Fixture { get; }

        public ItemDefinitionFileTests(TestCacheFixture fixture)
        {
            this.Fixture = fixture;
        }

        [Theory]
        [InlineData(CacheIndex.ItemDefinitions, 5, 241, "Oak logs", 12)]
        [InlineData(CacheIndex.ItemDefinitions, 155, 134, "Hazelmere's signet ring", 4, Skip = "Fails because of new opcode 15")]
        [InlineData(CacheIndex.ItemDefinitions, 155, 104, "Attuned crystal teleport seed", 14, Skip = "Fails because of new opcode 15")]
        public void TestItemDefinitionFile(CacheIndex index, int fileId, int entryId, string expectedName, int expectedPropertyCount)
        {
            var itemDefinitionFile = ItemDefinitionFile.Decode(
                this.Fixture.JavaClientCache.GetFile(index, fileId).Entries[entryId]
            );

            Assert.Equal(expectedName, itemDefinitionFile.Name);
            Assert.Equal(expectedPropertyCount, itemDefinitionFile.Properties.Count);
        }
    }
}
