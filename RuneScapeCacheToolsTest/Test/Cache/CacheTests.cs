using System;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Test.Fixture;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Test.Cache
{
    [Collection(TestCacheCollection.Name)]
    public class CacheTests : BaseTests
    {
        private readonly TestCacheFixture _fixture;

        public CacheTests(TestCacheFixture fixture)
        {
            this._fixture = fixture;
        }

        /// <summary>
        /// Test for a file that exists, an entry file that exists and a file that doesn't exist.
        /// </summary>
        [Theory]
        [InlineData(typeof(JavaClientCache))]
        [InlineData(typeof(FlatFileCache))]
        [InlineData(typeof(DownloaderCache))]
        [InlineData(typeof(NxtClientCache))]
        public void TestGetFile(Type cacheType)
        {
            var cache = this._fixture.Caches[cacheType];

            var scriptFile = cache.GetFile(CacheIndex.ClientScripts, 3);
            Assert.True(scriptFile.Data.Length > 0);

            var entryCacheFile = cache.GetFile(CacheIndex.Enums, 5);
            Assert.True(entryCacheFile.Data.Length > 0);

            Assert.Throws<CacheFileNotFoundException>(() =>
            {
                cache.GetFile(CacheIndex.Music, 180000);
            });
        }

        [Theory]
        [InlineData(typeof(JavaClientCache), CacheIndex.Music)]
        [InlineData(typeof(JavaClientCache), CacheIndex.Enums)]
        [InlineData(typeof(JavaClientCache), CacheIndex.ClientScripts)]
        [InlineData(typeof(DownloaderCache), CacheIndex.Enums)]
        [InlineData(typeof(NxtClientCache), CacheIndex.Enums)]
        public void TestGetReferenceTableFile(Type cacheType, CacheIndex index)
        {
            var cache = this._fixture.Caches[cacheType];

            ReferenceTableFile.Decode(cache.GetFile(CacheIndex.ReferenceTables, (int)index).Data);
        }

        [Theory]
        [InlineData(typeof(JavaClientCache), CacheIndex.Models, 47000)] // Gzip
        [InlineData(typeof(JavaClientCache), CacheIndex.Enums, 23)] // Bzip2, entries
        [InlineData(typeof(FlatFileCache), CacheIndex.Enums, 23)]
        [InlineData(typeof(NxtClientCache), CacheIndex.Enums, 23)] // Zlib, entries
        public void TestPutFile(Type cacheType, CacheIndex index, int fileId)
        {
            var cache = this._fixture.Caches[cacheType];

            var file1 = cache.GetFile(index, fileId);

            cache.PutFile(index, fileId, file1);

            // Refresh a reference table cache to make sure everything read after this point is freshly obtained
            (cache as ReferenceTableCache)?.ClearCachedReferenceTables();

            var file2 = cache.GetFile(index, fileId);

            // Compare the info objects
            Assert.Equal(file1.Info.UncompressedSize, file2.Info.UncompressedSize);

            // Byte-compare both files
            Assert.True(file1.Data.SequenceEqual(file2.Data));
        }

        [Theory]
        [InlineData(typeof(JavaClientCache), 7)]
        [InlineData(typeof(FlatFileCache), 7)]
        [InlineData(typeof(NxtClientCache), 7)]
        [InlineData(typeof(DownloaderCache), 45)]
        public void TestGetIndexes(Type cacheType, int amountOfIndexes)
        {
            var indexes = this._fixture.Caches[cacheType].GetAvailableIndexes();

            Assert.Equal(amountOfIndexes, indexes.Count());
        }
    }
}
