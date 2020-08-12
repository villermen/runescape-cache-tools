using System.Collections.Generic;
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

        [Fact]
        public void TestEncodeDecode()
        {
            var fileInfo = new CacheFileInfo
            {
                Crc = -893872,
                Version = 1598632487,
                CompressedSize = 2389,
                UncompressedSize = 9238,
                Entries = new Dictionary<int, CacheFileEntryInfo>()
            };
            fileInfo.Entries[0] = new CacheFileEntryInfo();
            fileInfo.Entries[5] = new CacheFileEntryInfo();

            var referenceTable = new ReferenceTableFile
            {
                Options = ReferenceTableOptions.Sizes,
            };
            referenceTable.SetFileInfo(3, fileInfo);

            var encodedReferenceTable = referenceTable.Encode();
            var decodedReferenceTable = ReferenceTableFile.Decode(encodedReferenceTable);
            var decodedFileInfo = decodedReferenceTable.GetFileInfo(3);

            Assert.Equal(fileInfo.Crc, decodedFileInfo.Crc);
            Assert.Equal(fileInfo.Version, decodedFileInfo.Version);
            Assert.Equal(fileInfo.CompressedSize, decodedFileInfo.CompressedSize);
            Assert.Equal(fileInfo.UncompressedSize, decodedFileInfo.UncompressedSize);
            Assert.Equal(fileInfo.Entries.Keys, decodedFileInfo.Entries.Keys);
        }

        [Theory]
        [InlineData(CacheIndex.Music)]
        [InlineData(CacheIndex.AnimationFrames)]
        public void TestDecodeEncodeFromCache(CacheIndex index)
        {
            var referenceTableFile = this._fixture.JavaClientCache.GetFile(CacheIndex.ReferenceTables, (int)index);
            var referenceTable = ReferenceTableFile.Decode(referenceTableFile.Data);

            var encodedFile = referenceTable.Encode();

            Assert.True(referenceTableFile.Data.SequenceEqual(encodedFile));
        }
    }
}
