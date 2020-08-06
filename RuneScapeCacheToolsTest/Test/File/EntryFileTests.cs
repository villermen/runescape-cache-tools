using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Test.Fixture;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Test.File
{
    [Collection(TestCacheCollection.Name)]
    public class EntryFileTests : BaseTests
    {
        private TestCacheFixture Fixture { get; }

        public EntryFileTests(TestCacheFixture fixture)
        {
            this.Fixture = fixture;
        }

        [Fact]
        public void TestEncodeDecode()
        {
            var entry0 = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var entry1 = new byte[] { 0x05, 0x06, 0x07, 0x08 };

            var entryFile = new RuneScapeCacheTools.File.EntryFile();
            entryFile.Entries[0] = entry0;
            entryFile.Entries[1] = entry1;

            var entryFileData = entryFile.Encode(out var entryIds);
            Assert.Equal(new [] { 0, 1 }, entryIds);

            var decodedEntryFile = RuneScapeCacheTools.File.EntryFile.Decode(entryFileData, entryIds);

            Assert.Equal(entry0, decodedEntryFile.Entries[0]);
            Assert.Equal(entry1, decodedEntryFile.Entries[1]);
        }

        [Fact]
        public void TestEncodeDecodeNonConsecutive()
        {
            var entry5 = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var entry10 = new byte[] { 0x05, 0x06, 0x07, 0x08 };

            var entryFile = new RuneScapeCacheTools.File.EntryFile();
            entryFile.Entries[5] = entry5;
            entryFile.Entries[10] = entry10;

            var entryFileData = entryFile.Encode(out var entryIds);
            Assert.Equal(new [] { 5, 10 }, entryIds);

            var decodedEntryFile = RuneScapeCacheTools.File.EntryFile.Decode(entryFileData, entryIds);
            Assert.Equal(entry5, decodedEntryFile.Entries[5]);
            Assert.Equal(entry10, decodedEntryFile.Entries[10]);
        }

        [Fact]
        public void TestWithCacheFile()
        {
            const int entryCount = 256;

            var entryCacheFile = this.Fixture.JavaClientCache.GetFile(CacheIndex.ItemDefinitions, 155);
            Assert.Equal(entryCount, entryCacheFile.Info.Entries.Count);

            var entryFile = RuneScapeCacheTools.File.EntryFile.DecodeFromCacheFile(entryCacheFile);
            Assert.Equal(entryCount, entryFile.Entries.Count);

            var encodedEntryCacheFile = entryFile.EncodeToCacheFile();
            Assert.Equal(entryCount, entryFile.Entries.Count);

            Assert.Equal(entryCacheFile.Data, encodedEntryCacheFile.Data);
        }
    }
}
