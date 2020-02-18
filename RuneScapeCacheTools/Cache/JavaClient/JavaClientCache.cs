using System;
using System.Collections.Generic;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.Cache.JavaClient
{
    /// <summary>
    /// Can read and write to a RuneTek5 type cache consisting of a single data (.dat2) file and some index (.id#) files.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Villermen</author>
    public class JavaClientCache : RuneTek5Cache
    {
        public static string DefaultCacheDirectory =>
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jagexcache/runescape/LIVE/";

        /// <summary>
        /// The directory where the cache is located.
        /// </summary>
        public string CacheDirectory { get; }

        public bool ReadOnly { get; }

        /// <summary>
        /// The <see cref="FileStore" /> that backs this cache.
        /// </summary>
        private FileStore _fileStore;

        /// <summary>
        /// Creates an interface on the cache stored in the given directory.
        /// </summary>
        /// <param name="cacheDirectory"></param>
        /// <param name="readOnly"></param>
        public JavaClientCache(string cacheDirectory = null, bool readOnly = true)
        {
            this.CacheDirectory = cacheDirectory ?? JavaClientCache.DefaultCacheDirectory;
            this.ReadOnly = readOnly;

            this._fileStore = new FileStore(this.CacheDirectory, this.ReadOnly);
        }

        public override IEnumerable<CacheIndex> GetIndexes()
        {
            return this._fileStore.GetIndexes();
        }

        protected override RawCacheFile GetFile(CacheFileInfo fileInfo)
        {
            var file = new RawCacheFile
            {
                Info = fileInfo
            };

            file.Decode(this._fileStore.ReadFileData(fileInfo.CacheIndex, fileInfo.FileId.Value));

            return file;
        }

        protected override void PutBinaryFile(RawCacheFile file)
        {
            // Write data to file store
            this._fileStore.WriteFileData(file.Info.Index, file.Info.FileId.Value, file.Encode());
        }

        public override void Dispose()
        {
            base.Dispose();

            if (this._fileStore != null)
            {
                this._fileStore.Dispose();
                this._fileStore = null;
            }
        }
    }
}
