using Villermen.RuneScapeCacheTools.Exceptions;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    public class TcpFileRequest : FileRequest
    {
        public TcpFileRequest(Index index, int fileId, CacheFileInfo cacheFileInfo) : base(index, fileId, cacheFileInfo)
        {
        }

        public int FileSize { get; set; }

        public int RemainingLength => (int)(this.FileSize - this.DataStream.Length);

        public override void Write(byte[] data)
        {
            if (data.Length > this.RemainingLength)
            {
                throw new DownloaderException("Tried to write more bytes than were remaining in the file.");
            }

            base.Write(data);

            if (this.RemainingLength == 0)
            {
                this.Complete();
            }
        }
    }
}