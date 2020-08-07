using System;
using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.Cache
{
    public class NxtClientCache : ReferenceTableCache
    {
        public static string DefaultCacheDirectory => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "/Jagex/RuneScape/";

        /// <summary>
        /// The directory where the cache is located.
        /// </summary>
        public string CacheDirectory { get; }

        public bool ReadOnly { get; }

        private object _ioLock = new object();

        public NxtClientCache(string? cacheDirectory = null, bool readOnly = true)
        {
            this.CacheDirectory = PathExtensions.FixDirectory(cacheDirectory ?? NxtClientCache.DefaultCacheDirectory);
            this.ReadOnly = readOnly;

            // Create the cache directory if writing is allowed.
            if (!this.ReadOnly)
            {
                Directory.CreateDirectory(this.CacheDirectory);
            }
        }

        public override IEnumerable<CacheIndex> GetAvailableIndexes()
        {
            throw new System.NotImplementedException();
        }

        public override byte[] GetFileData(CacheIndex index, int fileId)
        {
            throw new System.NotImplementedException();
        }

        protected override void PutFileData(CacheIndex index, int fileId, byte[] data)
        {
            throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}
