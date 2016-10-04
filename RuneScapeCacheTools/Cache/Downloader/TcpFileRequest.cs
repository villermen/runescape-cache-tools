using System.IO;
using System.Threading.Tasks;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    public class TcpFileRequest
    {
        public MemoryStream DataStream { get; } = new MemoryStream();

        public int FileSize { get; set; }

        public int RemainingLength => (int)(FileSize - DataStream.Length);

        private TaskCompletionSource<bool> CompletionSource { get; } = new TaskCompletionSource<bool>();

        public void Complete()
        {
            CompletionSource.SetResult(true);
        }

        public async Task<byte[]> WaitForCompletionAsync()
        {
            await CompletionSource.Task;

            return DataStream.ToArray();
        }
    }
}