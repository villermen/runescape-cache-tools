using System;
using Villermen.RuneScapeCacheTools;
using Villermen.RuneScapeCacheTools.Cache;
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
        public DownloaderCache DownloaderCache { get; }
        public FlatFileCache FlatFileCache { get; }

        public Soundtrack Soundtrack { get; }

        public TestCacheFixture()
        {
            this.RuneTek5Cache = new RuneTek5Cache("testdata/runetek5", false);
            this.DownloaderCache = new DownloaderCache();
            this.FlatFileCache = new FlatFileCache("testdata/flatfile");

            this.Soundtrack = new Soundtrack(this.RuneTek5Cache, "soundtrack");
        }

        public BaseCache GetCache(Type cacheType)
        {
            if (cacheType == typeof(RuneTek5Cache))
            {
                return this.RuneTek5Cache;
            }
            if (cacheType == typeof(FlatFileCache))
            {
                return this.FlatFileCache;
            }
            if (cacheType == typeof(DownloaderCache))
            {
                return this.DownloaderCache;
            }

            throw new ArgumentException("Invalid cache type requested.");
        }

        public void Dispose()
        {
            this.RuneTek5Cache.Dispose();
            this.DownloaderCache.Dispose();
            this.FlatFileCache.Dispose();
        }
    }
}