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

        public DownloaderTests(ITestOutputHelper output)
        {
            _output = output;
            _downloader = new Downloader();
        }

        [Fact]
        public void TestUnknown()
        {
            _downloader.Connect();
        }

        public void Dispose()
        {
        }
    }
}
