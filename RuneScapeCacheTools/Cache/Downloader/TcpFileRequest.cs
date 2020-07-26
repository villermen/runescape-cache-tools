using System.IO;
using System.Threading.Tasks;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    /// <summary>
    /// TCP files parts are retrieved asynchronously so we keep track of the requested file and its partial data here.
    /// </summary>
    public class TcpFileRequest
    {
        public CacheIndex Index { get; }

        public int FileId { get; }

        public int RemainingLength => (int)(this._fileSize - this._dataStream.Length);

        public bool Completed { get; private set; }
        public bool Requested { get; set; }
        public bool MetaWritten { get; private set; }

        private readonly MemoryStream _dataStream = new MemoryStream();
        private readonly TaskCompletionSource<byte[]> _completionSource = new TaskCompletionSource<byte[]>();
        private int _fileSize;

        public TcpFileRequest(CacheIndex index, int fileId)
        {
            this.Index = index;
            this.FileId = fileId;
        }

        public void WriteMeta(CompressionType compressionType, int length)
        {
            if (this._dataStream.Length != 0)
            {
                throw new DownloaderException("File metadata must be written before anything else.");
            }

            var writer = new BinaryWriter(this._dataStream);
            writer.Write((byte)compressionType);
            writer.WriteInt32BigEndian(length);

            this._fileSize = 5 + (compressionType != CompressionType.None ? 4 : 0) + length;

            this.MetaWritten = true;
        }

        // TODO: Change Get and Put to byte arrays?

        public void WriteContent(byte[] data)
        {
            if (!this.MetaWritten)
            {
                throw new DownloaderException("File content must be written after metadata");
            }

            if (data.Length > this.RemainingLength)
            {
                throw new DownloaderException("Tried to write more bytes than were remaining in the file.");
            }

            this._dataStream.Write(data, 0, data.Length);

            if (this.RemainingLength == 0)
            {
                // TODO: Append file version if possible?
                // if (this._cacheFileInfo?.Version != null)
                // {
                //     var writer = new BinaryWriter(this._dataStream);
                //     writer.WriteUInt16BigEndian((ushort)this._cacheFileInfo.Version);
                // }

                this.Completed = true;

                this._completionSource.SetResult(this._dataStream.ToArray());
            }
        }

        public async Task<byte[]> WaitForCompletionAsync()
        {
             return await this._completionSource.Task;
        }
    }
}
