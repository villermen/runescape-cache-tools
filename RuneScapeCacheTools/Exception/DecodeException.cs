using System;

namespace Villermen.RuneScapeCacheTools.Exception
{
    [Serializable]
    public class DecodeException : CacheException
    {
        public DecodeException()
        {
        }

        public DecodeException(string message) : base(message)
        {
        }

        public DecodeException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
