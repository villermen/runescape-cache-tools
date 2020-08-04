using System;

namespace Villermen.RuneScapeCacheTools.Exception
{
    [Serializable]
    public class CacheException : System.Exception
    {
        public CacheException()
        {
        }

        public CacheException(string message) : base(message)
        {
        }

        public CacheException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
