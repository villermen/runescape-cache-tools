using System.Linq;
using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Tests.Tests.FileTypesTests
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
            var binaryFile = this.Fixture.RuneTek5Cache.GetFile<BinaryFile>(Index.ItemDefinitions, 155);
            var entryFile = new EntryFile();
            entryFile.FromBinaryFile(binaryFile);

            var binaryFile1 = entryFile.GetEntry<BinaryFile>(0);
            Assert.Equal(242, binaryFile1.Data.Length);

            var itemDefinitionFile = entryFile.GetEntry<ItemDefinitionFile>(0);
            Assert.Equal(2609, itemDefinitionFile.UnknownShort4);

            var itemDefinitionFiles = entryFile.GetEntries<ItemDefinitionFile>();
            Assert.Equal(256, entryFile.Capacity);
            Assert.Equal(2609, itemDefinitionFiles.First(file => file.Info.EntryId == 0).UnknownShort4);

            Assert.True(entryFile.Encode().SequenceEqual(binaryFile.Data));
        }
    }
}