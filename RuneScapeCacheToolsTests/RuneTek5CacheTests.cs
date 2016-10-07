using System;
using System.Collections.Generic;
using System.IO;
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
            Output = output;
            Fixture = fixture;
        }

        /// <summary>
        /// Test for a file that exists, an archive file that exists and a file that doesn't exist.
        /// </summary>
        [Fact]
        public void TestGetFile()
        {
            var file = Fixture.RuneTek5Cache.GetFile(Index.ClientScripts, 3);

            var fileData = file.Data;

            Assert.True(fileData.Length > 0, "File's data is empty.");

            var archiveFile = Fixture.RuneTek5Cache.GetFile(Index.Enums, 5);

            var archiveEntry = archiveFile.Entries[255];

            Assert.True(archiveEntry.Length > 0, "Archive entry's data is empty.");

            try
            {
                Fixture.RuneTek5Cache.GetFile(Index.Music, 30);

                Assert.True(false, "Cache returned a file that shouldn't exist.");
            }
            catch (CacheException exception)
            {
                Assert.True(exception.Message.Contains("incomplete"), "Non-existent file cache exception had the wrong message.");
            }
        }

        [Theory]
        [InlineData(Index.Music)]
        [InlineData(Index.Enums)]
        [InlineData(Index.ClientScripts)]
        public void TestGetReferenceTable(Index index)
        {
            var referenceTable = Fixture.RuneTek5Cache.GetReferenceTable(index);
        }
    }
}