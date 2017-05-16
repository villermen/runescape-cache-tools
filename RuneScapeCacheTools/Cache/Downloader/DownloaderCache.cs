using System;
using System.Collections.Generic;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    /// <summary>
    /// The <see cref="DownloaderCache" /> provides the means to download current cache files from the runescape servers.
    /// Downloading uses 2 different interfaces depending on the <see cref="Index" /> of the requested file: The original
    /// TCP based interface, and a much simpler (read: better) HTTP interface.
    /// Properties prefixed with Tcp or Http will only be used by the specified downloading method.
    /// </summary>
    /// <author>Villermen</author>
    /// <author>Method</author>
    public class DownloaderCache : ReferenceTableCache
    {
        private static readonly Index[] IndexesUsingHttpInterface = { Index.Music };

        private readonly HttpFileDownloader _httpFileDownloader = new HttpFileDownloader();

        private readonly TcpFileDownloader _tcpFileDownloader = new TcpFileDownloader();

        private MasterReferenceTableFile _cachedMasterReferenceTableFile;

        public override IEnumerable<Index> GetIndexes()
        {
            return this.GetMasterReferenceTable().ReferenceTableFiles.Keys;

        }

        protected override BinaryFile GetBinaryFile(CacheFileInfo fileInfo)
        {
            var downloader = DownloaderCache.IndexesUsingHttpInterface.Contains(fileInfo.Index) ? (IFileDownloader)this._httpFileDownloader : this._tcpFileDownloader;

            return downloader.DownloadFileAsync(fileInfo.Index, fileInfo.FileId, fileInfo).Result;
        }

        protected override void PutBinaryFile(BinaryFile file)
        {
            throw new NotSupportedException("I am a downloader, not an uploader...");
        }

        public MasterReferenceTableFile GetMasterReferenceTable()
        {
            if (this._cachedMasterReferenceTableFile != null)
            {
                return this._cachedMasterReferenceTableFile;
            }

            return this._cachedMasterReferenceTableFile = this.GetFile<MasterReferenceTableFile>(Index.ReferenceTables, (int)Index.ReferenceTables);
        }

        public override void Dispose()
        {
            base.Dispose();

            this._tcpFileDownloader.Dispose();
        }
    }
}