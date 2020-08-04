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

        private readonly ITestOutputHelper _output;

        private readonly FlatFileCache _outputFlatFileCache;

        public FlatFileCacheTests(TestCacheFixture fixture, ITestOutputHelper output)
        {
            this._fixture = fixture;
            this._output = output;
            this._outputFlatFileCache = new FlatFileCache("output");
        }

        [Theory]
        [InlineData(CacheIndex.ClientScripts, 3)]
        public void TestPutFile(CacheIndex index, int fileId)
        {
            var expectedFilePath = $"output/{(int)index}/{fileId}";

            var startTime = DateTime.UtcNow - TimeSpan.FromSeconds(1);

            this._outputFlatFileCache.PutFile(
                index,
                fileId,
                this._fixture.FlatFileCache.GetFile(index, fileId)
            );

            Assert.True(System.IO.File.Exists(expectedFilePath), $"File was not extracted, or not extracted to {expectedFilePath}.");

            var modifiedTime = System.IO.File.GetLastAccessTimeUtc(expectedFilePath);

            Assert.True(startTime <= modifiedTime, $"Starting time of test ({startTime}) was not earlier or equal to extracted file modified time ({modifiedTime}).");
        }

        [Theory(Skip = "Entries are no longer split out by default.")]
        [InlineData(CacheIndex.Enums, 5, 65)]
        public void TestFileWithEntries(CacheIndex index, int fileId, int entryId)
        {
            var expectedFilePath = $"output/{(int)index}/{fileId}/{entryId}";

            var file = EntryFile.DecodeFromCacheFile(this._fixture.FlatFileCache.GetFile(index, fileId));
            this._outputFlatFileCache.PutFile(index, fileId, file.EncodeToCacheFile());

            FlatFileCacheTests.AssertFileExistsAndModified(expectedFilePath);

            // Readback
            // TODO: Won't work because info is discarded in flatfile.
            var readFile = EntryFile.DecodeFromCacheFile(this._outputFlatFileCache.GetFile(index, fileId));

            Assert.Equal(file.Entries.Count, readFile.Entries.Count);
            Assert.Equal(
                file.Entries[entryId].Length,
                readFile.Entries[entryId].Length
            );
        }

        [Theory]
        [InlineData(CacheIndex.LoadingSprites, 30556)]
        public void TestFileWithExtension(CacheIndex index, int fileId)
        {
            var file = this._fixture.FlatFileCache.GetFile(index, fileId);
            this._outputFlatFileCache.PutFile(index, fileId, file);

            // Verify that the .jpg extension was added
            var expectedFilePath = $"output/{(int)CacheIndex.LoadingSprites}/30556.jpg";
            FlatFileCacheTests.AssertFileExistsAndModified(expectedFilePath);

            // Readback
            var readFile = this._outputFlatFileCache.GetFile(index, fileId);

            Assert.Equal(file.Data.Length, readFile.Data.Length);
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
