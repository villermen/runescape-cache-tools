using System;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.Test.Fixture
{
    /// <summary>
    /// Provides cache related properties that can be re-used among tests.
    /// </summary>
    public class TestCacheFixture : IDisposable
    {
        public JavaClientCache JavaClientCache { get; }
        public DownloaderCache DownloaderCache { get; }
        public FlatFileCache FlatFileCache { get; }

        public SoundtrackExtractor SoundtrackExtractor { get; }

        public TestCacheFixture()
        {
            this.JavaClientCache = new JavaClientCache("testcache/java", false);
            this.DownloaderCache = new DownloaderCache();
            this.FlatFileCache = new FlatFileCache("testcache/file");

            this.SoundtrackExtractor = new SoundtrackExtractor(this.JavaClientCache, "soundtrack");
        }

        public ICache GetCache(Type cacheType)
        {
            if (cacheType == typeof(JavaClientCache))
            {
                return this.JavaClientCache;
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
            this.JavaClientCache.Dispose();
            this.DownloaderCache.Dispose();
            this.FlatFileCache.Dispose();
        }
    }
}
