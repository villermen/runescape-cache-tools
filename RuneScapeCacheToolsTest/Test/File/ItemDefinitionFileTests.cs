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
        [InlineData(CacheIndex.ItemDefinitions, 155, 134, "Hazelmere's signet ring", 8)]
        public void TestItemDefinitionFile(CacheIndex index, int fileId, int entryId, string expectedName, int expectedPropertyCount)
        {
            var itemDefinitionFile = ItemDefinitionFile.Decode(
                this.Fixture.JavaClientCache.GetFile(index, fileId).Entries[entryId]
            );

            Assert.Equal(expectedName, itemDefinitionFile.Name);
            Assert.Equal(expectedPropertyCount, itemDefinitionFile.Properties?.Count);
        }

        [Fact]
        public void TestOptionParsing()
        {
            var itemDefinitionFile = ItemDefinitionFile.Decode(
                this.Fixture.JavaClientCache.GetFile(CacheIndex.ItemDefinitions, 155).Entries[104]
            );

            Assert.Equal("Attuned crystal teleport seed", itemDefinitionFile.Name);
            Assert.Equal(new []
            {
                "Activate",
                "Put in pocket",
                "null", // "Lletya" in game. Probably dynamically hacked in via this "null" value?
                "Use",
                "null", // "Temple of Light" in game. Same deal here.
                "Destroy",
                "Examine",
            }, itemDefinitionFile.GetInventoryOptions());
            Assert.Equal(new []
            {
                "Remove",
                "Activate",
                null,
                null,
                null,
                "Examine",
            }, itemDefinitionFile.GetEquipOptions());
        }
    }
}
