namespace Villermen.RuneScapeCacheTools.Cache.CacheFile
{
    using System;

    /// <summary>
    /// An exception indicating that a file can not be retrieved from the cache.
    /// </summary>
    public class CacheFileNotFoundException : CacheException
    {
        public CacheFileNotFoundException(string message)
            : base(message)
        {
        }

        public CacheFileNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}