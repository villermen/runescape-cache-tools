using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace RuneScapeCacheToolsTests
{
    [Collection("TestCache")]
    public class SoundtrackTests
    {
        private readonly ITestOutputHelper _output;

        private readonly CacheFixture _fixture;

        public SoundtrackTests(ITestOutputHelper output, CacheFixture fixture)
        {
            _output = output;
            _fixture = fixture;
        }

        /// <summary>
        /// Soundtrack names must be retrievable.
        /// 
        /// Checks if GetTrackNames returns a track with name "Soundscape".
        /// </summary>
        [Fact]
        public void TestGetTrackNames()
        {
            var trackNames = _fixture.Soundtrack.GetTrackNames();

            _output.WriteLine($"Amount of track names: {trackNames.Count}");

            Assert.True(trackNames.Any(trackNamePair => trackNamePair.Value == "Soundscape"));
        }

        // [Fact] // Removed because test data is not customized yet
        public void TestExportTracksAsync()
        {
            var startTime = DateTime.UtcNow;
            _fixture.Soundtrack.ExportTracksAsync(true, "soundscape").Wait();

            const string expectedOutputPath = "output/soundtrack/Soundscape.ogg";

            // Verify that Soundscape.ogg has been created
            Assert.True(File.Exists(expectedOutputPath));

            // Verify that it has been created during this test
            var modifiedTime = File.GetLastWriteTimeUtc(expectedOutputPath);
            Assert.True(modifiedTime >= startTime);
        }

        // [Fact]
        public void TestGetVersionFromCombinedTrackFile()
        {
            // Enforce order of tests? Extract again?
            var version = _fixture.Soundtrack.GetVersionFromExportedTrackFile("output/soundtrack/Soundscape.ogg");
        }
    }
}
