using System.IO;
using System.Threading.Tasks;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    public abstract class FileRequest
    {
        protected FileRequest(Index index, int fileId, CacheFileInfo cacheFileInfo)
        {
            Index = index;
            FileId = fileId;
            CacheFileInfo = cacheFileInfo;
        }

        public CacheFileInfo CacheFileInfo { get; }

        public MemoryStream DataStream { get; } = new MemoryStream();

        public int FileId { get; }

        public Index Index { get; }

        private TaskCompletionSource<byte[]> CompletionSource { get; } = new TaskCompletionSource<byte[]>();

        public virtual void Write(byte[] data)
        {
            DataStream.Write(data, 0, data.Length);
        }

        public void Complete()
        {
            CompletionSource.SetResult(DataStream.ToArray());
        }

        public byte[] WaitForCompletion()
        {
            return CompletionSource.Task.Result;
        }
    }
}