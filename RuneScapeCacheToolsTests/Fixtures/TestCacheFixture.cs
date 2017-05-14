using System;
using Villermen.RuneScapeCacheTools;
using Villermen.RuneScapeCacheTools.Cache.Downloader;
using Villermen.RuneScapeCacheTools.Cache.FlatFile;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;

namespace RuneScapeCacheToolsTests.Fixtures
{
    /// <summary>
    /// Provides cache related properties that can be re-used among tests.
    /// </summary>
    public class TestCacheFixture : IDisposable
    {
        public RuneTek5Cache RuneTek5Cache { get; }

        public Soundtrack Soundtrack { get; }

        public DownloaderCache Downloader { get; }

        public FlatFileCache FlatFileCache { get; }

        public TestCacheFixture()
        {
            this.RuneTek5Cache = new RuneTek5Cache("testdata/runetek5", false);
            this.Soundtrack = new Soundtrack(this.RuneTek5Cache, "soundtrack");
            this.Downloader = new DownloaderCache();
            this.FlatFileCache = new FlatFileCache("testdata/flatfile");
        }

        public void Dispose()
        {
            this.RuneTek5Cache.Dispose();
        }
    }
}