using System.Collections.Generic;
using System.Linq;
using Villermen.RuneScapeCacheTools.File;

namespace Villermen.RuneScapeCacheTools.Model
{
    /// <summary>
    /// Contains information on a <see cref="CacheFile" /> file. Required to decode and encode the file.
    /// </summary>
    public class CacheFileInfo
    {
        /// <summary>
        /// The type of compression used to store this file in the cache.
        /// </summary>
        public CompressionType CompressionType { get; set; } = CompressionType.None;

        /// <summary>
        /// The compressed size of this entry in bytes.
        /// </summary>
        public int? CompressedSize { get; set; }

        /// <summary>
        /// The uncompressed size of this entry in bytes.
        /// </summary>
        public int? UncompressedSize { get; set; }

        /// <summary>
        /// CRC checksum for this file. Does not include the files version that may be appended to the file's data.
        /// </summary>
        public int? Crc { get; set; }

        /// <summary>
        /// Version of the file. Sometimes a unix timestamp is used here and sometimes it is manually versioned.
        /// </summary>
        public int? Version { get; set; }

        /// <summary>
        /// Some unknown hash added on build 816. It is hard to pinpoint what exactly this is because it is not used in
        /// the client.
        /// </summary>
        public int? MysteryHash { get; set; }

        public byte[] WhirlpoolDigest { get; set; }

        /// <summary>
        /// Key used for encrypting and decrypting the file to and from cache.
        /// </summary>
        public byte[] EncryptionKey { get; set; }

        // TODO: Verify (again) that files without entries define 1 or 0 entries. Which could help in creating 👇
        // TODO: HasEntries or something

        /// <summary>
        /// TODO: Find out what identifiers are actually used for.
        /// </summary>
        public int? Identifier { get; set; }

        /// <summary>
        /// The entry IDs mapped to their additional info if this file contains entries.
        /// </summary>
        public SortedDictionary<int, CacheFileEntryInfo> Entries { get; set; } = new SortedDictionary<int, CacheFileEntryInfo>();

        public bool HasEntries => this.Entries.Count > 1;

        /// <summary>
        /// Creates a new <see cref="CacheFileInfo"/> with the same values as this one.
        /// </summary>
        public CacheFileInfo Clone()
        {
            return new CacheFileInfo
            {
                CompressedSize = this.CompressedSize,
                CompressionType = this.CompressionType,
                Crc = this.Crc,
                Version = this.Version,
                Identifier = this.Identifier,
                MysteryHash = this.MysteryHash,
                UncompressedSize = this.UncompressedSize,
                WhirlpoolDigest = this.WhirlpoolDigest,
                EncryptionKey = this.EncryptionKey,
                Entries = new SortedDictionary<int, CacheFileEntryInfo>(this.Entries.ToDictionary(
                    entryInfoPair => entryInfoPair.Key,
                    entryInfoPair => entryInfoPair.Value.Clone()
                )),
            };
        }
    }
}
