using System;
using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Cache;

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
        public void ConstructTestCache()
        {
            var files = new List<Tuple<Index, int>>
            {
                new Tuple<Index, int>(Index.Enums, 5),
                new Tuple<Index, int>(Index.ClientScripts, 3),
                new Tuple<Index, int>(Index.LoadingSprites, 8501),
                new Tuple<Index, int>(Index.Models, 47000),
                new Tuple<Index, int>(Index.Enums, 23),

                // Soundscape
                new Tuple<Index, int>(Index.Music, 38900),
                new Tuple<Index, int>(Index.Music, 73450),
                new Tuple<Index, int>(Index.Music, 73451),
                new Tuple<Index, int>(Index.Music, 73452),
                new Tuple<Index, int>(Index.Music, 73453),
                new Tuple<Index, int>(Index.Music, 73454),
                new Tuple<Index, int>(Index.Music, 73455),
                new Tuple<Index, int>(Index.Music, 73456),
                new Tuple<Index, int>(Index.Music, 73457),
                new Tuple<Index, int>(Index.Music, 73458),
                new Tuple<Index, int>(Index.Music, 73459),
                new Tuple<Index, int>(Index.Music, 73460),
                new Tuple<Index, int>(Index.Music, 73461),
                new Tuple<Index, int>(Index.Music, 73462),
                new Tuple<Index, int>(Index.Music, 73463),
                new Tuple<Index, int>(Index.Music, 73464),
                new Tuple<Index, int>(Index.Music, 73465),
                new Tuple<Index, int>(Index.Music, 73466),
                new Tuple<Index, int>(Index.Music, 73467),
                new Tuple<Index, int>(Index.Music, 73468),
                new Tuple<Index, int>(Index.Music, 73469),
                new Tuple<Index, int>(Index.Music, 73470),
                new Tuple<Index, int>(Index.Music, 73471),
                new Tuple<Index, int>(Index.Music, 73472),
                new Tuple<Index, int>(Index.Music, 73473),
                new Tuple<Index, int>(Index.Music, 73474),
                new Tuple<Index, int>(Index.Music, 73475),
                new Tuple<Index, int>(Index.Music, 73476),
                new Tuple<Index, int>(Index.Music, 73477),
                new Tuple<Index, int>(Index.Music, 73478),
                new Tuple<Index, int>(Index.Music, 73479),
                new Tuple<Index, int>(Index.Music, 73480),
                new Tuple<Index, int>(Index.Music, 73481),
                new Tuple<Index, int>(Index.Music, 73482),
                new Tuple<Index, int>(Index.Music, 73483),
                new Tuple<Index, int>(Index.Music, 73484),
                new Tuple<Index, int>(Index.Music, 73485),
                new Tuple<Index, int>(Index.Music, 73486),
                new Tuple<Index, int>(Index.Music, 73487),
                new Tuple<Index, int>(Index.Music, 73488),
                new Tuple<Index, int>(Index.Music, 73489),
                new Tuple<Index, int>(Index.Music, 73490),
                new Tuple<Index, int>(Index.Music, 73491),
                new Tuple<Index, int>(Index.Music, 73492),
                new Tuple<Index, int>(Index.Music, 73493),
                new Tuple<Index, int>(Index.Music, 73494),
                new Tuple<Index, int>(Index.Music, 73495),
                new Tuple<Index, int>(Index.Music, 73496),
                new Tuple<Index, int>(Index.Music, 73497),
                new Tuple<Index, int>(Index.Music, 73498),
                new Tuple<Index, int>(Index.Music, 73499),
                new Tuple<Index, int>(Index.Music, 73500),
                new Tuple<Index, int>(Index.Music, 73501),

                // Black Zabeth: LIVE!
                new Tuple<Index, int>(Index.Music, 4978),
                new Tuple<Index, int>(Index.Music, 45182),
                new Tuple<Index, int>(Index.Music, 45183),
                new Tuple<Index, int>(Index.Music, 45184),
                new Tuple<Index, int>(Index.Music, 45185),
                new Tuple<Index, int>(Index.Music, 45186),
                new Tuple<Index, int>(Index.Music, 45187),
                new Tuple<Index, int>(Index.Music, 45188),
                new Tuple<Index, int>(Index.Music, 45189),
                new Tuple<Index, int>(Index.Music, 45190),
                new Tuple<Index, int>(Index.Music, 45191),
                new Tuple<Index, int>(Index.Music, 45192),
                new Tuple<Index, int>(Index.Music, 45193),
                new Tuple<Index, int>(Index.Music, 45194),
                new Tuple<Index, int>(Index.Music, 45195),
                new Tuple<Index, int>(Index.Music, 45196),
                new Tuple<Index, int>(Index.Music, 45197),
                new Tuple<Index, int>(Index.Music, 45198),
                new Tuple<Index, int>(Index.Music, 45199),
                new Tuple<Index, int>(Index.Music, 45200),
                new Tuple<Index, int>(Index.Music, 45201),
                new Tuple<Index, int>(Index.Music, 45202),
                new Tuple<Index, int>(Index.Music, 45203),
                new Tuple<Index, int>(Index.Music, 45204),
                new Tuple<Index, int>(Index.Music, 45205),
                new Tuple<Index, int>(Index.Music, 45206),
                new Tuple<Index, int>(Index.Music, 45207),
                new Tuple<Index, int>(Index.Music, 45208),
                new Tuple<Index, int>(Index.Music, 45209),
                new Tuple<Index, int>(Index.Music, 45210),
                new Tuple<Index, int>(Index.Music, 45211),
                new Tuple<Index, int>(Index.Music, 45212),
                new Tuple<Index, int>(Index.Music, 45213),
                new Tuple<Index, int>(Index.Music, 45214),
                new Tuple<Index, int>(Index.Music, 45215),
                new Tuple<Index, int>(Index.Music, 45216),
                new Tuple<Index, int>(Index.Music, 45217),
                new Tuple<Index, int>(Index.Music, 45218),
                new Tuple<Index, int>(Index.Music, 45219)
            };

            try
            {
                Directory.Delete("NewCache", true);
            }
            catch (DirectoryNotFoundException)
            {
            }

            using (var cache = new RuneTek5Cache("NewCache", false))
            using (var downloader = new DownloaderCache())
            {
                foreach (var fileTuple in files)
                {
                    var file = downloader.GetFile(fileTuple.Item1, fileTuple.Item2);
                    cache.PutFile(file);
                }

                // Verify that the files are now obtainable
                cache.Refresh();
                foreach (var fileTuple in files)
                {
                    cache.GetFile(fileTuple.Item1, fileTuple.Item2);
                }
            }
        }
    }
}