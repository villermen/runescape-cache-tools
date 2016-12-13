using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    ///     A <see cref="ReferenceTable" /> holds metadata for all registered files in an index, such as checksums, versions
    ///     and archive members.
    ///     Note that the data of registered files does not have to be present in the index for them to be listed.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Sean</author>
    /// <author>Villermen</author>
    public class ReferenceTable
    {
        /// <summary>
        ///     The entries in this table.
        /// </summary>
        private readonly Dictionary<int, CacheFileInfo> files = new Dictionary<int, CacheFileInfo>();

        /// <summary>
        ///     Decodes the reference table contained in the given data.
        /// </summary>
        /// <param name="cacheFile"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public ReferenceTable(CacheFile cacheFile, Index index)
        {
            var reader = new BinaryReader(new MemoryStream(cacheFile.Data));

            this.Format = reader.ReadByte();

            // Read header
            if ((this.Format < 5) || (this.Format > 7))
            {
                throw new CacheException($"Incorrect reference table protocol number: {this.Format}.");
            }

            if (this.Format >= 6)
            {
                this.Version = reader.ReadInt32BigEndian();
            }

            this.Options = (CacheFileOptions)reader.ReadByte();

            // Read the ids of the files (delta encoded)
            var fileCount = this.Format >= 7 ? reader.ReadSmartInt() : reader.ReadUInt16BigEndian();

            var fileId = 0;
            for (var fileNumber = 0; fileNumber < fileCount; fileNumber++)
            {
                fileId += this.Format >= 7 ? reader.ReadSmartInt() : reader.ReadUInt16BigEndian();

                this.files.Add(fileId, new CacheFileInfo
                {
                    Index = index,
                    FileId = fileId
                });
            }

            // Read the identifiers if present
            if ((this.Options & CacheFileOptions.Identifiers) != 0)
            {
                foreach (var file in this.files.Values)
                {
                    file.Identifier = reader.ReadInt32BigEndian();
                }
            }

            // Read the CRC32 checksums
            foreach (var file in this.files.Values)
            {
                file.CRC = reader.ReadInt32BigEndian();
            }

            // Read some type of hash
            if ((this.Options & CacheFileOptions.MysteryHashes) != 0)
            {
                foreach (var file in this.files.Values)
                {
                    file.MysteryHash = reader.ReadInt32BigEndian();
                }
            }

            // Read the whirlpool digests if present
            if ((this.Options & CacheFileOptions.WhirlpoolDigests) != 0)
            {
                foreach (var file in this.files.Values)
                {
                    file.WhirlpoolDigest = reader.ReadBytes(64);
                }
            }

            // Read the compressed and uncompressed sizes
            if ((this.Options & CacheFileOptions.Sizes) != 0)
            {
                foreach (var file in this.files.Values)
                {
                    file.CompressedSize = reader.ReadInt32BigEndian();
                    file.UncompressedSize = reader.ReadInt32BigEndian();
                }
            }

            // Read the version numbers
            foreach (var file in this.files.Values)
            {
                var version = reader.ReadInt32BigEndian();
                file.Version = version;
            }

            // Read the entry counts
            var entryCounts = new Dictionary<int, int>();
            foreach (var file in this.files.Values)
            {
                entryCounts.Add(file.FileId, this.Format >= 7 ? reader.ReadSmartInt() : reader.ReadUInt16BigEndian());
            }

            // Read the entry ids (delta encoded)
            foreach (var entryCountPair in entryCounts)
            {
                var entryCountFileId = entryCountPair.Key;
                var entryCount = entryCountPair.Value;

                var entryId = 0;
                for (var entryNumber = 0; entryNumber < entryCount; entryNumber++)
                {
                    entryId += this.Format >= 7 ? reader.ReadSmartInt() : reader.ReadUInt16BigEndian();

                    this.files[entryCountFileId].Entries.Add(entryId, new CacheFileEntryInfo
                    {
                        EntryId = entryId
                    });
                }
            }

            // Read the entry identifiers if present
            if ((this.Options & CacheFileOptions.Identifiers) != 0)
            {
                foreach (var file in this.files.Values)
                {
                    foreach (var entry in file.Entries.Values)
                    {
                        entry.Identifier = reader.ReadInt32BigEndian();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the ids of the files listed in this <see cref="ReferenceTable"/>.
        /// </summary>
        /// <returns></returns>
        public int[] FileIds => this.files.Keys.ToArray();

        /// <summary>
        /// Gets the <see cref="CacheFileInfo"/> for the given file in the index described by this <see cref="ReferenceTable"/>.
        /// </summary>
        /// <returns></returns>
        public CacheFileInfo GetFileInfo(int fileId)
        {
            return this.files[fileId].Clone();
        }

        /// <summary>
        ///     The format of this table.
        /// </summary>
        public int Format { get; set; }

        /// <summary>
        ///     The flags of this table.
        /// </summary>
        public CacheFileOptions Options { get; set; }

        /// <summary>
        ///     The version of this table.
        /// </summary>
        public int Version { get; set; }
    }
}