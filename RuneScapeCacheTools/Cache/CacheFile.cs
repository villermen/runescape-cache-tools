namespace Villermen.RuneScapeCacheTools.Cache
{
    public class CacheFile
    {
        public CacheFile()
        {
        }

        public CacheFile(byte[][] entries)
        {
            this.Entries = entries;
        }

        /// <summary>
        ///     Shorthand to get the first entry, which the full file in most cases.
        /// </summary>
        public byte[] Data => this.Entries[0];

        /// <summary>
        ///     The individual data entries in this file.
        ///     Most files only contain one entry.
        /// </summary>
        public byte[][] Entries { get; set; }

        public CacheFileInfo Info { get; set; } = new CacheFileInfo();
    }
}