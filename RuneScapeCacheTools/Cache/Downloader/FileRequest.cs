using System.IO;
using System.Threading.Tasks;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    // TODO: Split into HtppFileRequest and BaseFileRequest 
    public abstract class FileRequest
    {
        protected FileRequest(Index index, int fileId, ReferenceTableFile referenceTableFile)
        {
            Index = index;
            FileId = fileId;
            ReferenceTableFile = referenceTableFile;
        }

        public MemoryStream DataStream { get; } = new MemoryStream();

        public int FileId { get; }

        public Index Index { get; }

        public ReferenceTableFile ReferenceTableFile { get; }

        private TaskCompletionSource<byte[]> CompletionSource { get; } = new TaskCompletionSource<byte[]>();

        public virtual void Write(byte[] data)
        {
            DataStream.Write(data, 0, data.Length);
        }

        public void Complete()
        {
            CompletionSource.SetResult(DataStream.ToArray());
        }

        public async Task<byte[]> WaitForCompletionAsync()
        {
            return await CompletionSource.Task;
        }
    }
}