using System;
using System.Runtime.Serialization;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    [Serializable]
    internal class VorbisException : Exception
    {
        public VorbisException()
        {
        }

        public VorbisException(string message) : base(message)
        {
        }

        public VorbisException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected VorbisException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}