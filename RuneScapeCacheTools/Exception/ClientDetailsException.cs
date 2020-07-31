using System;

namespace Villermen.RuneScapeCacheTools.Exception
{
    [Serializable]
    public class ClientDetailsException : CacheException
    {
        public ClientDetailsException()
        {
        }

        public ClientDetailsException(string message) : base(message)
        {
        }

        public ClientDetailsException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
