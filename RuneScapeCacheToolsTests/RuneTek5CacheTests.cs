using System;
using System.IO;
using System.Linq;
using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Xunit;

namespace RuneScapeCacheToolsTests
{
    [Collection(TestCacheCollection.Name)]
    public class RuneTek5CacheTests
    {
        private TestCacheFixture Fixture { get; }

        public RuneTek5CacheTests(TestCacheFixture fixture)
        {
            this.Fixture = fixture;
        }

        [Theory]
        [InlineData(Index.ClientScripts, 3)]
        public void TestExtract(Index index, int fileId)
        {
            var expectedFilePath = $"output/extracted/{(int)index}/{fileId}";

            var startTime = DateTime.UtcNow - TimeSpan.FromSeconds(1);

            this.Fixture.RuneTek5Cache.Extract(index, fileId, true);

            Assert.True(File.Exists(expectedFilePath), $"File was not extracted, or not extracted to {expectedFilePath}.");

            var modifiedTime = File.GetLastAccessTimeUtc(expectedFilePath);

            Assert.True(startTime <= modifiedTime, $"Starting time of test ({startTime}) was not earlier or equal to extracted file modified time ({modifiedTime}).");
        }

        [Fact]
        public void TestExtractWithEntries()
        {
            var expectedFilePath = $"output/extracted/{(int)Index.Enums}/5-65";

            var startTime = DateTime.UtcNow - TimeSpan.FromSeconds(1);

            this.Fixture.RuneTek5Cache.Extract(Index.Enums, 5, true);

            Assert.True(File.Exists(expectedFilePath), $"File entry was not extracted, or not extracted to {expectedFilePath}.");

            var modifiedTime = File.GetLastAccessTimeUtc(expectedFilePath);

            Assert.True(startTime <= modifiedTime, $"Starting time of test ({startTime}) was not earlier or equal to extracted file modified time ({modifiedTime}).");
        }

        [Fact]
        public void TestExtractExtension()
        {
            this.Fixture.RuneTek5Cache.Extract(Index.LoadingSprites, 8501);

            // Verify that the .jpg extension was added
            Assert.True(File.Exists($"output/extracted/{(int)Index.LoadingSprites}/8501.jpg"));
        }

        /// <summary>
        /// Test for a file that exists, an archive file that exists and a file that doesn't exist.
        /// </summary>
        [Fact]
        public void TestGetFile()
        {
            var file = this.Fixture.RuneTek5Cache.GetFile<BinaryFile>(Index.ClientScripts, 3);

            var fileData = file.Data;

            Assert.True(fileData.Length > 0, "File's data is empty.");

            var archiveFile = this.Fixture.RuneTek5Cache.GetFile<EntryFile>(Index.Enums, 5);

            var archiveEntry = archiveFile.Entries[255];

            Assert.True(archiveEntry.Data.Length > 0, "Archive entry's data is empty.");

            Assert.Throws<FileNotFoundException>(() =>
            {
                this.Fixture.RuneTek5Cache.GetFile<BinaryFile>(Index.Music, 30);
            });
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
            var file1 = this.Fixture.RuneTek5Cache.GetFile<BinaryFile>(index, fileId);

            this.Fixture.RuneTek5Cache.PutFile(file1);

            // Refresh the cache to make sure everything read after this point is freshly obtained
            this.Fixture.RuneTek5Cache.Refresh();

            var file2 = this.Fixture.RuneTek5Cache.GetFile<BinaryFile>(index, fileId);

            // Compare the info objects
            Assert.Equal(file1.Info.UncompressedSize, file2.Info.UncompressedSize);

            // Byte-compare both files
            Assert.True(file1.Data.SequenceEqual(file2.Data));
        }

        [Theory]
        [InlineData(Index.Music)]
        public void TestEncodeReferenceTable(Index index)
        {
            var referenceTableFile = this.Fixture.RuneTek5Cache.GetFile<BinaryFile>(Index.ReferenceTables, (int)index);
            var referenceTable =  new ReferenceTableFile();
            referenceTable.FromBinaryFile(referenceTableFile);

            var encodedFile = referenceTable.ToBinaryFile();

            Assert.True(referenceTableFile.Data.SequenceEqual(encodedFile.Data));
        }
    }
}