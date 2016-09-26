using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache;
using Xunit;
using Xunit.Abstractions;

namespace RuneScapeCacheToolsTests
{
    [Collection("TestCache")]
    public class RuneTek5CacheTests
    {
        private readonly ITestOutputHelper _output;

        private readonly CacheFixture _fixture;

        public RuneTek5CacheTests(ITestOutputHelper output, CacheFixture fixture)
        {
            _output = output;

            _fixture = fixture;
        }

        /// <summary>
        /// Test for a file that exists, an archive file that exists and a file that doesn't exist.
        /// </summary>
        [Fact]
        public void TestGetFile()
        {
            var file = _fixture.RuneTek5Cache.GetFile(12, 3);

            var fileData = file.Data;

            Assert.True(fileData.Length > 0, "File's data is empty.");

            var archiveFile = _fixture.RuneTek5Cache.GetFile(17, 5);

            var archiveEntry = archiveFile.Entries[255];

            Assert.True(archiveEntry.Length > 0, "Archive entry's data is empty.");

            try
            {
                _fixture.RuneTek5Cache.GetFile(40, 30);

                Assert.True(false, "Cache returned a file that shouldn't exist.");
            }
            catch (CacheException exception)
            {
                Assert.True(exception.Message.Contains("incomplete"), "Non-existent file cache exception had the wrong message.");
            }
        }
    }
}