using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    ///     Base class for current cache systems.
    ///     For cache structures expressing the notion of indexes and archives.
    /// </summary>
    public abstract class CacheBase : IDisposable
    {
        public abstract IEnumerable<Index> Indexes { get; }

        /// <summary>
        ///     Returns the requested file and tries to convert it to the requested type if possible.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public T GetFile<T>(Index index, int fileId) where T : CacheFile
        {
            // Obtain the file /entry
            var file = this.FetchFile(index, fileId);

            // These we know
            file.Info.Index = index;
            file.Info.FileId = fileId;

            // Return the file as is when a binary file is requested
            if (typeof(T) == typeof(BinaryFile))
            {
                return file as T;
            }

            // Decode the file to the requested type
            var decodedFile = Activator.CreateInstance<T>();
            decodedFile.FromBinaryFile(file);
            return decodedFile;
        }

        /// <summary>
        /// Implements the logic for actually retrieving a file from the cache.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        protected abstract BinaryFile FetchFile(Index index, int fileId);

        public abstract IEnumerable<int> GetFileIds(Index index);

        public void PutFile(CacheFile file)
        {
            if (file.Info.EntryId != -1)
            {
                throw new ArgumentException("Entries can not be directly written to the cache. Use an entry file containing entries or remove the entry id from its info.");
            }

            this.PutFile(file.ToBinaryFile());
        }

        protected abstract void PutFile(BinaryFile file);

        /// <summary>
        ///     Returns info on the file without actually obtaining the file.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public abstract CacheFileInfo GetFileInfo(Index index, int fileId);

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}