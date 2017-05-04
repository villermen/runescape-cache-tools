using Xunit;

namespace RuneScapeCacheToolsTests.Fixtures
{
    [CollectionDefinition(TestCacheCollection.Name)]
    public class TestCacheCollection : ICollectionFixture<TestCacheFixture>
    {
        public const string Name = "TestCache";
    }
}