using System.Linq;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Xunit;

namespace RuneScapeCacheToolsTests
{
    public class RuneTek5CacheTests
    {
        private RuneTek5Cache _cache;

        [Fact]
        public void TestInitialize()
        {
            // TODO: Use test resource file
            _cache = new RuneTek5Cache();
        }
    }
}