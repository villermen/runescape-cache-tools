using System;
using System.Collections.Concurrent;
using System.IO;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    ///     The <see cref="RuneTek5Cache" /> class provides a unified, high-level API for modifying the cache of a Jagex game.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Villermen</author>
    public class RuneTek5Cache : CacheBase
    {
        /// <summary>
        ///     Index that contains metadata about the other indexes.
        /// </summary>
        public const int MetadataIndexId = 255;

        /// <summary>
        ///     Creates an interface on the cache stored in the given directory.
        /// </summary>
        /// <param name="cacheDirectory"></param>
        public RuneTek5Cache(string cacheDirectory = null) :
            base(cacheDirectory ?? DefaultCacheDirectory)
        {
            FileStore = new FileStore(CacheDirectory);
        }

        /// <summary>
        ///     The <see cref="RuneTek5.FileStore" /> that backs this cache.
        /// </summary>
        public FileStore FileStore { get; }

        public override int IndexCount => FileStore.IndexCount;

        public static string DefaultCacheDirectory
            => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jagexcache/runescape/LIVE/";

        private ConcurrentDictionary<int, ReferenceTable> ReferenceTables { get; } =
            new ConcurrentDictionary<int, ReferenceTable>();

        /// <summary>
        ///     Gets the number of files in the specified index.
        /// </summary>
        /// <param name="indexId"></param>
        /// <returns></returns>
        public override int GetFileCount(int indexId)
        {
            return FileStore.GetFileCount(indexId);
        }

        public override CacheFile GetFile(int indexId, int fileId)
        {
            // Obtain the reference table for the requested index
            var referenceTable = GetReferenceTable(indexId);

            // The file must at least be defined in the reference table (doesn't mean it is actually complete)
            if (!referenceTable.Files.ContainsKey(fileId))
            {
                throw new CacheException($"Given cache file {fileId} in index {indexId} does not exist.");
            }

            var referenceTableEntry = referenceTable.Files[fileId];

            try
            {
                return new RuneTek5CacheFile(FileStore.GetFileData(indexId, fileId), referenceTableEntry);
            }
            catch (SectorException exception)
            {
                throw new CacheException($"Cache file {fileId} in index {indexId} is incomplete or corrupted.",
                    exception);
            }
        }

        public ReferenceTable GetReferenceTable(int indexId)
        {
            // Try to get it from cache (I mean our own cache, it will be obtained from some kind of cache either way)
            return ReferenceTables.GetOrAdd(indexId, indexId2 =>
            {
                var cacheFile = new RuneTek5CacheFile(FileStore.GetFileData(MetadataIndexId, indexId2), null);
                return new ReferenceTable(cacheFile, indexId);
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                FileStore.Dispose();
            }
        }
    }
}