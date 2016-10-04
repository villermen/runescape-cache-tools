using System.IO;
using System.Threading.Tasks;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    // TODO: Split into HtppFileRequest and BaseFileRequest 
    public class FileRequest
    {
        public FileRequest(Index index, int fileId, ReferenceTableFile referenceTableFile)
        {
            Index = index;
            FileId = fileId;
            ReferenceTableFile = referenceTableFile;
        }

        public Index Index { get; }

        public int FileId { get; }

        public ReferenceTableFile ReferenceTableFile { get; }

        public MemoryStream DataStream { get; } = new MemoryStream();

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