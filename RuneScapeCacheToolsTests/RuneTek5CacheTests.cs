using System;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Xunit;
using Xunit.Abstractions;

namespace RuneScapeCacheToolsTests
{
    [Collection("TestCache")]
    public class RuneTek5CacheTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        private readonly RuneTek5Cache _cache;

        public RuneTek5CacheTests(ITestOutputHelper output)
        {
            _output = output;

            _cache = new RuneTek5Cache("TestCache");
        }

        /// <summary>
        /// Test for a file that exists, an archive file that exists and a file that doesn't exist.
        /// </summary>
        [Fact]
        public void TestGetFile()
        {
            var file = _cache.GetFile(12, 3);

            var fileData = file.Data;

            Assert.True(fileData.Length > 0, "File's data is empty.");

            var archiveFile = _cache.GetFile(17, 5);

            var archiveEntry = archiveFile.Entries[255];

            Assert.True(archiveEntry.Length > 0, "Archive entry's data is empty.");

            try
            {
                _cache.GetFile(40, 30);

                Assert.True(false, "Cache returned a file that shouldn't exist.");
            }
            catch (CacheException exception)
            {
                Assert.True(exception.Message.Contains("incomplete"), "Non-existent file cache exception had the wrong message.");
            }
        }

        public void Dispose()
        {
            _cache?.Dispose();
        }
    }
}