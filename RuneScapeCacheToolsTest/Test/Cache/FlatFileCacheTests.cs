using System;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Test.Fixture;
using Xunit;
using Xunit.Abstractions;

namespace Villermen.RuneScapeCacheTools.Test.Cache
{
    [Collection(TestCacheCollection.Name)]
    public class FlatFileCacheTests
    {
        private readonly TestCacheFixture _fixture;

        private readonly FlatFileCache _outputFlatFileCache;

        public FlatFileCacheTests(TestCacheFixture fixture)
        {
            this._fixture = fixture;
            this._outputFlatFileCache = new FlatFileCache("output");
        }

        [Theory]
        [InlineData(CacheIndex.ClientScripts, 3)]
        public void TestPutAndGetFile(CacheIndex index, int fileId)
        {
            var expectedFilePath = $"output/{(int)index}/{fileId}";

            // Read file from fixture and put it into our own cache.
            var file1 = this._fixture.FlatFileCache.GetFile(index, fileId);
            this._outputFlatFileCache.PutFile(index, fileId, file1);
            Assert.True(
                System.IO.File.Exists(expectedFilePath),
                $"File was not extracted, or not extracted to {expectedFilePath}."
            );

            // Ensure that the file was modified by this test as it could just be a leftover from last test.
            var writeTime = DateTimeOffset.UtcNow;
            DateTimeOffset modifiedTime = System.IO.File.GetLastAccessTimeUtc(expectedFilePath);
            Assert.False(
                modifiedTime.ToUnixTimeSeconds() < writeTime.ToUnixTimeSeconds(),
                $"File modified time ({modifiedTime:u}) was less than writing time ({writeTime:u})."
            );

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
            FlatFileCacheTests.AssertFileExistsAndModified(expectedFilePath);

            var readFile = this._outputFlatFileCache.GetFile(index, fileId);
            Assert.Equal(file.Data, readFile.Data);
        }

        private static void AssertFileExistsAndModified(string filePath)
        {
            Assert.True(System.IO.File.Exists(filePath));

            var startTime = DateTime.UtcNow - TimeSpan.FromSeconds(1);
            var modifiedTime = System.IO.File.GetLastAccessTimeUtc(filePath);
            Assert.True(startTime <= modifiedTime, $"Starting time of test ({startTime}) was not earlier or equal to extracted file modified time ({modifiedTime}).");
        }
    }
}
