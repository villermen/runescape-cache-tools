using System;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Downloader;
using Xunit;
using Xunit.Abstractions;

namespace RuneScapeCacheToolsTests
{
    [Collection("TestCache")]
    public class DownloaderTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Downloader _downloader;
        private readonly CacheFixture _fixture;

        public DownloaderTests(ITestOutputHelper output, CacheFixture fixture)
        {
            _output = output;
            _fixture = fixture;

            _downloader = new Downloader(_fixture.Cache);
        }

        [Fact]
        public void TestGetKeyFromPage()
        {
            var key = _downloader.GetKeyFromPage();

            _output.WriteLine($"Key obtained from downloader: {key}");
        }

        [Fact]
        public void TestConnect()
        {
            _downloader.Connect();
        }

        public void Dispose()
        {
        }
    }
}
