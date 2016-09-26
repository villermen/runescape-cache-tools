using System;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Audio;

namespace RuneScapeCacheToolsTests
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

            DownloadTestFiles();
        }

        public CacheBase Cache => RuneTek5Cache;

        public RuneTek5Cache RuneTek5Cache { get; }

        public Soundtrack Soundtrack { get; }

        private void DownloadTestFiles()
        {
            // TODO: Actually download test files into cache in here
        }

        public void Dispose()
        {
            Cache.Dispose();
        }
    }
}