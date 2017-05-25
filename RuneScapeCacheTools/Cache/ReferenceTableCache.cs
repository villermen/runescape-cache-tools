using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;

namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    /// A cache that stores information on its files in reference tables in index 255.
    /// </summary>
    public abstract class ReferenceTableCache : CacheBase
    {
        private ConcurrentDictionary<Index, ReferenceTableFile> _cachedReferenceTables =
            new ConcurrentDictionary<Index, ReferenceTableFile>();
        
        private List<Index> _changedReferenceTableIndexes = new List<Index>();

        public ReferenceTableFile GetReferenceTable(Index index, bool createIfNotFound = false)
        {
            // Obtain the reference table either from our own cache or the actual cache
            return this._cachedReferenceTables.GetOrAdd(index, regardlesslyDiscarded =>
            {
                try
                {
                    return this.GetFile<ReferenceTableFile>(Index.ReferenceTables, (int)index);
                }
                catch (FileNotFoundException) when (createIfNotFound)
                {
                    return new ReferenceTableFile
                    {
                        Info = new CacheFileInfo
                        {
                            Index = Index.ReferenceTables,
                            FileId = (int)index
                        }
                    };
                }
            });
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
            // Reference tables don't need no reference tables of their own 
            if (fileInfo.Index != Index.ReferenceTables)
            {
                this.GetReferenceTable(fileInfo.Index, true).SetFileInfo(fileInfo.FileId, fileInfo);
                this._changedReferenceTableIndexes.Add(fileInfo.Index);
            }
        }

        public sealed override IEnumerable<int> GetFileIds(Index index)
        {
            return this.GetReferenceTable(index).FileIds;
        }

        public override void Dispose()
        {
            base.Dispose();

            if (this._changedReferenceTableIndexes != null)
            {
                // Write out changed cached reference tables
                foreach (var tableIndex in this._changedReferenceTableIndexes)
                {
                    var referenceTable = this._cachedReferenceTables[tableIndex];
                    this.PutFile(referenceTable);
                }

                this._cachedReferenceTables = null;
                this._changedReferenceTableIndexes = null;
            }
        }
    }
}