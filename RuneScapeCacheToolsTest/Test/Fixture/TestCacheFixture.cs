using System;
using System.Collections.Generic;
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
        public NxtClientCache NxtClientCache { get; }
        public DownloaderCache DownloaderCache { get; }
        public FlatFileCache FlatFileCache { get; }
        public readonly Dictionary<Type, ICache> Caches = new Dictionary<Type, ICache>();

        public SoundtrackExtractor SoundtrackExtractor { get; }

        public TestCacheFixture()
        {
            SQLitePCL.Batteries.Init();

            this.JavaClientCache = new JavaClientCache("testcache/java", false);
            this.Caches[typeof(JavaClientCache)] = this.JavaClientCache;

            this.NxtClientCache = new NxtClientCache("testcache/nxt", false);
            this.Caches[typeof(NxtClientCache)] = this.NxtClientCache;

            this.DownloaderCache = new DownloaderCache();
            this.Caches[typeof(DownloaderCache)] = this.DownloaderCache;

            this.FlatFileCache = new FlatFileCache("testcache/file");
            this.Caches[typeof(FlatFileCache)] = this.FlatFileCache;

            this.SoundtrackExtractor = new SoundtrackExtractor(this.JavaClientCache, "soundtrack");
        }

        public void Dispose()
        {
            this.JavaClientCache.Dispose();
            this.NxtClientCache.Dispose();
            this.DownloaderCache.Dispose();
            this.FlatFileCache.Dispose();
        }
    }
}
