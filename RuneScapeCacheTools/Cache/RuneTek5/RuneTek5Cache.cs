using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    using Villermen.RuneScapeCacheTools.Cache.CacheFile;

    /// <summary>
    ///     The <see cref="RuneTek5Cache" /> class provides a unified, high-level API for modifying the cache of a Jagex game.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Villermen</author>
    public class RuneTek5Cache : CacheBase
    {
        /// <summary>
        ///     Creates an interface on the cache stored in the given directory.
        /// </summary>
        /// <param name="cacheDirectory"></param>
        /// <param name="readOnly"></param>
        public RuneTek5Cache(string cacheDirectory = null, bool readOnly = true)
        {
            this.CacheDirectory = cacheDirectory ?? DefaultCacheDirectory;
            this.ReadOnly = readOnly;

            this.Refresh();
        }

        public static string DefaultCacheDirectory
            => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jagexcache/runescape/LIVE/";

        public override IEnumerable<Index> Indexes => Enumerable.Range(0, this.FileStore.IndexCount).Cast<Index>();

        /// <summary>
        ///     The directory where the cache is located.
        /// </summary>
        public string CacheDirectory { get; }

        public bool ReadOnly { get; }

        /// <summary>
        ///     The <see cref="RuneTek5.FileStore" /> that backs this cache.
        /// </summary>
        public FileStore FileStore { get; private set; }

        private ConcurrentDictionary<Index, ReferenceTable> ReferenceTables { get; set; }

        public override T GetFile<T>(Index index, int fileId)
        {
            CacheFileInfo info;

            if (index != Index.ReferenceTables)
            {
                // Obtain the reference table for the requested index
                var referenceTable = this.GetReferenceTable(index);

                // The file must at least be defined in the reference table (doesn't mean it is actually complete)
                if (!referenceTable.FileIds.Contains(fileId))
                {
                    throw new CacheFileNotFoundException($"{index}/{fileId} does not exist.");
                }

                info = referenceTable.GetFileInfo(fileId);
            }
            else
            {
                info = new CacheFileInfo();
            }

            try
            {
                var file = RuneTek5FileDecoder.DecodeFile(this.FileStore.ReadFileData(index, fileId), info);

                // TODO: Move this check up in inheritance?
                if (!(file is T))
                {
                    throw new ArgumentException($"Obtained file is of type  of given type {file.GetType().Name} instead of requested {nameof(T)}.");
                }

                return file as T;

            }
            catch (SectorException exception)
            {
                throw new CacheFileNotFoundException($"{index}/{fileId} is incomplete or corrupted.",
                    exception);
            }
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
            return referenceTable.FileIds;
        }

        public override CacheFileInfo GetFileInfo(Index index, int fileId)
        {
            return this.GetReferenceTable(index).GetFileInfo(fileId);
        }

        public ReferenceTable GetReferenceTable(Index index)
        {
            // Try to get it from cache (I mean our own cache, it will be obtained from some kind of cache either way)
            return this.ReferenceTables.GetOrAdd(index, index2 =>
            {
                var data = this.FileStore.ReadFileData(Index.ReferenceTables, (int)index2);
                var cacheFile = (DataCacheFile)RuneTek5FileDecoder.DecodeFile(data, new CacheFileInfo // TODO: We can do better than casting
                {
                    // TODO: Insert magical goop
                });
                return ReferenceTable.Decode(cacheFile);
            });
        }

        public override void PutFile(BaseCacheFile file)
        {
            if (!(file is DataCacheFile))
            {
                throw new InvalidOperationException($"Only cache files of type {nameof(DataCacheFile)} can be written to a RuneTek5 cache.");
            }

            this.PutFile((DataCacheFile)file);
        }

        public void PutFile(DataCacheFile file)
        {
            // Write data to file store
            this.FileStore.WriteFileData(file.Info.Index, file.Info.FileId, RuneTek5FileDecoder.EncodeFile(file));

            // TODO: Allow for creation of reference tables and entries out of thin air
            // Adjust and write reference table
            var referenceTable = this.GetReferenceTable(file.Info.Index);
            referenceTable.SetFileInfo(file.Info.FileId, file.Info);

            this.FileStore.WriteFileData(Index.ReferenceTables, (int)file.Info.Index, RuneTek5FileDecoder.EncodeFile(referenceTable.Encode()));
        }

        /// <summary>
        /// Recreates the backing file store and drops all cached reference tables.
        /// </summary>
        public void Refresh()
        {
            this.FileStore?.Dispose();

            this.FileStore = new FileStore(this.CacheDirectory, this.ReadOnly);

            this.ReferenceTables = new ConcurrentDictionary<Index, ReferenceTable>();
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