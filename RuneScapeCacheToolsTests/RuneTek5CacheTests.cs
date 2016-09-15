using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;

namespace RuneScapeCacheToolsTests
{
    [TestClass]
    public class RuneTek5CacheTests
    {
        private RuneTek5Cache _cache;

        [TestInitialize]
        public void TestInitialize()
        {
            // TODO: Use test resource file
            _cache = new RuneTek5Cache();
        }
    }
}