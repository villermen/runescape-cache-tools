using System.Threading.Tasks;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Utility
{
    public interface IFileDownloader
    {
        Task<BinaryFile> DownloadFileAsync(Index index, int fileId, CacheFileInfo fileInfo);
    }
}
