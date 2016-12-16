namespace Villermen.RuneScapeCacheTools.Cache.CacheFile
{
    using System;

    public class DataCacheFile : BaseCacheFile
    {
        public bool UsesEntries => this.Entries.Length > 1;

        public byte[] Data
        {
            get
            {
                if (this.UsesEntries)
                {
                    throw new InvalidOperationException($"This {nameof(DataCacheFile)} uses entries, and data can't be accessed using {nameof(this.Data)}.");
                }

                return this.Entries[0];
            }
            set { this.Entries = new byte[][] { value }; }
        }

        public byte[][] Entries { get; set; } = new byte[1][];
    }
}