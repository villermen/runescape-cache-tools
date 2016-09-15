using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Audio;

namespace RuneScapeCacheToolsTests
{
    [TestClass]
    public class SoundtrackTests
    {
        private RuneTek5Cache _cache;
        private Soundtrack _soundtrack;

        [TestInitialize]
        public void TestInitialize()
        {
            // TODO: Use test resource file
            _cache = new RuneTek5Cache();
            _soundtrack = new Soundtrack(_cache);
        }

        /// <summary>
        /// Archive 40 must have entries, and archive 17 must contain entry 5.
        /// 
        /// Checks the reference tables to see if they denote that the files exist.
        /// </summary>
        [TestMethod]
        public void TestReferenceTables()
        {
            var referenceTable40 = _cache.GetReferenceTable(40);

            var referenceTable40EntryCount = referenceTable40.Entries.Count;

            Console.WriteLine($"Index 40 item count: {referenceTable40EntryCount}");

            var referenceTable17 = _cache.GetReferenceTable(17);

            Assert.IsTrue(referenceTable17.Entries.ContainsKey(5));
        }

        /// <summary>
        /// The jaga files in the reference table for index 40 must have versions, so that they can be checked for changes.
        /// 
        /// Checks the first jaga file for a positive version.
        /// </summary>
        [TestMethod]
        public void TestJagaFileVersion()
        {
            var firstJagaFileId = _soundtrack.GetTrackNames().First().Key;

            Console.WriteLine($"First Jaga file id: {firstJagaFileId}");

            var referenceTable40 = _cache.GetReferenceTable(40);

            var firstJagaFileVersion = referenceTable40.Entries[firstJagaFileId].Version;

            Console.WriteLine($"First Jaga file version: {firstJagaFileVersion}");

            Assert.IsTrue(firstJagaFileVersion > 0);
        }
    }
}
