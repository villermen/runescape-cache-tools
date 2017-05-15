using System.Linq;
using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Xunit;

namespace RuneScapeCacheToolsTests.FileTypesTests
{
    [Collection(TestCacheCollection.Name)]
    public class ReferenceTableFileTests
    {
        private TestCacheFixture Fixture { get; }

        public ReferenceTableFileTests(TestCacheFixture fixture)
        {
            this.Fixture = fixture;
        }

        [Theory]
        [InlineData(Index.Music)]
        public void TestEncode(Index index)
        {
            var referenceTableFile = this.Fixture.RuneTek5Cache.GetFile<BinaryFile>(Index.ReferenceTables, (int)index);
            var referenceTable =  new ReferenceTableFile();
            referenceTable.FromBinaryFile(referenceTableFile);

            var encodedFile = referenceTable.ToBinaryFile();

            Assert.True(referenceTableFile.Data.SequenceEqual(encodedFile.Data));
        }
    }
}