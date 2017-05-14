using System;
using System.Collections.Generic;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;

namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    /// Base class for cache systems.
    /// </summary>
    public abstract class CacheBase : IDisposable
    {
        /// <summary>
        /// Returns the indexes available in the cache.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<Index> GetIndexes();

        /// <summary>
        /// Returns the files available in the given index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public abstract IEnumerable<int> GetFileIds(Index index);

        /// <summary>
        /// Returns info on the specified file without actually obtaining the file.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public abstract CacheFileInfo GetFileInfo(Index index, int fileId);

        /// <summary>
        /// Returns the requested file converted to the requested type.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public T GetFile<T>(Index index, int fileId) where T : CacheFile
        {
            // Obtain the file /entry
            var file = this.GetFile(index, fileId);

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
        protected abstract BinaryFile GetFile(Index index, int fileId);

        /// <summary>
        /// Writes a file to the cache.
        /// The file's info will be used to determine where and how to put the file in the cache.
        /// </summary>
        /// <param name="file"></param>
        /// <exception cref="ArgumentException"></exception>
        public void PutFile(CacheFile file)
        {
            if (file.Info.EntryId != -1)
            {
                throw new ArgumentException("Entries can not be directly written to the cache. Use an entry file containing entries or remove the entry id from its info.");
            }

            this.PutFile(file.ToBinaryFile());
        }

        protected abstract void PutFile(BinaryFile file);

        public void CopyFile(Index index, int fileId, CacheBase cache)
        {
            cache.PutFile(this.GetFile(index, fileId));
        }

        public virtual void Dispose() { }
    }
}
