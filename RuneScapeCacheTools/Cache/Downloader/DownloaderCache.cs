using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;

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
    public class DownloaderCache : CacheBase
    {
        public override IEnumerable<Index> Indexes => this.GetMasterReferenceTable().ReferenceTableFiles.Keys;

        private static readonly Index[] IndexesUsingHttpInterface = { Index.Music };

        private MasterReferenceTableFile _cachedMasterReferenceTableFile;

        private readonly ConcurrentDictionary<Index, ReferenceTableFile> _cachedReferenceTables = new ConcurrentDictionary<Index, ReferenceTableFile>();

        private readonly HttpFileDownloader _httpFileDownloader = new HttpFileDownloader();

        private readonly TcpFileDownloader _tcpFileDownloader = new TcpFileDownloader();

        protected override BinaryFile FetchFile(Index index, int fileId)
        {
            var fileInfo = index != Index.ReferenceTables ? this.GetReferenceTable(index).GetFileInfo(fileId) : new CacheFileInfo
            {
                Index = index,
                FileId = fileId
            };

            var downloader = DownloaderCache.IndexesUsingHttpInterface.Contains(index) ? (IFileDownloader)this._httpFileDownloader : this._tcpFileDownloader;

            return downloader.DownloadFileAsync(index, fileId, fileInfo).Result;
        }

        protected override void PutFile(BinaryFile file)
        {
            throw new NotSupportedException("I am a downloader, not an uploader...");
        }

        public override IEnumerable<int> GetFileIds(Index index)
        {
            return this.GetReferenceTable(index).FileIds;
        }

        public override CacheFileInfo GetFileInfo(Index index, int fileId)
        {
            return this.GetReferenceTable(index).GetFileInfo(fileId);
        }

        public MasterReferenceTableFile GetMasterReferenceTable()
        {
            if (this._cachedMasterReferenceTableFile != null)
            {
                return this._cachedMasterReferenceTableFile;
            }

            this._cachedMasterReferenceTableFile = this.GetFile<MasterReferenceTableFile>(Index.ReferenceTables, (int)Index.ReferenceTables);

            return this._cachedMasterReferenceTableFile;
        }

        public ReferenceTableFile GetReferenceTable(Index index)
        {
            return this._cachedReferenceTables.GetOrAdd(index, index2 => this.GetFile<ReferenceTableFile>(Index.ReferenceTables, (int)index));
        }

        protected override void Dispose(bool disposing)
        {
            this._tcpFileDownloader.Dispose();
        }
    }
}