using System;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5.Downloader
{
    public class DownloaderException : Exception
    {
        public DownloaderException()
        {
        }

        public DownloaderException(string message) : base(message)
        {
        }

        public DownloaderException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}