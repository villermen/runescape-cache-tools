using System;
using System.IO;
using System.Linq;
using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.Downloader;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Villermen.RuneScapeCacheTools.Cache.FlatFile;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Tests.Tests
{
    [Collection(TestCacheCollection.Name)]
    public class CacheTypesTests
    {
        private readonly TestCacheFixture _fixture;

        public CacheTypesTests(TestCacheFixture fixture)
        {
            this._fixture = fixture;
        }

        /// <summary>
        /// Test for a file that exists, an archive file that exists and a file that doesn't exist.
        /// </summary>
        [Theory]
        [InlineData(typeof(RuneTek5Cache))]
        [InlineData(typeof(DownloaderCache))]
        [InlineData(typeof(FlatFileCache))]
        public void TestGetFile(Type cacheType)
        {
            var cache = this._fixture.GetCache(cacheType);

            var file = cache.GetFile<BinaryFile>(Index.ClientScripts, 3);

            var fileData = file.Data;

            Assert.True(fileData.Length > 0, "File's data is empty.");

            var archiveFile = cache.GetFile<EntryFile>(Index.Enums, 5);

            Assert.Equal(1493044636, archiveFile.Info.Version);
            Assert.Equal(CompressionType.Gzip, archiveFile.Info.CompressionType);

            var archiveEntry = archiveFile.GetEntry<BinaryFile>(255);

            Assert.True(archiveEntry.Data.Length > 0, "Archive entry's data is empty.");

            Assert.Throws<FileNotFoundException>(() =>
            {
                cache.GetFile<BinaryFile>(Index.Music, 180000);
            });
        }

        [Theory]
        [InlineData(typeof(RuneTek5Cache), Index.Music)]
        [InlineData(typeof(RuneTek5Cache), Index.Enums)]
        [InlineData(typeof(RuneTek5Cache), Index.ClientScripts)]
        [InlineData(typeof(DownloaderCache), Index.Enums)]
        public void TestGetReferenceTableFile(Type cacheType, Index index)
        {
            var cache = this._fixture.GetCache(cacheType);

            cache.GetFile<ReferenceTableFile>(Index.ReferenceTables, (int)index);
        }

        [Theory]
        [InlineData(typeof(RuneTek5Cache), Index.Models, 47000)] // Gzip
        [InlineData(typeof(RuneTek5Cache), Index.Enums, 23)] // Bzip2, entries
        [InlineData(typeof(FlatFileCache), Index.Enums, 23)]
        public void TestWriteBinaryFile(Type cacheType, Index index, int fileId)
        {
            var file1 = this._fixture.RuneTek5Cache.GetFile<BinaryFile>(index, fileId);

            this._fixture.RuneTek5Cache.PutFile(file1);

            // Refresh the cache to make sure everything read after this point is freshly obtained
            this._fixture.RuneTek5Cache.Dispose();

            using (var freshRuneTek5Cache = new RuneTek5Cache("testdata/runetek5", true))
            {
                var file2 = freshRuneTek5Cache.GetFile<BinaryFile>(index, fileId);

                // Compare the info objects
                Assert.Equal(file1.Info.UncompressedSize, file2.Info.UncompressedSize);

                // Byte-compare both files
                Assert.True(file1.Data.SequenceEqual(file2.Data));
            }
        }
        
        [Theory]
        [InlineData(typeof(RuneTek5Cache))]
        [InlineData(typeof(FlatFileCache))]
        public void TestGetIndexes(Type cacheType)
        {
            var indexes = this._fixture.GetCache(cacheType).GetIndexes();
            
            Assert.Equal(6, indexes.Count());
        }
    }
}
