using System;
using System.IO;
using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Villermen.RuneScapeCacheTools.Cache.FlatFile;
using Xunit;
using Xunit.Abstractions;

namespace Villermen.RuneScapeCacheTools.Tests.Tests
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
        [InlineData(Index.ClientScripts, 3)]
        public void TestPutFile(Index index, int fileId)
        {
            var expectedFilePath = $"output/{(int)index}/{fileId}";

            var startTime = DateTime.UtcNow - TimeSpan.FromSeconds(1);
            
            this._outputFlatFileCache.PutFile(
                this._fixture.FlatFileCache.GetFile<BinaryFile>(index, fileId));
            
            Assert.True(File.Exists(expectedFilePath), $"File was not extracted, or not extracted to {expectedFilePath}.");

            var modifiedTime = File.GetLastAccessTimeUtc(expectedFilePath);

            Assert.True(startTime <= modifiedTime, $"Starting time of test ({startTime}) was not earlier or equal to extracted file modified time ({modifiedTime}).");
        }

        [Theory]
        [InlineData(Index.Enums, 5, 65)]
        public void TestFileWithEntries(Index index, int fileId, int entryId)
        {
            var expectedFilePath = $"output/{(int)index}/{fileId}/{entryId}";

            var file = this._fixture.FlatFileCache.GetFile<EntryFile>(index, fileId);
            this._outputFlatFileCache.PutFile(file);

            FlatFileCacheTests.AssertFileExistsAndModified(expectedFilePath);
            
            // Readback
            var readFile = this._outputFlatFileCache.GetFile<EntryFile>(index, fileId);

            Assert.Equal(file.EntryCount, readFile.EntryCount);
            Assert.Equal(
                file.GetEntry<BinaryFile>(entryId).Data.Length,
                readFile.GetEntry<BinaryFile>(entryId).Data.Length);
        }

        [Theory]
        [InlineData(Index.LoadingSprites, 30556)]
        public void TestFileWithExtension(Index index, int fileId)
        {
            var file = this._fixture.FlatFileCache.GetFile<BinaryFile>(index, fileId);
            this._outputFlatFileCache.PutFile(file);

            // Verify that the .jpg extension was added
            var expectedFilePath = $"output/{(int)Index.LoadingSprites}/30556.jpg";
            FlatFileCacheTests.AssertFileExistsAndModified(expectedFilePath);

            // Readback
            var readFile = this._outputFlatFileCache.GetFile<BinaryFile>(index, fileId);
            
            Assert.Equal(file.Data.Length, readFile.Data.Length);
        }

        private static void AssertFileExistsAndModified(string filePath)
        {
            Assert.True(File.Exists(filePath));
            
            var startTime = DateTime.UtcNow - TimeSpan.FromSeconds(1);
            var modifiedTime = File.GetLastAccessTimeUtc(filePath);
            Assert.True(startTime <= modifiedTime, $"Starting time of test ({startTime}) was not earlier or equal to extracted file modified time ({modifiedTime}).");
        }
    }
}