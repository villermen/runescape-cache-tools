using System.IO;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    public class TcpFileRequest : FileRequest
    {
        public TcpFileRequest(Index index, int fileId, ReferenceTableFile referenceTableFile) : base (index, fileId, referenceTableFile)
        {
        }

        public int FileSize { get; set; }

        public int RemainingLength => (int)(FileSize - DataStream.Length);

        public override void Write(byte[] data)
        {
            if (data.Length > RemainingLength)
            {
                throw new DownloaderException("Tried to write more bytes than were remaining in the file.");
            }

            base.Write(data);

            if (RemainingLength == 0)
            {
                Complete();
            }
        }
    }
}