using System;
using System.Collections.Generic;
using System.Linq;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    /// <summary>
    /// The <see cref="DownloaderCache" /> provides the means to download current cache files from the runescape servers.
    /// Downloading uses 2 different interfaces depending on the <see cref="CacheIndex" /> of the requested file: The original
    /// TCP based interface, and a much simpler (read: better) HTTP interface.
    /// </summary>
    public class DownloaderCache : RuneTek5Cache
    {
        private static readonly CacheIndex[] IndexesUsingHttpInterface = { CacheIndex.Music };

        private readonly HttpFileDownloader _httpFileDownloader = new HttpFileDownloader();

        private TcpFileDownloader _tcpFileDownloader = new TcpFileDownloader();

        private MasterReferenceTableFile _cachedMasterReferenceTableFile;

        public override IEnumerable<CacheIndex> GetIndexes()
        {
            return this.GetMasterReferenceTable().GetAvailableReferenceTables().Keys;
        }

        protected override RawCacheFile GetFile(CacheFileInfo fileInfo)
        {
            var downloader = DownloaderCache.IndexesUsingHttpInterface.Contains(fileInfo.CacheIndex) ? (IFileDownloader)this._httpFileDownloader : this._tcpFileDownloader;

            return downloader.DownloadFileAsync(fileInfo.CacheIndex, fileInfo.FileId.Value, fileInfo).Result;
        }

        protected override void PutBinaryFile(RawCacheFile file)
        {
            throw new NotSupportedException("I am a downloader, not an uploader...");
        }

        public MasterReferenceTableFile GetMasterReferenceTable()
        {
            if (this._cachedMasterReferenceTableFile != null)
            {
                return this._cachedMasterReferenceTableFile;
            }

            return this._cachedMasterReferenceTableFile = this.GetFile(CacheIndex.ReferenceTables, (int)CacheIndex.ReferenceTables);
        }

        public override void Dispose()
        {
            base.Dispose();

            if (this._tcpFileDownloader != null)
            {
                this._tcpFileDownloader.Dispose();
                this._tcpFileDownloader = null;
            }
        }
    }
}
