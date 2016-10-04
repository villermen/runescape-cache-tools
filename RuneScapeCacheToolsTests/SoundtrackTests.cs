using System;
using System.IO;
using System.Linq;
using RuneScapeCacheToolsTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace RuneScapeCacheToolsTests
{
    [Collection("TestCache")]
    public class SoundtrackTests
    {
        private ITestOutputHelper Output { get; }

        private CacheFixture Fixture { get; }

        public SoundtrackTests(ITestOutputHelper output, CacheFixture fixture)
        {
            Output = output;
            Fixture = fixture;
        }

        /// <summary>
        /// Soundtrack names must be retrievable.
        /// 
        /// Checks if GetTrackNames returns a track with name "Soundscape".
        /// </summary>
        [Fact]
        public void TestGetTrackNames()
        {
            var trackNames = Fixture.Soundtrack.GetTrackNames();

            Output.WriteLine($"Amount of track names: {trackNames.Count}");

            Assert.True(trackNames.Any(trackNamePair => trackNamePair.Value == "Soundscape"), "\"Soundscape\" did not occur in the list of track names.");
        }

        [Fact]
        public void TestExportTracksAsync()
        {
            var startTime = DateTime.UtcNow;
            Fixture.Soundtrack.ExportTracksAsync(true, "soundscape").Wait();

            const string expectedOutputPath = "output/soundtrack/Soundscape.ogg";

            // Verify that Soundscape.ogg has been created
            Assert.True(File.Exists(expectedOutputPath), "Soundscape.ogg should've been created during extraction.");

            // Verify that it has been created during this test
            var modifiedTime = File.GetLastWriteTimeUtc(expectedOutputPath);
            Assert.True(modifiedTime >= startTime, "Soundscape.ogg's modiied time was not updated during extraction (so probably was not extracted)."); // TODO: I believe oggCat does not update mtime weirdly enough

            var version = Fixture.Soundtrack.GetVersionFromExportedTrackFile("output/soundtrack/Soundscape.ogg");

            // TODO: Make this happen
            // Assert.True(version > 0, "Version of Soundscape.ogg was 0.");
        }
    }
}
