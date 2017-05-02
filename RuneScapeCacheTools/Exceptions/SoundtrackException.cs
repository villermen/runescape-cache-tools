using System;

namespace Villermen.RuneScapeCacheTools.Exceptions
{
    public class SoundtrackException : Exception
    {
        public SoundtrackException()
        {
        }

        public SoundtrackException(string message) : base(message)
        {
        }

        public SoundtrackException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}