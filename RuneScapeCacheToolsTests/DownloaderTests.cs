using System;
using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Download;
using Xunit;
using Xunit.Abstractions;

namespace RuneScapeCacheToolsTests
{
    [Collection("TestCache")]
    public class DownloaderTests : IDisposable
    {
        private ITestOutputHelper Output { get; }
        private CacheFixture Fixture { get; }

        public DownloaderTests(ITestOutputHelper output, CacheFixture fixture)
        {
            Output = output;
            Fixture = fixture;
        }

        [Fact(Skip = "Not implemented")]
        public void TestDownloadFile()
        {
            Fixture.Downloader.DownloadFile(12, 3);
            // TODO: HTTP worker or something similar for
            // Fixture.Downloader.DownloadFile(40, 30468);

            // TODO: Verify result
        }

        [Fact(Skip = "Not implemented")]
        public void TestDownloadFileEntries()
        {
            var archiveFile = Fixture.Downloader.DownloadFile(17, 5);

            Assert.True(archiveFile.Entries.Length == 256);
        }

        [Fact]
        public void TestDownloadReferenceTable()
        {
            var referenceTable12 = Fixture.Downloader.DownloadReferenceTable(12);

            var rawReferenceTable = Fixture.Downloader.DownloadFile(RuneTek5Cache.MetadataIndexId, 17);

            var referenceTable = Fixture.Downloader.DownloadReferenceTable(17);

            Output.WriteLine($"Files in reference table for index 17: {referenceTable.Files.Count}.");

            Assert.True(referenceTable.Files.Count == 46);
        }

        [Fact(Skip = "Not implemented")]
        public void TestDownloadReferenceTableReferenceTable()
        {
            Fixture.Downloader.DownloadReferenceTable(RuneTek5Cache.MetadataIndexId);

            //buffer.position(5);
            //entryCount = buffer.get() & 0xff;
            //entries = new Entry[entryCount];

            ////System.out.println("#,crc,version,files,size");
            //for (int i = 0; i < entryCount; i++)
            //{
            //    Entry entry = entries[i] = new ReferenceTableFile();
            //    entry.crc = buffer.getInt();
            //    entry.version = buffer.getInt();
            //    int files = buffer.getInt();
            //    int size = buffer.getInt();
            //    entry.digest = new byte[64];
            //    buffer.get(entry.digest);
            //    //System.out.println(i + ":" + entry.crc + "," + entry.version + "," + files + "," + size);
            //}
        }

        public void Dispose()
        {
        }
    }
}
