using System.Collections.Concurrent;
using System.Collections.Generic;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;

namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    /// A cache that stores information on its files in reference tables in index 255.
    /// </summary>
    public abstract class ReferenceTableCache : CacheBase
    {
        private readonly ConcurrentDictionary<Index, ReferenceTableFile> _cachedReferenceTables =
            new ConcurrentDictionary<Index, ReferenceTableFile>();

        public ReferenceTableFile GetReferenceTable(Index index)
        {
            // Obtain the reference table either from our own cache or the actual cache
            return this._cachedReferenceTables.GetOrAdd(index, regardlesslyDiscarded =>
                this.GetFile<ReferenceTableFile>(Index.ReferenceTables, (int)index));
        }

        public sealed override CacheFileInfo GetFileInfo(Index index, int fileId)
        {
            if (index != Index.ReferenceTables)
            {
                return this.GetReferenceTable(index).GetFileInfo(fileId);
            }

            return new CacheFileInfo
            {
                Index = index,
                FileId = fileId
                // TODO: Compression for reference tables? Compression by default?
            };
        }

        protected sealed override void PutFileInfo(CacheFileInfo fileInfo)
        {
            if (fileInfo.Index != Index.ReferenceTables)
            {
                this.GetReferenceTable(fileInfo.Index).SetFileInfo(fileInfo.FileId, fileInfo);
            }
        }

        public sealed override IEnumerable<int> GetFileIds(Index index)
        {
            return this.GetReferenceTable(index).FileIds;
        }

        public override void Dispose()
        {
            base.Dispose();

            if (!this.Disposed)
            {
                // Write out cached reference tables
                foreach (var referenceTable in this._cachedReferenceTables)
                {
                    this.PutFile(referenceTable.Value);
                }
            }
        }
    }
}