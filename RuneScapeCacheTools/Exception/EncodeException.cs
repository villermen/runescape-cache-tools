using System;

namespace Villermen.RuneScapeCacheTools.Exception
{
    [Serializable]
    public class EncodeException : System.Exception
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
