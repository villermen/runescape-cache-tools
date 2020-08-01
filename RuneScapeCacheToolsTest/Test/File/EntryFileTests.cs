using System.Linq;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Test.Fixture;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Test.File
{
    [Collection(TestCacheCollection.Name)]
    public class EntryFileTests
    {
        private TestCacheFixture Fixture { get; }

        public EntryFileTests(TestCacheFixture fixture)
        {
            this.Fixture = fixture;
        }

        [Fact]
        public void TestEntryFile()
        {
            var file = this.Fixture.JavaClientCache.GetFile(CacheIndex.ItemDefinitions, 155);
            var entryFile = EntryFile.Decode(file);
            Assert.Equal(256, entryFile.Entries.Count);

            var itemDefinitionData = entryFile.Entries[0];
            Assert.Equal(242, itemDefinitionData.Length);

            var itemDefinitionFile = ItemDefinitionFile.Decode(itemDefinitionData);
            Assert.Equal(2609, itemDefinitionFile.UnknownShort4);

            Assert.True(entryFile.Encode().SequenceEqual(file.Data));
        }

        [Fact]
        public void TestEncodeDecodeWithEmptyEntries()
        {
            var entryData = new byte[] { 0, 12, 123, 8 };

            var entryFile = new EntryFile();
            entryFile.Entries[5] = entryData;

            var entryFileData = entryFile.Encode();
            var decodedEntryFile = EntryFile.Decode(new CacheFile(entryFileData));

            Assert.True(entryData.SequenceEqual(decodedEntryFile.Entries[5]));
        }
    }
}
