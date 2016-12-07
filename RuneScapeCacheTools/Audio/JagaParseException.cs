using System;

namespace Villermen.RuneScapeCacheTools.Audio
{
    public class JagaParseException : Exception
    {
        public JagaParseException()
        {
        }

        public JagaParseException(string message) : base(message)
        {
        }

        public JagaParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}