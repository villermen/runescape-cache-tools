using Xunit;

namespace Villermen.RuneScapeCacheTools.Test.Fixture
{
    [CollectionDefinition(TestCacheCollection.Name)]
    public class TestCacheCollection : ICollectionFixture<TestCacheFixture>
    {
        public const string Name = "TestCache";
    }
}
