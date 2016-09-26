using Xunit;

namespace RuneScapeCacheToolsTests.Fixtures
{
    [CollectionDefinition("TestCache")]
    public class CacheCollection : ICollectionFixture<CacheFixture>
    {
    }
}