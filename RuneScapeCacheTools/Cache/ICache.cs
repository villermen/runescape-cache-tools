using System;
using System.Collections.Generic;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    /// Interface describing the absolute minimum methods a cache should support.
    /// </summary>
    public interface ICache : IDisposable
    {
        /// <summary>
        /// Returns the indexes available in the cache.
        /// </summary>
        /// <exception cref="CacheException"></exception>
        IEnumerable<CacheIndex> GetAvailableIndexes();

        /// <summary>
        /// Returns the file IDs available in the given index.
        /// </summary>
        /// /// <exception cref="CacheException"></exception>
        IEnumerable<int> GetAvailableFileIds(CacheIndex index);

        /// <exception cref="CacheException"></exception>
        CacheFile GetFile(CacheIndex index, int fileId);

        /// <exception cref="CacheException"></exception>
        void PutFile(CacheIndex index, int fileId, CacheFile file);

        // TODO: Rewrite to abtract "Cache" or "BaseCache"
        // TODO: Import ReadOnly property to implement in all caches.
    }
}
