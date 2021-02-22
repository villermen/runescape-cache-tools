using System;

namespace Villermen.RuneScapeCacheTools.Exception
{
    [Serializable]
    public class ClientPropertiesException : CacheException
    {
        public ClientPropertiesException()
        {
        }

        public ClientPropertiesException(string message) : base(message)
        {
        }

        public ClientPropertiesException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
