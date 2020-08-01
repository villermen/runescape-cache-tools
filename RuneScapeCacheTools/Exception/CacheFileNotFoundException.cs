using System;

namespace Villermen.RuneScapeCacheTools.Exception
{
    [Serializable]
    public class CacheFileNotFoundException : CacheException
    {
        public CacheFileNotFoundException()
        {
        }

        public CacheFileNotFoundException(string message) : base(message)
        {
        }

        public CacheFileNotFoundException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
