using System.IO;
using System.Threading.Tasks;

namespace Villermen.RuneScapeCacheTools.Download
{
    public class FileRequest
    {
        public MemoryStream DataStream { get; } = new MemoryStream();

        private TaskCompletionSource<bool> CompletionSource { get; } = new TaskCompletionSource<bool>();

        public int FileSize { get; set; }

        public int RemainingLength => (int)(FileSize - DataStream.Length);

        public void Complete()
        {
            CompletionSource.SetResult(true);
        }

        public void WaitForCompletion()
        {
            CompletionSource.Task.Wait();
        }
    }
}