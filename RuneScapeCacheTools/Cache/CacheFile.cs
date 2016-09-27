namespace Villermen.RuneScapeCacheTools.Cache
{
    public class CacheFile
    {
        public CacheFile(int indexId, int fileId, byte[][] entries, int version)
        {
            IndexId = indexId;
            FileId = fileId;
            Entries = entries;
            Version = version;
        }

        /// <summary>
        ///     Shorthand to get the first entry, which the full file in most cases.
        /// </summary>
        public byte[] Data => Entries[0];

        /// <summary>
        ///     The cache index this file originated from.
        /// </summary>
        public int IndexId { get; }

        /// <summary>
        ///     The file id within the index this file originated from.
        /// </summary>
        public int FileId { get; }

        /// <summary>
        ///     The individual data entries in this file.
        ///     Most files only contain one entry.
        /// </summary>
        public byte[][] Entries { get; }

        /// <summary>
        ///     The version of the file within the cache.
        ///     Sometimes a unix timestamp is used to express this value.
        /// </summary>
        public int Version { get; }
    }
}