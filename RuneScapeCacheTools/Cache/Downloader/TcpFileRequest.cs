using System.IO;
using System.Threading.Tasks;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    /// <summary>
    /// TCP files parts are retrieved asynchronously so we keep track of the requested file and its partial data here.
    /// </summary>
    public class TcpFileRequest
    {
        public BinaryWriter DataWriter { get; }
        public int RemainingSize { get; set; }

        public bool MetaWritten { get; private set; }
        public long? RequestedAtMilliseconds { get; private set; }
        public bool Requested => this.RequestedAtMilliseconds != null;

        private readonly MemoryStream _dataStream;
        private readonly TaskCompletionSource<byte[]> _completionSource = new TaskCompletionSource<byte[]>();

        public TcpFileRequest()
        {
            this._dataStream = new MemoryStream();
            this.DataWriter = new BinaryWriter(this._dataStream);
        }

        public void MarkRequested(long milliseconds)
        {
            this.RequestedAtMilliseconds = milliseconds;
        }

        public void MarkMetaWritten()
        {
            this.MetaWritten = true;
        }

        public void MarkCompleted()
        {
            this._completionSource.SetResult(this._dataStream.ToArray());
            this.DataWriter.Dispose();
        }

        public void MarkFailed(System.Exception exception)
        {
            this._completionSource.SetException(exception);
        }

        public async Task<byte[]> WaitForCompletionAsync()
        {
            return await this._completionSource.Task;
        }
    }
}
