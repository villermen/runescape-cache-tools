using System;
using System.Collections.Generic;
using System.Linq;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.File
{
    /// <summary>
    /// A file in the cache containing (decoded) binary data.
    /// </summary>
    public class CacheFile
    {
        public readonly CacheFileInfo Info;

        public readonly Dictionary<int, byte[]> Entries = new Dictionary<int, byte[]>();

        public bool HasEntries => this.Entries.Count > 1;

        /// <summary>
        /// The decoded data of this cache file. Convenience property that encodes the backing <see cref="Entries" /> if
        /// entries are being used.
        /// </summary>
        public byte[] Data
        {
            get
            {
                if (!this.HasEntries)
                {
                    return this.Entries[0];
                }

                return new RuneTek5CacheFileDecoder().EncodeEntries(this.Entries, null);
            }
            set
            {
                this.Entries.Clear();
                this.Entries[0] = value;
            }
        }

        public CacheFile()
        {
            this.Info = new CacheFileInfo();
        }

        public CacheFile(byte[] data, CacheFileInfo? info = null)
        {
            this.Info = info ?? new CacheFileInfo();
            this.Data = data;
        }

        public CacheFile(IEnumerable<KeyValuePair<int, byte[]>> entries, CacheFileInfo info)
        {
            if (!info.HasEntries)
            {
                throw new ArgumentException("Passed info does not specify any entries.");
            }

            this.Info = info;

            foreach (var entryPair in entries)
            {
                this.Entries.Add(entryPair.Key, entryPair.Value);
            }

            if (!this.Entries.Keys.SequenceEqual(info.Entries.Keys))
            {
                var message = $"Cache file expects {info.Entries.Count} entries. {this.Entries.Count} passed.";
                if (this.Entries.Count == info.Entries.Count)
                {
                    message = "Cache file entry IDs do not match info's IDs.";
                }

                throw new ArgumentException(message);
            }
        }
    }
}
