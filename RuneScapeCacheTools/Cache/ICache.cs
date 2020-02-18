using System.Collections.Generic;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Cache
{
    public interface ICache
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

        void WriteFile(CacheIndex index, int fileId, CacheFile file);

        CacheFile ReadFile(CacheIndex index, int fileId);
    }
}
