using System;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Xunit;

namespace RuneScapeCacheToolsTests
{
    public class FileStoreTests : IDisposable
    {
        private readonly FileStore store;

        public FileStoreTests()
        {
            this.store = new FileStore("store", false);
        }

        [Theory]
        [InlineData(1234)]
        [InlineData(65535)]
        [InlineData(65555)]
        [InlineData(1234)]
        public void TestReadback(int fileId)
        {
            // Create some random bytes
            var randomData = new byte[2456];
            new Random().NextBytes(randomData);

            this.store.WriteFileData(Index.EmptyUnknown1, fileId, randomData);
            var readData = this.store.ReadFileData(Index.EmptyUnknown1, fileId);

            Assert.True(readData.SequenceEqual(randomData), "Read data does not equal written data.");
        }

        public void Dispose()
        {
            this.store.Dispose();
        }
    }
}