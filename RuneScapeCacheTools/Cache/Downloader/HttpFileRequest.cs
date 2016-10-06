using Villermen.RuneScapeCacheTools.Cache.RuneTek5;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    public class HttpFileRequest : FileRequest
    {
        public HttpFileRequest(Index index, int fileId, ReferenceTableFile referenceTableFile) : base(index, fileId, referenceTableFile)
        {
        }
    }
}