using System.Threading.Tasks;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    public interface IFileDownloader
    {
        Task<BinaryFile> DownloadFileAsync(Index index, int fileId, CacheFileInfo fileInfo);
    }
}