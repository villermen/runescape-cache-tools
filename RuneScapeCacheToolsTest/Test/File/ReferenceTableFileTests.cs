using System.Linq;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Test.Fixture;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Test.File
{
    [Collection(TestCacheCollection.Name)]
    public class ReferenceTableFileTests : BaseTests
    {
        private readonly TestCacheFixture _fixture;

        public ReferenceTableFileTests(TestCacheFixture fixture)
        {
            this._fixture = fixture;
        }

        [Theory]
        [InlineData(CacheIndex.Music)]
        [InlineData(CacheIndex.AnimationFrames)]
        public void TestDecodeEncode(CacheIndex index)
        {
            var referenceTableFile = this._fixture.JavaClientCache.GetFile(CacheIndex.ReferenceTables, (int)index);
            var referenceTable = ReferenceTableFile.Decode(referenceTableFile.Data);

            var encodedFile = referenceTable.Encode();

            Assert.True(referenceTableFile.Data.SequenceEqual(encodedFile));
        }
    }
}
