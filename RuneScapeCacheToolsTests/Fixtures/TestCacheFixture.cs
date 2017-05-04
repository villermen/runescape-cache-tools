using System;
using Villermen.RuneScapeCacheTools;
using Villermen.RuneScapeCacheTools.Cache.Downloader;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;

namespace RuneScapeCacheToolsTests.Fixtures
{
    /// <summary>
    /// Provides cache related properties that can be re-used among tests.
    /// </summary>
    public class TestCacheFixture : IDisposable
    {
        public TestCacheFixture()
        {
            this.RuneTek5Cache = new RuneTek5Cache("testdata/cache", false);
            this.Soundtrack = new Soundtrack(this.RuneTek5Cache);

            this.Downloader = new Villermen.RuneScapeCacheTools.Cache.Downloader.DownloaderCache();
        }

        public RuneTek5Cache RuneTek5Cache { get; }

        public Soundtrack Soundtrack { get; }

        public Villermen.RuneScapeCacheTools.Cache.Downloader.DownloaderCache Downloader { get; }

        public void Dispose()
        {
            this.RuneTek5Cache.Dispose();
        }
    }
}