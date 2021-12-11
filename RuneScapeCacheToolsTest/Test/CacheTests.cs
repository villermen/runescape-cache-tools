using Villermen.RuneScapeCacheTools.Test.Fixture;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Test
{
    [Collection(TestCacheCollection.Name)]
    public class DebugTests : BaseTests
    {
        private TestCacheFixture Fixture { get; }

        public DebugTests(TestCacheFixture fixture)
        {
            this.Fixture = fixture;
        }

        [Fact]
        public void TestDebug()
        {
        }
    }
}
