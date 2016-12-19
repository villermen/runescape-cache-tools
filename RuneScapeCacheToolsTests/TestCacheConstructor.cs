using System;
using System.Collections.Generic;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.CacheFile;
using Villermen.RuneScapeCacheTools.Cache.Downloader;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Xunit;

namespace RuneScapeCacheToolsTests
{
    public class TestCacheConstructor
    {
        /// <summary>
        /// Only used manually to construct a new test cache.
        /// 
        /// Downloads and stores files necessary to perform all tests.
        /// </summary>
        [Fact(Skip = "Only run manually to construct a new test cache")]
        public void ConstructTestcache()
        {
            var files = new List<Tuple<Index, int>>
            {
                new Tuple<Index, int>(Index.Enums, 5),
                new Tuple<Index, int>(Index.ClientScripts, 3),

            };

            var cache = new RuneTek5Cache("newCache", false);
            var downloader = new CacheDownloader();

            foreach (var fileTuple in files)
            {
                cache.PutFile(downloader.GetFile<DataCacheFile>(fileTuple.Item1, fileTuple.Item2));
            }
        }
    }
}