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
            RuneTek5Cache = new RuneTek5Cache("TestCache");
            Soundtrack = new Soundtrack(Cache);
            Downloader = new CacheDownloader();

            CreateTestCache();
        }

        public CacheBase Cache => RuneTek5Cache;

        public RuneTek5Cache RuneTek5Cache { get; }

        public Soundtrack Soundtrack { get; }

        private CacheDownloader _cacheDownloader;

        /// <summary>
        /// Returns a downloader which is already connected.
        /// Connecting beforehand is done to reduce network overhead each time a test is started.
        /// </summary>
        public CacheDownloader Downloader
        {
            get
            {
                if (!_cacheDownloader.TcpConnected)
                {
                    _cacheDownloader.TcpConnect();
                }

                return _cacheDownloader;
            }
            private set { _cacheDownloader = value; }
        }

        /// <summary>
        /// Generates a test cache by downloading just enough files to perform the basic testing.
        /// </summary>
        private void CreateTestCache()
        {
            // TODO: Actually download test files into cache in here
        }

        public void Dispose()
        {
            Cache.Dispose();
        }
    }
}