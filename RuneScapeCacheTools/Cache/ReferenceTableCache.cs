using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    /// A cache that stores information on its files in <see cref="ReferenceTableFile" />s in index 255.
    /// </summary>
    public abstract class ReferenceTableCache : ICache
    {
        public readonly ICacheFileDecoder FileDecoder;

        protected ReferenceTableCache(ICacheFileDecoder fileDecoder)
        {
            this.FileDecoder = fileDecoder;
        }

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
                    return new ReferenceTableFile
                    {
                        Options = ReferenceTableOptions.Sizes,
                    };
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
            var fileData = this.GetFileData(index, fileId, fileInfo);

            return this.FileDecoder.DecodeFile(fileData, fileInfo);
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

        public abstract byte[] GetFileData(CacheIndex index, int fileId, CacheFileInfo? info);

        public void PutFile(CacheIndex index, int fileId, CacheFile file)
        {
            if (index == CacheIndex.ReferenceTables)
            {
                throw new ArgumentException("Manually writing files to the reference table index is not allowed.");
            }

            var info = file.Info.Clone();

            this.PutFileData(index, fileId, this.FileDecoder.EncodeFile(file, info), info);

            // Write updated reference table.
            var referenceTable = this.GetReferenceTable(index, true);
            referenceTable.SetFileInfo(fileId, info);
            var referenceTableFile = new CacheFile(referenceTable.Encode());
            this.PutFileData(
                CacheIndex.ReferenceTables,
                (int)index,
                this.FileDecoder.EncodeFile(referenceTableFile, null),
                null
            );
        }

        protected abstract void PutFileData(CacheIndex index, int fileId, byte[] data, CacheFileInfo? info);

        public void ClearCachedReferenceTables()
        {
            this._cachedReferenceTables.Clear();
        }

        public abstract void Dispose();
    }
}
