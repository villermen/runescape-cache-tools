

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    public class HttpFileRequest : FileRequest
    {
        public HttpFileRequest(Index index, int fileId, CacheFileInfo cacheFileInfo) : base(index, fileId, cacheFileInfo)
        {
        }
    }
}