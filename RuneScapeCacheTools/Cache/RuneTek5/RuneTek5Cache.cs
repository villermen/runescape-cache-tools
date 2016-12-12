using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    ///     The <see cref="RuneTek5Cache" /> class provides a unified, high-level API for modifying the cache of a Jagex game.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Villermen</author>
    // TODO: Don't forget to add to metadata after writing file
    public class RuneTek5Cache : CacheBase
    {
        /// <summary>
        ///     Creates an interface on the cache stored in the given directory.
        /// </summary>
        /// <param name="cacheDirectory"></param>
        public RuneTek5Cache(string cacheDirectory = null)
        {
            this.CacheDirectory = cacheDirectory ?? DefaultCacheDirectory;
            this.FileStore = new FileStore(this.CacheDirectory);
        }

        public static string DefaultCacheDirectory
            => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jagexcache/runescape/LIVE/";

        public override IEnumerable<Index> Indexes => Enumerable.Range(0, this.FileStore.IndexCount).Cast<Index>();

        /// <summary>
        ///     The directory where the cache is located.
        /// </summary>
        public string CacheDirectory { get; }

        /// <summary>
        ///     The <see cref="RuneTek5.FileStore" /> that backs this cache.
        /// </summary>
        public FileStore FileStore { get; }

        private ConcurrentDictionary<Index, ReferenceTable> ReferenceTables { get; } =
            new ConcurrentDictionary<Index, ReferenceTable>();

        public override CacheFile GetFile(Index index, int fileId)
        {
            return this.GetRuneTek5File(index, fileId);
        }

        /// <summary>
        ///     Gets the files specified in the given index.
        ///     Returned files might still not be present in the cache however.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public override IEnumerable<int> GetFileIds(Index index)
        {
            var referenceTable = this.GetReferenceTable(index);
            return referenceTable.Files.Keys;
        }

        public override CacheFileInfo GetFileInfo(Index index, int fileId)
        {
            return this.GetReferenceTable(index).Files[fileId];
        }

        public ReferenceTable GetReferenceTable(Index index)
        {
            // Try to get it from cache (I mean our own cache, it will be obtained from some kind of cache either way)
            return this.ReferenceTables.GetOrAdd(index, index2 =>
            {
                var cacheFile = new RuneTek5CacheFile(this.FileStore.ReadFileData(Index.ReferenceTables, (int)index2), null);
                return new ReferenceTable(cacheFile, index);
            });
        }

        public RuneTek5CacheFile GetRuneTek5File(Index index, int fileId)
        {
            // Obtain the reference table for the requested index
            var referenceTable = this.GetReferenceTable(index);

            // The file must at least be defined in the reference table (doesn't mean it is actually complete)
            if (!referenceTable.Files.ContainsKey(fileId))
            {
                throw new CacheException($"Given cache file {fileId} in index {index} does not exist.");
            }

            var referenceTableEntry = referenceTable.Files[fileId];

            try
            {
                return new RuneTek5CacheFile(this.FileStore.ReadFileData(index, fileId), referenceTableEntry);
            }
            catch (SectorException exception)
            {
                throw new CacheException($"Cache file {fileId} in index {index} is incomplete or corrupted.",
                    exception);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.FileStore.Dispose();
            }
        }
    }
}