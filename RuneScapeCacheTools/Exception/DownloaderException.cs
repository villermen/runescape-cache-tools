using System;

namespace Villermen.RuneScapeCacheTools.Exception
{
    [Serializable]
    public class DownloaderException : System.Exception
    {
        public DownloaderException()
        {
        }

        public DownloaderException(string message) : base(message)
        {
        }

        public DownloaderException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}