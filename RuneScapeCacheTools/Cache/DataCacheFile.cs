using System;

namespace Villermen.RuneScapeCacheTools.Cache
{
    public class DataCacheFile : CacheFile
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

        protected override void Decode(byte[] data)
        {
            throw new NotSupportedException("Decoding a data cache file from another data cache file seems redundant.");
        }

        protected override byte[] Encode()
        {
            throw new NotSupportedException("Encoding a data cache file to another data cache file seems redundant.");
        }
    }
}