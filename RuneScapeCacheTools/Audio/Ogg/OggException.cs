using System;
using System.Runtime.Serialization;

namespace Villermen.RuneScapeCacheTools.Audio.Ogg
{
    [Serializable]
    internal class OggException : Exception
    {
        public OggException()
        {
        }

        public OggException(string message) : base(message)
        {
        }

        public OggException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OggException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}