using System;

namespace Villermen.RuneScapeCacheTools.Cache
{
    [Serializable]
    internal class DecodeException : Exception
    {
        public DecodeException()
        {
        }

        public DecodeException(string message) : base(message)
        {
        }

        public DecodeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}