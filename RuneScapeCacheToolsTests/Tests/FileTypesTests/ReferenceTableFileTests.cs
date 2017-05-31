using System.Linq;
using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Tests.Tests.FileTypesTests
{
    [Collection(TestCacheCollection.Name)]
    public class ReferenceTableFileTests
    {
        private readonly TestCacheFixture _fixture;

        public ReferenceTableFileTests(TestCacheFixture fixture)
        {
            this._fixture = fixture;
        }

        [Theory]
        [InlineData(Index.Music)]
        [InlineData(Index.AnimationFrames)]
        public void TestDecodeEncode(Index index)
        {
            var referenceTableFile = this._fixture.RuneTek5Cache.GetFile<BinaryFile>(Index.ReferenceTables, (int)index);
            var referenceTable =  new ReferenceTableFile();
            referenceTable.FromBinaryFile(referenceTableFile);

            var encodedFile = referenceTable.ToBinaryFile();

            Assert.True(referenceTableFile.Data.SequenceEqual(encodedFile.Data));
        }

        [Fact]
        public void TestTemp()
        {
            this._fixture.DownloaderCache.GetFile<ReferenceTableFile>(Index.ReferenceTables, (int)Index.Enums);
        }
    }
}