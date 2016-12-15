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
                var cacheFile = RuneTek5CacheFile.Decode(this.FileStore.ReadFileData(Index.ReferenceTables, (int)index2), new CacheFileInfo());
                return new ReferenceTable(cacheFile, index);
            });
        }

        public RuneTek5CacheFile GetRuneTek5File(Index index, int fileId)
        {
            // Obtain the reference table for the requested index
            var referenceTable = this.GetReferenceTable(index);

            // The file must at least be defined in the reference table (doesn't mean it is actually complete)
            if (!referenceTable.FileIds.Contains(fileId))
            {
                throw new CacheFileNotFoundException($"{index}/{fileId} does not exist.");
            }

            var referenceTableEntry = referenceTable.GetFileInfo(fileId);

            try
            {
                return RuneTek5CacheFile.Decode(this.FileStore.ReadFileData(index, fileId), referenceTableEntry);
            }
            catch (SectorException exception)
            {
                throw new CacheFileNotFoundException($"{index}/{fileId} is incomplete or corrupted.",
                    exception);
            }
        }

        public override void PutFile(CacheFile file)
        {
            var runeTek5File = file as RuneTek5CacheFile;
            if (runeTek5File == null)
            {
                throw new ArgumentException("Only RuneTek5CacheFiles can be put into a RuneTek5Cache.");
            }

            // Write data to file store
            this.FileStore.WriteFileData(runeTek5File.Info.Index, runeTek5File.Info.FileId, runeTek5File.Encode());

            // TODO: Allow for creation of reference tables and entries out of thin air

            // Adjust and write reference table
            var referenceTable = this.GetReferenceTable(runeTek5File.Info.Index);
            referenceTable.SetFileInfo(runeTek5File.Info.FileId, runeTek5File.Info);

            this.FileStore.WriteFileData(Index.ReferenceTables, (int)runeTek5File.Info.Index, referenceTable.Encode().Encode());
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