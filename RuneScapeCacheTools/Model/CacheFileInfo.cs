using System.Collections.Generic;
using System.Linq;

namespace Villermen.RuneScapeCacheTools.Model
{
    /// <summary>
    /// Contains detailed information on a file in the cache.
    /// </summary>
    public class CacheFileInfo
    {
        public CacheFileOptions Options { get; set; }

        /// <summary>
        /// The compressed size of this entry.
        /// </summary>
        public int? CompressedSize { get; set; }

        public CompressionType CompressionType { get; set; } = CompressionType.None;

        public int? Crc { get; set; }

        /// <summary>
        /// The children in this entry.
        /// </summary>
        public SortedDictionary<int, CacheFileEntryInfo> EntryInfo { get; set; } =
            new SortedDictionary<int, CacheFileEntryInfo>();

        public int? FileId { get; set; }

        /// <summary>
        /// If this file is an entry, this will be set to its index.
        /// </summary>
        public int? EntryId { get; set; }

        public int? Identifier { get; set; }

        public Index Index { get; set; } = Index.Undefined;

        /// <summary>
        /// Some unknown hash added on build 816.
        /// It is hard to pinpoint what exactly this is because it is not used in the client.
        /// </summary>
        public int? MysteryHash { get; set; }

        /// <summary>
        /// The uncompressed size of this entry.
        /// </summary>
        public int? UncompressedSize { get; set; }

        public int? Version { get; set; }

        public byte[] WhirlpoolDigest { get; set; }

        /// <summary>
        /// Key used for encrypting and decrypting the file to and from cache.
        /// </summary>
        public byte[] EncryptionKey { get; set; }

        /// <summary>
        /// A file is an entry file when there are multiple entries defined in the info.
        /// A non-entry file only has one entry defined.
        /// </summary>
        public bool UsesEntries => this.EntryInfo.Count > 1;

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
                EntryInfo = new SortedDictionary<int, CacheFileEntryInfo>(this.EntryInfo.ToDictionary(
                    entryInfoPair => entryInfoPair.Key,
                    entryInfoPair => entryInfoPair.Value.Clone())),
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
