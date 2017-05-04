using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;

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
            this.CacheDirectory = cacheDirectory ?? RuneTek5Cache.DefaultCacheDirectory;
            this.ReadOnly = readOnly;

            this.Refresh();
        }

        public static string DefaultCacheDirectory
            => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jagexcache/runescape/LIVE/";

        public override IEnumerable<Index> Indexes => this.FileStore.Indexes;

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

        protected override BinaryFile FetchFile(Index index, int fileId)
        {
            CacheFileInfo info;

            if (index != Index.ReferenceTables)
            {
                // Obtain the reference table for the requested index
                var referenceTable = this.GetReferenceTable(index);

                // The file must at least be defined in the reference table (doesn't mean it is actually complete)
                if (!referenceTable.FileIds.Contains(fileId))
                {
                    throw new FileNotFoundException($"{index}/{fileId} does not exist.");
                }

                info = referenceTable.GetFileInfo(fileId);
            }
            else
            {
                info = new CacheFileInfo();
            }

            return RuneTek5FileDecoder.DecodeFile(this.FileStore.ReadFileData(index, fileId), info);
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
            return this.ReferenceTables.GetOrAdd(index, regardlesslyDiscarded =>
                this.GetFile<ReferenceTable>(Index.ReferenceTables, (int)index));
        }

        public override void PutFile(BinaryFile file)
        {
            // Write data to file store
            this.FileStore.WriteFileData(file.Info.Index, file.Info.FileId, RuneTek5FileDecoder.EncodeFile(file));

            // Adjust and write reference table
            if (file.Info.Index != Index.ReferenceTables)
            {
                ReferenceTable referenceTable;

                try
                {
                    referenceTable = this.GetReferenceTable(file.Info.Index);
                }
                catch (FileNotFoundException)
                {
                    referenceTable = new ReferenceTable
                    {
                        Format = 7,
                        Options = CacheFileOptions.Sizes | CacheFileOptions.MysteryHashes,
                        Info = new CacheFileInfo
                        {
                            CompressionType = CompressionType.Gzip,
                            Index = Index.ReferenceTables,
                            FileId = (int)file.Info.Index
                        },
                        Version = 0
                    };
                }

                // Update stored file in reference table and store reference table
                referenceTable.SetFileInfo(file.Info.FileId, file.Info);

                this.PutFile(referenceTable);
            }
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