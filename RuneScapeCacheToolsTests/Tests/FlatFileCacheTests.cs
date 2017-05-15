using System;
using System.IO;
using System.Linq;
using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache;
using Xunit;
using Xunit.Abstractions;

namespace RuneScapeCacheToolsTests
{
    [Collection(TestCacheCollection.Name)]
    public class FlatFileCacheTests
    {
        private TestCacheFixture Fixture { get; }

        private ITestOutputHelper Output { get; }

        public FlatFileCacheTests(TestCacheFixture fixture, ITestOutputHelper output)
        {
            this.Fixture = fixture;
            this.Output = output;
        }

        [Theory]
        [InlineData(Index.ClientScripts, 3)]
        public void TestExtract(Index index, int fileId)
        {
            var expectedFilePath = $"output/extracted/{(int)index}/{fileId}";

            var startTime = DateTime.UtcNow - TimeSpan.FromSeconds(1);

            // TODO: this.Fixture.RuneTek5Cache.Extract(index, fileId, true);

            Assert.True(File.Exists(expectedFilePath), $"File was not extracted, or not extracted to {expectedFilePath}.");

            var modifiedTime = File.GetLastAccessTimeUtc(expectedFilePath);

            Assert.True(startTime <= modifiedTime, $"Starting time of test ({startTime}) was not earlier or equal to extracted file modified time ({modifiedTime}).");
        }

        [Fact]
        public void TestExtractWithEntries()
        {
            var expectedFilePath = $"output/extracted/{(int)Index.Enums}/5-65";

            var startTime = DateTime.UtcNow - TimeSpan.FromSeconds(1);

            // TODO: this.Fixture.RuneTek5Cache.Extract(Index.Enums, 5, true);

            Assert.True(File.Exists(expectedFilePath), $"File entry was not extracted, or not extracted to {expectedFilePath}.");

            var modifiedTime = File.GetLastAccessTimeUtc(expectedFilePath);

            Assert.True(startTime <= modifiedTime, $"Starting time of test ({startTime}) was not earlier or equal to extracted file modified time ({modifiedTime}).");
        }

        [Fact]
        public void TestExtractExtension()
        {
            // TODO: this.Fixture.RuneTek5Cache.Extract(Index.LoadingSprites, 8501);

            // Verify that the .jpg extension was added
            Assert.True(File.Exists($"output/extracted/{(int)Index.LoadingSprites}/30556.jpg"));
        }

        [Fact]
        public void TestGetIndexes()
        {
            var indexes = this.Fixture.FlatFileCache.GetIndexes();
            Assert.Equal(7, indexes.Count());
        }
    }
}