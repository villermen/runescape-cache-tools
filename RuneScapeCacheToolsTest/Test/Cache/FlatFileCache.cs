using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Test.Fixture;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Test.Cache
{
    [Collection(TestCacheCollection.Name)]
    public class FlatFileCache : BaseTests
    {
        private readonly TestCacheFixture _fixture;
        private readonly RuneScapeCacheTools.Cache.FlatFileCache _outputFlatFileCache;

        public FlatFileCache(TestCacheFixture fixture)
        {
            this._fixture = fixture;
            this._outputFlatFileCache = new RuneScapeCacheTools.Cache.FlatFileCache("output");
        }

        [Theory]
        [InlineData(CacheIndex.ClientScripts, 3)]
        public void TestPutAndGetFile(CacheIndex index, int fileId)
        {
            var expectedFilePath = $"output/{(int)index}/{fileId}";

            // Read file from fixture and put it into our own cache.
            var file1 = this._fixture.FlatFileCache.GetFile(index, fileId);
            this._outputFlatFileCache.PutFile(index, fileId, file1);

            this.AssertFileExistsAndModified(expectedFilePath);

            var file2 = this._outputFlatFileCache.GetFile(index, fileId);
            Assert.Equal(file1.Data, file2.Data);
        }

        [Theory]
        [InlineData(CacheIndex.LoadingSprites, 30462, ".jpg")]
        [InlineData(CacheIndex.Enums, 5, ".entries")]
        public void TestFileWithExtension(CacheIndex index, int fileId, string expectedExtension)
        {
            // We use JavaClientCache to source the file because FlatFileCache doesn't preserve entry information.
            var file = this._fixture.JavaClientCache.GetFile(index, fileId);
            this._outputFlatFileCache.PutFile(index, fileId, file);

            var expectedFilePath = $"output/{(int)index}/{fileId}{expectedExtension}";
            this.AssertFileExistsAndModified(expectedFilePath);

            var readFile = this._outputFlatFileCache.GetFile(index, fileId);
            Assert.Equal(file.Data, readFile.Data);
        }
    }
}
