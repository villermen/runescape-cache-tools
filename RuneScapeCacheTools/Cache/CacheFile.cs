namespace Villermen.RuneScapeCacheTools.Cache
{
    using System;
    using System.Text;

    public class CacheFile
    {
        public CacheFile()
        {
        }

        public CacheFile(byte[] data, CacheFileInfo info)
            : this(new byte[][] { data }, info)
        {
        }

        public CacheFile(byte[][] entries, CacheFileInfo info)
        {
            this.Entries = entries;
            this.Info = info;
        }

        /// <summary>
        ///     Shorthand to get the first entry, which the full file in most cases.
        /// </summary>
        public byte[] Data
        {
            get { return this.Entries[0]; }
            set { this.Entries = new byte[][] { value }; }
        }

        /// <summary>
        ///     The individual data entries in this file.
        ///     Most files only contain one entry.
        /// </summary>
        public byte[][] Entries { get; set; } = new byte[1][];

        public CacheFileInfo Info { get; set; }
    }
}