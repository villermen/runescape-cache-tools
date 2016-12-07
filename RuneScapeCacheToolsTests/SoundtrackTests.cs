using System;
using System.IO;
using System.Linq;
using RuneScapeCacheToolsTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace RuneScapeCacheToolsTests
{
    using Villermen.RuneScapeCacheTools.Audio;

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

        [Theory]
        [InlineData("SoundScape", 15)]
        public void TestExtract(string trackName, int expectedVersion)
        {
            var startTime = DateTime.UtcNow;
            Fixture.Soundtrack.Extract(true, trackName);

            string expectedOutputPath = $"output/soundtrack/{trackName}.ogg";

            // Verify that Soundscape.ogg has been created
            Assert.True(File.Exists(expectedOutputPath), $"{trackName}.ogg should've been created during extraction.");

            // Verify that it has been created during this test
            var modifiedTime = File.GetLastWriteTimeUtc(expectedOutputPath);
            Assert.True(modifiedTime >= startTime, $"{trackName}.ogg's modified time was not updated during extraction (so probably was not extracted).");

            var version = Fixture.Soundtrack.GetVersionFromExportedTrackFile($"output/soundtrack/{trackName}.ogg");

            Assert.True(version == expectedVersion, "Version of Soundscape.ogg was incorrect ({version} instead of {expectedVersion}).");
        }
    }
}
