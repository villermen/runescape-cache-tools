using System.Collections.Generic;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;

namespace Villermen.RuneScapeCacheTools.Cache
{
    public class CacheFileInfo
    {
        public CacheFileOptions Options;

        /// <summary>
        ///     The compressed size of this entry.
        /// </summary>
        public int CompressedSize { get; set; }

        public CompressionType CompressionType { get; set; }

        public int CRC { get; set; }

        /// <summary>
        ///     The children in this entry.
        /// </summary>
        public IDictionary<int, CacheFileEntryInfo> Entries { get; } = new Dictionary<int, CacheFileEntryInfo>();

        public int FileId { get; set; } = -1;

        public int Identifier { get; set; }

        public Index Index { get; set; }

        /// <summary>
        ///     Some unknown hash added on build 816.
        ///     It is hard to pinpoint what exactly this is because it is not used in the client.
        /// </summary>
        public int MysteryHash { get; set; }

        /// <summary>
        ///     The uncompressed size of this entry.
        /// </summary>
        public int UncompressedSize { get; set; }

        public int Version { get; set; } = -1;

        public byte[] WhirlpoolDigest { get; set; }
    }
}