using System;

namespace Villermen.RuneScapeCacheTools.Exception
{
    [Serializable]
    public class SoundtrackException : System.Exception
    {
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