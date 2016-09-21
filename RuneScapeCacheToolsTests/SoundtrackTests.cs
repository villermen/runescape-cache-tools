using System;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Audio;
using Xunit;
using Xunit.Abstractions;

namespace RuneScapeCacheToolsTests
{
    [Collection("TestCache")]
    public class SoundtrackTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
       
        private readonly RuneTek5Cache _cache;

        private readonly Soundtrack _soundtrack;

        public SoundtrackTests(ITestOutputHelper output)
        {
            _output = output;
            _cache = new RuneTek5Cache("TestCache");
            _soundtrack = new Soundtrack(_cache);
        }

        /// <summary>
        /// Soundtrack names must be retrievable.
        /// 
        /// Checks if GetTrackNames returns a track with name "Soundscape".
        /// </summary>
        [Fact]
        public void TestGetTrackNames()
        {
            var trackNames = _soundtrack.GetTrackNames();

            _output.WriteLine($"Amount of track names: {trackNames.Count}");

            Assert.True(trackNames.Any(trackNamePair => trackNamePair.Value == "Soundscape"));
        }

        // [Fact] Removed because test data is not customized yet
        public void TestExportTracksAsync()
        {
            var startTime = DateTime.UtcNow;
            _soundtrack.ExportTracksAsync(true).Wait();

            var filename = "output/soundtrack/Soundscape.ogg";

            // Verify that Soundscape.ogg has been created
            Assert.True(File.Exists(filename));

            // Verify that it has been created during this test
            Assert.True(File.GetCreationTimeUtc(filename) >= startTime);
        }

        // TODO: Check FILE_VERSION comment

        public void Dispose()
        {
            _cache?.Dispose();
        }
    }
}
