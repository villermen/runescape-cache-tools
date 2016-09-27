using System;
using RuneScapeCacheToolsTests.Fixtures;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Downloader;
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
            Fixture.Downloader.DownloadFile(17, 5);
            Fixture.Downloader.DownloadReferenceTable(17);
            Fixture.Downloader.DownloadFile(12, 423);

            // TODO: HTTP worker or something similar for
            // Fixture.Downloader.DownloadFile(40, 30468);
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
