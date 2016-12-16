namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    using Villermen.RuneScapeCacheTools.Cache.CacheFile;

    public class HttpFileRequest : FileRequest
    {
        public HttpFileRequest(Index index, int fileId, CacheFileInfo cacheFileInfo) : base(index, fileId, cacheFileInfo)
        {
        }
    }
}