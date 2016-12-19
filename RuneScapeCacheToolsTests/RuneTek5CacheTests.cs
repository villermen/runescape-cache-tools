using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Xunit;
using Xunit.Abstractions;

namespace RuneScapeCacheToolsTests
{
    using Villermen.RuneScapeCacheTools.Cache.CacheFile;

    [Collection("TestCache")]
    public class RuneTek5CacheTests
    {
        private ITestOutputHelper Output { get; }

        private CacheFixture Fixture { get; }

        public RuneTek5CacheTests(ITestOutputHelper output, CacheFixture fixture)
        {
            this.Output = output;
            this.Fixture = fixture;
        }

        [Theory]
        [InlineData(Index.ClientScripts, 3)]
        public void TestExtract(Index index, int fileId)
        {
            var expectedFilePath = $"output/extracted/{index}/{fileId}";

            var startTime = DateTime.UtcNow - TimeSpan.FromSeconds(1);

            this.Fixture.RuneTek5Cache.Extract(index, fileId, true);

            Assert.True(File.Exists(expectedFilePath), $"File was not extracted, or not extracted to {expectedFilePath}.");

            var modifiedTime = File.GetLastAccessTimeUtc(expectedFilePath);

            Assert.True(startTime <= modifiedTime, $"Starting time of test ({startTime}) was not earlier or equal to extracted file modified time ({modifiedTime}).");
        }

        [Fact]
        public void TestExtractWithEntries()
        {
            var expectedFilePath = $"output/extracted/{Index.Enums}/5-65";

            var startTime = DateTime.UtcNow - TimeSpan.FromSeconds(1);

            this.Fixture.RuneTek5Cache.Extract(Index.Enums, 5, true);

            Assert.True(File.Exists(expectedFilePath), $"File entry was not extracted, or not extracted to {expectedFilePath}.");

            var modifiedTime = File.GetLastAccessTimeUtc(expectedFilePath);

            Assert.True(startTime <= modifiedTime, $"Starting time of test ({startTime}) was not earlier or equal to extracted file modified time ({modifiedTime}).");
        }

        [Fact]
        public void TestExtractExtension()
        {
            // TODO: Use pre-built cache for this
            this.Fixture.Downloader.Extract(Index.LoadingSprites, 8501);

            // Verify that the .jpg extension was added
            Assert.True(File.Exists($"output/extracted/{Index.LoadingSprites}/8501.jpg"));
        }

        /// <summary>
        /// Test for a file that exists, an archive file that exists and a file that doesn't exist.
        /// </summary>
        [Fact]
        public void TestGetFile()
        {
            var file = this.Fixture.RuneTek5Cache.GetFile<DataCacheFile>(Index.ClientScripts, 3);

            var fileData = file.Data;

            Assert.True(fileData.Length > 0, "File's data is empty.");

            var archiveFile = this.Fixture.RuneTek5Cache.GetFile<DataCacheFile>(Index.Enums, 5);

            var archiveEntry = archiveFile.Entries[255];

            Assert.True(archiveEntry.Length > 0, "Archive entry's data is empty.");

            try
            {
                this.Fixture.RuneTek5Cache.GetFile<DataCacheFile>(Index.Music, 30);

                Assert.True(false, "Cache returned a file that shouldn't exist.");
            }
            catch (CacheException exception)
            {
                Assert.True(exception.Message.Contains("no size"), "Non-existent file cache exception had the wrong message.");
            }
        }

        [Theory]
        [InlineData(Index.Music)]
        [InlineData(Index.Enums)]
        [InlineData(Index.ClientScripts)]
        public void TestGetReferenceTable(Index index)
        {
            var referenceTable = this.Fixture.RuneTek5Cache.GetReferenceTable(index);
        }

        [Theory]
        [InlineData(Index.Models, 47000)] // Gzip
        [InlineData(Index.Enums, 23)] // Bzip2, entries
        public void TestWriteCacheFile(Index index, int fileId)
        {
            var file1 = this.Fixture.RuneTek5Cache.GetFile<DataCacheFile>(index, fileId);

            this.Fixture.RuneTek5Cache.PutFile(file1);

            // Refresh the cache to make sure everything read after this point is freshly obtained
            this.Fixture.RuneTek5Cache.Refresh();

            var file2 = this.Fixture.RuneTek5Cache.GetFile<DataCacheFile>(index, fileId);

            // Compare the info objects
            Assert.Equal(file1.Info.UncompressedSize, file2.Info.UncompressedSize);

            // Byte-compare all entries in both files
            for (var entryIndex = 0; entryIndex < file1.Entries.Length; entryIndex++)
            {
                Assert.True(
                    file1.Entries[entryIndex].SequenceEqual(file2.Entries[entryIndex]),
                    $"Entry {entryIndex} from initial file did not match the one from the file after being written and read back.");
            }
        }
    }
}