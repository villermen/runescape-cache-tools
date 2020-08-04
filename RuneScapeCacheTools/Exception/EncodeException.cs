using System;

namespace Villermen.RuneScapeCacheTools.Exception
{
    [Serializable]
    public class EncodeException : CacheException
    {
        public EncodeException()
        {
        }

        public EncodeException(string message) : base(message)
        {
        }

        public EncodeException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
