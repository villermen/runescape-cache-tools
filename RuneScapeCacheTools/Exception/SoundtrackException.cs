using System;

namespace Villermen.RuneScapeCacheTools.Exception
{
    [Serializable]
    public class SoundtrackException : CacheException
    {
        public bool IsSoxError = false;

        public SoundtrackException()
        {
        }

        public SoundtrackException(string message) : base(message)
        {
        }

        public SoundtrackException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
