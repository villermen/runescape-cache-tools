using System.Collections.Generic;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;

namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    /// Contains detailed information on a file in the cache.
    /// </summary>
    public class CacheFileInfo
    {
        public CacheFileOptions Options { get; set; }

        /// <summary>
        ///     The compressed size of this entry.
        /// </summary>
        public int CompressedSize { get; set; }

        public CompressionType CompressionType { get; set; }

        public int? Crc { get; set; }

        /// <summary>
        ///     The children in this entry.
        /// </summary>
        public IDictionary<int, CacheFileEntryInfo> Entries { get; set; } = new Dictionary<int, CacheFileEntryInfo>();

        public bool HasEntries => this.Entries.Count > 1;

        public int FileId { get; set; } = -1;

        public int Identifier { get; set; }

        public Index Index { get; set; } = Index.Undefined;

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

        /// <summary>
        /// Key used for encrypting and decrypting the file to and from cache.
        /// </summary>
        public byte[] EncryptionKey { get; set; }

        /// <summary>
        /// Creates a new <see cref="CacheFileInfo"/> with the same values as this one.
        /// Entries will also be cloned to the new object.
        /// </summary>
        /// <returns></returns>
        public CacheFileInfo Clone()
        {
            return new CacheFileInfo
            {
                Options = this.Options,
                CompressedSize = this.CompressedSize,
                CompressionType = this.CompressionType,
                Crc = this.Crc,
                Entries = this.Entries.Select(entryPair => new KeyValuePair<int, CacheFileEntryInfo>(entryPair.Key, entryPair.Value.Clone())).ToDictionary(entryPair => entryPair.Key, entryPair => entryPair.Value),
                FileId = this.FileId,
                Identifier = this.Identifier,
                Index = this.Index,
                MysteryHash = this.MysteryHash,
                UncompressedSize = this.UncompressedSize,
                Version = this.Version,
                WhirlpoolDigest = this.WhirlpoolDigest,
                EncryptionKey = this.EncryptionKey
            };
        }
    }
}