using System;
using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Model;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Test.Fixture
{
    public class TestCacheConstructor : BaseTests
    {
        /// <summary>
        /// Downloads and stores files necessary to perform all tests.
        /// </summary>
        [Fact(Skip = "Only run manually to construct a new test cache")]
        public void ConstructTestCache()
        {
            var files = new List<Tuple<CacheIndex, int>>
            {
                new Tuple<CacheIndex, int>(CacheIndex.Enums, 5),
                new Tuple<CacheIndex, int>(CacheIndex.ClientScripts, 3),
                new Tuple<CacheIndex, int>(CacheIndex.LoadingSprites, 30462),
                new Tuple<CacheIndex, int>(CacheIndex.Models, 47000),
                new Tuple<CacheIndex, int>(CacheIndex.Enums, 23),
                new Tuple<CacheIndex, int>(CacheIndex.ItemDefinitions, 155),
                new Tuple<CacheIndex, int>(CacheIndex.ItemDefinitions, 5),
                new Tuple<CacheIndex, int>(CacheIndex.AnimationFrames, 0),

                // Soundscape
                new Tuple<CacheIndex, int>(CacheIndex.Music, 38900),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73450),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73451),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73452),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73453),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73454),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73455),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73456),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73457),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73458),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73459),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73460),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73461),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73462),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73463),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73464),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73465),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73466),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73467),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73468),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73469),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73470),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73471),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73472),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73473),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73474),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73475),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73476),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73477),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73478),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73479),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73480),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73481),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73482),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73483),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73484),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73485),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73486),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73487),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73488),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73489),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73490),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73491),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73492),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73493),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73494),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73495),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73496),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73497),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73498),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73499),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73500),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 73501),

                // Black Zabeth: LIVE!
                new Tuple<CacheIndex, int>(CacheIndex.Music, 4978),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45182),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45183),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45184),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45185),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45186),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45187),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45188),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45189),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45190),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45191),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45192),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45193),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45194),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45195),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45196),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45197),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45198),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45199),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45200),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45201),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45202),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45203),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45204),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45205),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45206),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45207),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45208),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45209),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45210),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45211),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45212),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45213),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45214),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45215),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45216),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45217),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45218),
                new Tuple<CacheIndex, int>(CacheIndex.Music, 45219)
            };

            if (Directory.Exists("generated"))
            {
                Directory.Delete("generated", true);
            }

            var expectedFileSizes = new Queue<int>();

            // Download and write the files.
            {
                using var downloaderCache = new DownloaderCache();
                using var flatFileCache = new FlatFileCache("generated/file");
                using var javaCache = new JavaClientCache("generated/java", false);

                foreach (var fileTuple in files)
                {
                    var file = downloaderCache.GetFile(fileTuple.Item1, fileTuple.Item2);

                    flatFileCache.PutFile(fileTuple.Item1, fileTuple.Item2, file);
                    javaCache.PutFile(fileTuple.Item1, fileTuple.Item2, file);

                    expectedFileSizes.Enqueue(file.Data.Length);
                }
            }

            // Verify that the files are now obtainable and unchanged.
            {
                using var flatFileCache = new FlatFileCache("generated/file");
                using var javaCache = new JavaClientCache("generated/java");

                foreach (var fileTuple in files)
                {
                    var expectedFileSize = expectedFileSizes.Dequeue();

                    var flatFile = flatFileCache.GetFile(fileTuple.Item1, fileTuple.Item2);
                    Assert.Equal(expectedFileSize, flatFile.Data.Length);

                    var javaFile = javaCache.GetFile(fileTuple.Item1, fileTuple.Item2);
                    Assert.Equal(expectedFileSize, javaFile.Data.Length);
                }
            }
        }
    }
}
