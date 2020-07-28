using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    /// A cache that stores information on its files in <see cref="ReferenceTable" />s in index 255.
    /// </summary>
    public abstract class RuneTek5Cache : ICache<RuneTek5CacheFile>, IDisposable
    {
        private readonly Dictionary<CacheIndex, ReferenceTable> _cachedReferenceTables = new Dictionary<CacheIndex, ReferenceTable>();

        private readonly List<CacheIndex> _changedReferenceTableIndexes = new List<CacheIndex>();

        public abstract IEnumerable<CacheIndex> GetAvailableIndexes();

        public ReferenceTable GetReferenceTable(CacheIndex index)
        {
            // Obtain the reference table either from our own cache or the actual cache
            if (this._cachedReferenceTables.ContainsKey(index))
            {
                return this._cachedReferenceTables[index];
            }

            var referenceTableFile = this.GetFile(CacheIndex.ReferenceTables, (int)index);
            var referenceTable = ReferenceTable.Decode(referenceTableFile.Data);

            this._cachedReferenceTables.Add(index, referenceTable);

            return referenceTable;
        }

        public IEnumerable<int> GetAvailableFileIds(CacheIndex index)
        {
            return this.GetReferenceTable(index).FileIds;
        }

        public RuneTek5CacheFile GetFile(CacheIndex index, int fileId)
        {
            var fileInfo = this.GetFileInfo(index, fileId);
            var fileData = this.GetFileData(index, fileId);
            return RuneTek5CacheFile.Decode(fileData, fileInfo);
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

        public void PutFile(CacheIndex index, int fileId, RuneTek5CacheFile file)
        {
            if (index == CacheIndex.ReferenceTables)
            {
                throw new ArgumentException("You can't manually write files to the reference table index.");
            }

            this.PutFileData(index, fileId, file.Encode());

            // Update the cached reference table with file's (updated) info.
            this.GetReferenceTable(index).SetFileInfo(fileId, file.Info);
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
                var encodedReferenceTable = this._cachedReferenceTables[tableIndex].Encode();
                this.PutFileData(CacheIndex.ReferenceTables, (int)tableIndex, encodedReferenceTable);
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
