namespace Villermen.RuneScapeCacheTools.Cache
{
    using System;
    using System.Text;

    public class CacheFile
    {
        public CacheFile()
        {
        }

        public CacheFile(byte[] data)
        {
            this.Entries = new byte[][] { data };
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

        /// <summary>
        /// Can be overwritten in derived classes to encode the data and information contained in this class into a byte array.
        /// </summary>
        /// <returns></returns>
        public virtual byte[] Encode()
        {
            throw new NotSupportedException("This cache file does not support encoding to binary data.");
        }
    }
}