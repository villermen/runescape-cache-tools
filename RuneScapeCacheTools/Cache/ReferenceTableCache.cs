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
        /// <summary>
        /// Reference tables are kept in memory so they don't have to be obtained again for every file.
        /// </summary>
        private readonly ConcurrentDictionary<CacheIndex, ReferenceTableFile> _cachedReferenceTables = new ConcurrentDictionary<CacheIndex, ReferenceTableFile>();

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
                throw new CacheFileNotFoundException($"File {(int)index}/{fileId} does not exist.");
            }

            return this.GetReferenceTable(index).GetFileInfo(fileId);
        }

        public abstract byte[] GetFileData(CacheIndex index, int fileId);

        public void PutFile(CacheIndex index, int fileId, CacheFile file)
        {
            if (index == CacheIndex.ReferenceTables)
            {
                throw new ArgumentException("Manually writing files to the reference table index is not allowed.");
            }

            this.PutFileData(index, fileId, file.Encode());

            // Write updated reference table.
            var referenceTable = this.GetReferenceTable(index, true);
            referenceTable.SetFileInfo(fileId, file.Info);
            var referenceTableFile = new CacheFile(referenceTable.Encode());
            referenceTableFile.Info.CompressionType = CompressionType.Bzip2;
            this.PutFileData(CacheIndex.ReferenceTables, (int)index, referenceTableFile.Encode());
        }

        protected abstract void PutFileData(CacheIndex index, int fileId, byte[] data);

        public void ClearCachedReferenceTables()
        {
            this._cachedReferenceTables.Clear();
        }

        public abstract void Dispose();
    }
}
