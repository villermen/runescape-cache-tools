using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    /// A cache that stores information on its files in reference tables in index 255.
    /// </summary>
    public abstract class RuneTek5Cache : ICache
    {
        private ConcurrentDictionary<CacheIndex, ReferenceTable> _cachedReferenceTables =
            new ConcurrentDictionary<CacheIndex, ReferenceTable>();

        private List<CacheIndex> _changedReferenceTableIndexes = new List<CacheIndex>();

        public ReferenceTable GetReferenceTable(CacheIndex cacheIndex, bool createIfNotFound = false)
        {
            // Obtain the reference table either from our own cache or the actual cache
            return this._cachedReferenceTables.GetOrAdd(cacheIndex, regardlesslyDiscarded =>
            {
                try
                {
                    return this.GetFile(CacheIndex.ReferenceTables, (int)cacheIndex);
                }
                catch (FileNotFoundException) when (createIfNotFound)
                {
                    return new ReferenceTable
                    {
                        Info = new CacheFileInfo
                        {
                            CacheIndex = CacheIndex.ReferenceTables,
                            FileId = (int)cacheIndex
                        }
                    };
                }
            });
        }

        public sealed override CacheFileInfo GetFileInfo(CacheIndex cacheIndex, int fileId)
        {
            if (cacheIndex != CacheIndex.ReferenceTables)
            {
                return this.GetReferenceTable(cacheIndex).GetFileInfo(fileId);
            }

            return new CacheFileInfo
            {
                CacheIndex = cacheIndex,
                FileId = fileId
                // TODO: Compression for reference tables? Compression by default?
            };
        }

        protected sealed override void PutFileInfo(CacheFileInfo fileInfo)
        {
            // Reference tables don't need no reference tables of their own
            if (fileInfo.CacheIndex != CacheIndex.ReferenceTables)
            {
                this.GetReferenceTable(fileInfo.CacheIndex, true).SetFileInfo(fileInfo.FileId.Value, fileInfo);
                this._changedReferenceTableIndexes.Add(fileInfo.CacheIndex);
            }
        }

        public sealed override IEnumerable<int> GetFileIds(CacheIndex cacheIndex)
        {
            return this.GetReferenceTable(cacheIndex).FileIds;
        }

        /// <summary>
        /// Writes changes made to the locally cached reference tables and clears the local cache.
        /// </summary>
        public void FlushCachedReferenceTables()
        {
            foreach (var tableIndex in this._changedReferenceTableIndexes)
            {
                this.PutFile(this._cachedReferenceTables[tableIndex]);
            }

            this._changedReferenceTableIndexes.Clear();
            this._cachedReferenceTables.Clear();
        }

        public override void Dispose()
        {
            this.FlushCachedReferenceTables();

            base.Dispose();

            this._cachedReferenceTables = null;
            this._changedReferenceTableIndexes = null;
        }
    }
}
