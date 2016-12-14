using System;
using System.Collections.Generic;
using System.IO;
using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Xunit;
using Xunit.Abstractions;

namespace RuneScapeCacheToolsTests
{
    [Collection("TestCache")]
    public class RuneTek5CacheTests
    {
        private ITestOutputHelper Output { get; }

        private CacheFixture Fixture { get; }

        public RuneTek5CacheTests(ITestOutputHelper output, CacheFixture fixture)
        {
            this.Output = output;
            this.Fixture = fixture;
        }

        [Theory]
        [InlineData(Index.Music)]
        [InlineData(Index.Enums)]
        [InlineData(Index.ClientScripts)]
        public void TestGetReferenceTable(Index index)
        {
            var referenceTable = this.Fixture.RuneTek5Cache.GetReferenceTable(index);
        }

        [Theory(Skip = "Not yet finished")]
        [InlineData(Index.Models, 47000)] // Gzip
        [InlineData(Index.Enums, 23)] // Bzip2, entries
        public void TestWriteCacheFile(Index index, int fileId)
        {
            var file = this.Fixture.RuneTek5Cache.GetFile(index, fileId);

            this.Fixture.RuneTek5Cache.PutFile(file);
        }
    }
}