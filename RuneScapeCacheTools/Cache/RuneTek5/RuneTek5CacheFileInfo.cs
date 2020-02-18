using System.Collections.Generic;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.File
{
    /// <summary>
    /// Contains information on a <see cref="RuneTek5CacheFile" /> file. Required to decode and encode the file.
    /// </summary>
    public class RuneTek5CacheFileInfo
    {
        // TODO: Only used for reference tables?
        // public CacheFileOptions Options { get; set; }
        // public SortedDictionary<int, CacheFileEntryInfo> EntryInfo { get; set; } =
        //     new SortedDictionary<int, CacheFileEntryInfo>();
        // public int? FileId { get; set; }
        // public CacheIndex CacheIndex { get; set; } = CacheIndex.Undefined;

        /// <summary>
        /// The compressed size of this entry in bytes.
        /// </summary>
        public int? CompressedSize { get; set; }

        /// <summary>
        /// The uncompressed size of this entry in bytes.
        /// </summary>
        public int? UncompressedSize { get; set; }

        public CompressionType CompressionType { get; set; } = CompressionType.None;

        /// <summary>
        /// CRC checksum for this file. Does not include the files version that may be appended to the file's data.
        /// </summary>
        public int? Crc { get; set; }

        /// <summary>
        /// Some unknown hash added on build 816. It is hard to pinpoint what exactly this is because it is not used in
        /// the client.
        /// </summary>
        public int? MysteryHash { get; set; }

        /// <summary>
        /// Version of the file. Sometimes a unix timestamp is used here and sometimes it is manually versioned.
        /// </summary>
        public int? Version { get; set; }

        public byte[] WhirlpoolDigest { get; set; }

        /// <summary>
        /// Key used for encrypting and decrypting the file to and from cache.
        /// </summary>
        public byte[] EncryptionKey { get; set; }

        /// <summary>
        /// A cache file can contain multiple entries
        /// </summary>
        public int EntryCount { get; set; } = 1;

        /// <summary>
        /// Creates a new <see cref="CacheFileInfo"/> with the same values as this one.
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
                CacheIndex = this.CacheIndex,
                MysteryHash = this.MysteryHash,
                UncompressedSize = this.UncompressedSize,
                Version = this.Version,
                WhirlpoolDigest = this.WhirlpoolDigest,
                EncryptionKey = this.EncryptionKey
            };
        }
    }
}
