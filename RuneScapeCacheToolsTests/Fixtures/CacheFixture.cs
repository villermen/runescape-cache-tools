using System;
using Villermen.RuneScapeCacheTools.Audio;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.Downloader;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;

namespace RuneScapeCacheToolsTests.Fixtures
{
    /// <summary>
    /// Provides cache related properties that can be re-used among tests.
    /// </summary>
    public class CacheFixture : IDisposable
    {
        public CacheFixture()
        {
            this.RuneTek5Cache = new RuneTek5Cache("testdata/cache");
            this.Downloader = new CacheDownloader();
            this.Soundtrack = new Soundtrack(this.Downloader);

            this.CreateTestCache();
        }

        public CacheBase Cache => this.RuneTek5Cache;

        public RuneTek5Cache RuneTek5Cache { get; }

        public Soundtrack Soundtrack { get; }

        public CacheDownloader Downloader { get; }

        /// <summary>
        /// Generates a test cache by downloading just enough files to perform the basic testing.
        /// </summary>
        private void CreateTestCache()
        {
            // TODO: Actually download test files into cache in here
        }

        public void Dispose()
        {
            this.Cache.Dispose();
        }
    }
}