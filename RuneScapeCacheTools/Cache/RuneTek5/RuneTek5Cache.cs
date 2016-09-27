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
    public class RuneTek5Cache : Cache
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
        ///     Computes the <see cref="ChecksumTable" /> for this cache.
        ///     The checksum table forms part of the so-called "update keys".
        /// </summary>
        /// <returns></returns>
        public ChecksumTable CreateChecksumTable()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Gets the number of files in the specified index.
        /// </summary>
        /// <param name="indexId"></param>
        /// <returns></returns>
        public override int GetFileCount(int indexId)
        {
            return FileStore.GetFileCount(indexId);
        }

        //public override int GetArchiveFileCount(int indexId, int archiveId)
        //{
        //	var archive = GetArchive(indexId, archiveId);
        //	return archive.Entries.Length;
        //}

        public override CacheFile GetFile(int indexId, int fileId)
        {
            // Obtain the reference table for the requested index
            var referenceTable = GetReferenceTable(indexId);

            // The file must at least be defined in the reference table (doesn't mean it is actually complete)
            if (!referenceTable.Entries.ContainsKey(fileId))
            {
                throw new CacheException($"Given cache file {fileId} in index {indexId} does not exist.");
            }

            var referenceTableEntry = referenceTable.Entries[fileId];

            Container container;

            try
            {
                container = new Container(new MemoryStream(FileStore.GetFileData(indexId, fileId)));
            }
            catch (SectorException exception)
            {
                throw new CacheException($"Cache file {fileId} in index {indexId} is incomplete or corrupted.",
                    exception);
            }

            // Archives (files with multiple entries) are handled differently
            byte[][] data;

            var amountOfEntries = referenceTableEntry.ChildEntries.Count;

            if (amountOfEntries == 1)
            {
                data = new[] {container.Data};
            }
            else
            {
                var archive = new Archive(container.Data, amountOfEntries);
                data = archive.Entries;
            }

            return new CacheFile(indexId, fileId, data, referenceTableEntry.Version);
        }

        public ReferenceTable GetReferenceTable(int indexId)
        {
            // Try to get it from cache (I mean our own cache, it will be obtained from cache either way)
            return ReferenceTables.GetOrAdd(indexId, indexId2 =>
            {
                var metaContainer = new Container(new MemoryStream(FileStore.GetFileData(MetadataIndexId, indexId2)));
                return new ReferenceTable(metaContainer);
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