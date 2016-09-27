using System;
using Villermen.RuneScapeCacheTools.Audio;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Download;

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
            Downloader = new Downloader(Cache);

            CreateTestCache();
        }

        public CacheBase Cache => RuneTek5Cache;

        public RuneTek5Cache RuneTek5Cache { get; }

        public Soundtrack Soundtrack { get; }

        private Downloader _downloader;

        /// <summary>
        /// Returns a downloader which is already connected.
        /// Connecting beforehand is done to reduce network overhead each time a test is started.
        /// </summary>
        public Downloader Downloader
        {
            get
            {
                if (!_downloader.Connected)
                {
                    _downloader.Connect();
                }

                return _downloader;
            }
            private set { _downloader = value; }
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