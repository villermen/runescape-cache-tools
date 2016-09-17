using System;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Audio;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace RuneScapeCacheToolsTests
{
    public class SoundtrackTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        private readonly RuneTek5Cache _cache;

        private readonly Soundtrack _soundtrack;

        public SoundtrackTests(ITestOutputHelper output)
        {
            _output = output;

            _cache = new RuneTek5Cache("TestData/");
            _soundtrack = new Soundtrack(_cache);
        }

        /// <summary>
        /// Archive 40 must have entries, and archive 17 must contain entry 5.
        /// 
        /// Checks the reference tables to see if they denote that the files exist.
        /// </summary>
        [Fact]
        public void TestReferenceTables()
        {
            var referenceTable40 = _cache.GetReferenceTable(40);

            var referenceTable40EntryCount = referenceTable40.Entries.Count;

            _output.WriteLine($"Index 40 item count: {referenceTable40EntryCount}");

            var referenceTable17 = _cache.GetReferenceTable(17);

            Assert.True(referenceTable17.Entries.ContainsKey(5));
        }

        /// <summary>
        /// The jaga files in the reference table for index 40 must have versions, so that they can be checked for changes.
        /// 
        /// Checks the first jaga file for a positive version.
        /// </summary>
        [Fact]
        public void TestJagaFileVersion()
        {
            var firstJagaFileId = _soundtrack.GetTrackNames().First().Key;

            _output.WriteLine($"First Jaga file id: {firstJagaFileId}");

            var referenceTable40 = _cache.GetReferenceTable(40);

            var firstJagaFileVersion = referenceTable40.Entries[firstJagaFileId].Version;

            _output.WriteLine($"First Jaga file version: {firstJagaFileVersion}");

            Assert.True(firstJagaFileVersion > 0);
        }

        public void Dispose()
        {
            _cache?.Dispose();
        }
    }
}
