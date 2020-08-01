using System;
using System.Collections.Generic;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    /// <summary>
    /// The <see cref="DownloaderCache" /> provides the means to download current cache files from the runescape
    /// servers. Downloading uses 2 different interfaces depending on the <see cref="CacheIndex" /> of the requested
    /// file: The original TCP based interface and a newer HTTP interface.
    ///
    /// TODO: Add 10s timeout to file retrieval =)
    /// </summary>
    public class DownloaderCache : RuneTek5Cache
    {
        private static readonly CacheIndex[] HttpInterfaceIndexes =
        {
            CacheIndex.Music,
        };

        private MasterReferenceTable? _cachedMasterReferenceTable;

        private readonly TcpFileDownloader _tcpFileDownloader;

        private readonly HttpFileDownloader _httpFileDownloader;

        public DownloaderCache()
        {
            this._tcpFileDownloader = new TcpFileDownloader();
            this._httpFileDownloader = new HttpFileDownloader();
        }

        public override IEnumerable<CacheIndex> GetAvailableIndexes()
        {
            return this.GetMasterReferenceTable().AvailableReferenceTables;
        }

        public MasterReferenceTable GetMasterReferenceTable()
        {
            if (this._cachedMasterReferenceTable != null)
            {
                return this._cachedMasterReferenceTable;
            }

            var masterReferenceTableFile = this.GetFile(CacheIndex.ReferenceTables, (int)CacheIndex.ReferenceTables);
            this._cachedMasterReferenceTable = MasterReferenceTable.Decode(masterReferenceTableFile.Data);
            return this._cachedMasterReferenceTable;
        }

        protected override byte[] GetFileData(CacheIndex index, int fileId)
        {
            if (DownloaderCache.HttpInterfaceIndexes.Contains(index))
            {
                // HTTP downloader requires file info in advance.
                var fileInfo = this.GetFileInfo(index, fileId);
                return this._httpFileDownloader.DownloadFileData(index, fileId, fileInfo);
            }

            return this._tcpFileDownloader.DownloadFileData(index, fileId);
        }

        protected override void PutFileData(CacheIndex index, int fileId, byte[] data)
        {
            throw new NotSupportedException("I am a downloader, stop trying to put things in me!");
        }

        public override void Dispose()
        {
            base.Dispose();

            this._tcpFileDownloader?.Dispose();
        }
    }
}
