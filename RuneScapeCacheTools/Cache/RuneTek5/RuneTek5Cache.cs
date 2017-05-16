using System;
using System.Collections.Generic;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    /// Can read and write to a RuneTek5 type cache consisting of a single data (.dat2) file and some index (.id#) files.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Villermen</author>
    public class RuneTek5Cache : ReferenceTableCache
    {
        public static string DefaultCacheDirectory =>
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jagexcache/runescape/LIVE/";

        /// <summary>
        /// The directory where the cache is located.
        /// </summary>
        public string CacheDirectory { get; }

        public bool ReadOnly { get; }

        /// <summary>
        /// The <see cref="RuneTek5.FileStore" /> that backs this cache.
        /// </summary>
        private readonly FileStore _fileStore;

        /// <summary>
        /// Creates an interface on the cache stored in the given directory.
        /// </summary>
        /// <param name="cacheDirectory"></param>
        /// <param name="readOnly"></param>
        public RuneTek5Cache(string cacheDirectory = null, bool readOnly = true)
        {
            this.CacheDirectory = cacheDirectory ?? RuneTek5Cache.DefaultCacheDirectory;
            this.ReadOnly = readOnly;

            this._fileStore = new FileStore(this.CacheDirectory, this.ReadOnly);
        }

        public override IEnumerable<Index> GetIndexes()
        {
            return this._fileStore.GetIndexes();
        }

        protected override BinaryFile GetBinaryFile(CacheFileInfo fileInfo)
        {
            var file = new BinaryFile
            {
                Info = fileInfo
            };

            file.Decode(this._fileStore.ReadFileData(fileInfo.Index, fileInfo.FileId));

            return file;
        }

        protected override void PutBinaryFile(BinaryFile file)
        {
            // Write data to file store
            this._fileStore.WriteFileData(file.Info.Index, file.Info.FileId, file.Encode());
        }

        public override void Dispose()
        {
            base.Dispose();

            if (!this.Disposed)
            {
                this._fileStore.Dispose();
            }
        }
    }
}