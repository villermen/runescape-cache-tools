using System;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Xunit;
using Xunit.Abstractions;

namespace RuneScapeCacheToolsTests
{
    public class RuneTek5CacheTests : IDisposable
    {
        private ITestOutputHelper _output;

        private RuneTek5Cache _cache;

        public RuneTek5CacheTests(ITestOutputHelper output)
        {
            _output = output;

            _cache = new RuneTek5Cache("TestData");
        }

        [Fact]
        public void TestGetReferenceTable()
        {
            //var referenceTable = _cache.GetReferenceTable(40);

            //Assert.True(referenceTable.Entries.Count == 60391);

            //_output.WriteLine(referenceTable.Entries.Count.ToString());
        }

        /// <summary>
        /// Test for a file that exists, an archive file that exists and a file that doesn't exist.
        /// </summary>
        [Fact]
        public void TestGetFile()
        {
            var file = _cache.GetFile(12, 3);

            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _cache?.Dispose();
        }
    }
}