using System.Collections.Generic;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    /// Interface describing the absolute minimum methods a cache should support.
    /// </summary>
    public interface ICache<TFile> where TFile : CacheFile
    {
        /// <summary>
        /// Returns the indexes available in the cache.
        /// </summary>
        /// <returns></returns>
        IEnumerable<CacheIndex> GetAvailableIndexes();

        /// <summary>
        /// Returns the file IDs available in the given index.
        /// </summary>
        IEnumerable<int> GetAvailableFileIds(CacheIndex index);

        TFile GetFile(CacheIndex index, int fileId);

        void PutFile(CacheIndex index, int fileId, TFile file);
    }
}
