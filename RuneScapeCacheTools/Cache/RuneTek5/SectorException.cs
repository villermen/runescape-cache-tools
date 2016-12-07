using System;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    public class SectorException : Exception
    {
        public SectorException()
        {
        }

        public SectorException(string message) : base(message)
        {
        }

        public SectorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}