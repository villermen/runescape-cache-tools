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
        [InlineData(Index.Music)]
        [InlineData(Index.Enums)]
        [InlineData(Index.ClientScripts)]
        public void TestGetReferenceTable(Index index)
        {
            var referenceTable = this.Fixture.RuneTek5Cache.GetReferenceTable(index);
        }

        [Theory(Skip = "Not fully implemented yet")]
        [InlineData(Index.Models, 47000)] // Gzip TODO: Takes 20 seconds during reference table writing, what is going on?
        [InlineData(Index.Enums, 23)] // Bzip2, entries
        public void TestWriteCacheFile(Index index, int fileId)
        {
            var file1 = this.Fixture.RuneTek5Cache.GetFile(index, fileId);

            this.Fixture.RuneTek5Cache.PutFile(file1);

            // Refresh the cache to make sure everything read after this point is freshly obtained
            this.Fixture.RuneTek5Cache.Refresh();

            var file2 = this.Fixture.RuneTek5Cache.GetFile(index, fileId);

            // Compare the info objects
            Assert.Equal(file1.Info, file2.Info);

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