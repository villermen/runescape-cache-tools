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
            this.RuneTek5Cache = new RuneTek5Cache("testdata/cache", false);
            this.Soundtrack = new Soundtrack(this.RuneTek5Cache);

            this.Downloader = new CacheDownloader();
        }

        public RuneTek5Cache RuneTek5Cache { get; }

        public Soundtrack Soundtrack { get; }

        public CacheDownloader Downloader { get; }

        public void Dispose()
        {
            this.RuneTek5Cache.Dispose();
        }
    }
}