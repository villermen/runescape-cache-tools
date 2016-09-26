namespace RuneScapeCacheToolsTests
{
    using Xunit;

    [CollectionDefinition("TestCache")]
    public class CacheCollection : ICollectionFixture<CacheFixture>
    {
    }
}