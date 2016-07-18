using System;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    ///     The <see cref="Cache" /> class provides a unified, high-level API for modifying the cache of a Jagex game.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Villermen</author>
    public class Cache
    {
        /// <summary>
        ///     Creates a new <see cref="Cache" /> backed by the given <see cref="RuneTek5.FileStore" />.
        /// </summary>
        /// <param name="fileStore"></param>
        public Cache(FileStore fileStore)
        {
            FileStore = fileStore;
        }

        /// <summary>
        ///     The <see cref="RuneTek5.FileStore" /> that backs this cache.
        /// </summary>
        public FileStore FileStore { get; }

        public int IndexCount => FileStore.IndexCount;

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
        public int GetFileCount(int indexId)
        {
            return FileStore.GetFileCount(indexId);
        }

        /// <summary>
        ///     Gets a file id from the cache by name.
        /// </summary>
        /// <returns></returns>
        public int GetFileIdByName(int indexId, string fileName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Writes a file to the cache and updates the <see cref="ReferenceTable" /> that it is associated with to match.
        /// </summary>
        /// <param name="indexId"></param>
        /// <param name="fileId"></param>
        /// <param name="container"></param>
        public void WriteFile(int indexId, int fileId, Container container)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Writes a file contained in an archive to the cache.
        /// </summary>
        /// <param name="indexId"></param>
        /// <param name="archiveId"></param>
        /// <param name="fileId"></param>
        /// <param name="data"></param>
        public void WriteArchiveFile(int indexId, int archiveId, int fileId, byte[] data)
        {
            throw new NotImplementedException();
        }

        public int FindFileName(ReferenceTable referenceTable, string fileName)
        {
            throw new NotImplementedException();
        }

        public int FindFileName(ReferenceTable referenceTable, string fileName, int fileId)
        {
            throw new NotImplementedException();
        }

        public int FindFileName(ReferenceTable referenceTable, string fileName, int archiveId, int fileId)
        {
            throw new NotImplementedException();
        }

        public int GetContainerCount(int indexId, int fileId)
        {
            throw new NotImplementedException();
        }
    }
}