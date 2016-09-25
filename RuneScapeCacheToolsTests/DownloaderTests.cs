using System;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Audio;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5.Downloader;
using Xunit;
using Xunit.Abstractions;

namespace RuneScapeCacheToolsTests
{
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
