using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    /// A cache that stores information on its files in <see cref="ReferenceTableFile" />s in index 255.
    /// </summary>
    public abstract class ReferenceTableCache : ICache
    {
        private readonly ConcurrentDictionary<CacheIndex, ReferenceTableFile> _cachedReferenceTables = new ConcurrentDictionary<CacheIndex, ReferenceTableFile>();

        private readonly List<CacheIndex> _changedReferenceTableIndexes = new List<CacheIndex>();

        public abstract IEnumerable<CacheIndex> GetAvailableIndexes();

        public ReferenceTableFile GetReferenceTable(CacheIndex index, bool createIfNotFound = false)
        {
            // Obtain the reference table either from our own cache or the actual cache
            if (this._cachedReferenceTables.ContainsKey(index))
            {
                return this._cachedReferenceTables[index];
            }

            return this._cachedReferenceTables.GetOrAdd(index, _ =>
            {
                try
                {
                    var file = this.GetFile(CacheIndex.ReferenceTables, (int)index);
                    return ReferenceTableFile.Decode(file.Data);
                }
                catch (CacheFileNotFoundException) when (createIfNotFound)
                {
                    return new ReferenceTableFile();
                }
            });
        }

        public IEnumerable<int> GetAvailableFileIds(CacheIndex index)
        {
            return this.GetReferenceTable(index).FileIds;
        }

        public CacheFile GetFile(CacheIndex index, int fileId)
        {
            var fileInfo = this.GetFileInfo(index, fileId);
            var fileData = this.GetFileData(index, fileId);
            return CacheFile.Decode(fileData, fileInfo);
        }

        public CacheFileInfo GetFileInfo(CacheIndex index, int fileId)
        {
            // Return empty info for reference tables themselves.
            if (index == CacheIndex.ReferenceTables)
            {
                return new CacheFileInfo();
            }

            if (!this.GetAvailableFileIds(index).Contains(fileId))
            {
                throw new ArgumentException($"File {fileId} does not exist in index {(int)index}.");
            }

            return this.GetReferenceTable(index).GetFileInfo(fileId);
        }

        protected abstract byte[] GetFileData(CacheIndex index, int fileId);

        public void PutFile(CacheIndex index, int fileId, CacheFile file)
        {
            if (index == CacheIndex.ReferenceTables)
            {
                throw new ArgumentException("You can't manually write files to the reference table index.");
            }

            this.PutFileData(index, fileId, file.Encode());

            // Update the cached reference table with file's (updated) info.
            this.GetReferenceTable(index, true).SetFileInfo(fileId, file.Info);
            this._changedReferenceTableIndexes.Add(index);
        }

        protected abstract void PutFileData(CacheIndex index, int fileId, byte[] data);

        /// <summary>
        /// Writes out changes made to the cached reference tables and clears the local cache.
        /// </summary>
        public void FlushCachedReferenceTables()
        {
            foreach (var tableIndex in this._changedReferenceTableIndexes)
            {
                var cacheFile = new CacheFile(this._cachedReferenceTables[tableIndex].Encode());
                this.PutFileData(CacheIndex.ReferenceTables, (int)tableIndex, cacheFile.Encode());
            }

            this._changedReferenceTableIndexes.Clear();
            this._cachedReferenceTables.Clear();
        }

        public virtual void Dispose()
        {
            this.FlushCachedReferenceTables();
        }
    }
}
