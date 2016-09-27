using System;
using RuneScapeCacheToolsTests.Fixtures;
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

        [Fact]
        public void TestDownloadFile()
        {
            var file175 = Fixture.Downloader.DownloadFile(17, 5);

            // TODO: HTTP worker or something similar for
            // Fixture.Downloader.DownloadFile(40, 30468);
        }

        [Fact]
        public void TestDownloadReferenceTable()
        {
            Fixture.Downloader.DownloadReferenceTable(17);

            // TODO: This
            // Fixture.Downloader.DownloadReferenceTable(255);

            //buffer.position(5);
            //entryCount = buffer.get() & 0xff;
            //entries = new Entry[entryCount];

            ////System.out.println("#,crc,version,files,size");
            //for (int i = 0; i < entryCount; i++)
            //{
            //    Entry entry = entries[i] = new Entry();
            //    entry.crc = buffer.getInt();
            //    entry.version = buffer.getInt();
            //    int files = buffer.getInt();
            //    int size = buffer.getInt();
            //    entry.digest = new byte[64];
            //    buffer.get(entry.digest);
            //    //System.out.println(i + ":" + entry.crc + "," + entry.version + "," + files + "," + size);
            //}
        }

        [Fact]
        public void TestDownloadMetadataFileDirectly()
        {
            Assert.Throws<DownloaderException>(() => Fixture.Downloader.DownloadFile(255, 40));
        }

        public void Dispose()
        {
        }
    }
}
