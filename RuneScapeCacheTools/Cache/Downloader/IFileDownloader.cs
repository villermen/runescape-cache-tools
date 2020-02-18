using System.Threading.Tasks;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    public interface IFileDownloader
    {
        Task<RawCacheFile> DownloadFileAsync(CacheIndex cacheIndex, int fileId, CacheFileInfo fileInfo);
    }
}
