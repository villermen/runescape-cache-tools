using System;
using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
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
        [Flags]
        public enum DataFlags
        {
            /// <summary>
            ///     A flag which indicates this <see cref="ReferenceTable" /> contains Djb2 hashed identifiers.
            /// </summary>
            Identifiers = 0x01,

            /// <summary>
            ///     A flag which indicates this <see cref="ReferenceTable" />} contains whirlpool digests for its entries.
            /// </summary>
            WhirlpoolDigests = 0x02,

            /// <summary>
            ///     A flag which indicates this <see cref="ReferenceTable" /> contains sizes for its entries.
            /// </summary>
            Sizes = 0x04,

            /// <summary>
            ///     A flag which indicates this <see cref="ReferenceTable" /> contains some kind of hash which is currently unused by
            ///     the RuneScape client.
            /// </summary>
            MysteryHashes = 0x08
        }

        /// <summary>
        ///     Decodes the reference table contained in the given data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="indexId"></param>
        /// <returns></returns>
        public ReferenceTable(byte[] data, int indexId)
        {
            var reader = new BinaryReader(new MemoryStream(data));

            Format = reader.ReadByte();

            // Read header
            if (Format < 5 || Format > 7)
            {
                throw new CacheException($"Incorrect reference table protocol number: {Format}.");
            }

            if (Format >= 6)
            {
                Version = reader.ReadInt32BigEndian();
            }

            Flags = (DataFlags)reader.ReadByte();

            // Read the ids of the files (delta encoded)
            var fileCount = Format >= 7 ? reader.ReadSmartInt() : reader.ReadUInt16BigEndian();

            var fileId = 0;
            for (var fileNumber = 0; fileNumber < fileCount; fileNumber++)
            {
                fileId += Format >= 7 ? reader.ReadSmartInt() : reader.ReadUInt16BigEndian();

                Files.Add(fileId, new ReferenceTableFile(indexId, fileId));
            }

            // Read the identifiers if present
            if ((Flags & DataFlags.Identifiers) != 0)
            {
                foreach (var file in Files.Values)
                {
                    file.Identifier = reader.ReadInt32BigEndian();
                }
            }

            // Read the CRC32 checksums
            foreach (var file in Files.Values)
            {
                file.CRC = reader.ReadInt32BigEndian();
            }

            // Read some type of hash
            if ((Flags & DataFlags.MysteryHashes) != 0)
            {
                foreach (var file in Files.Values)
                {
                    file.MysteryHash = reader.ReadInt32BigEndian();
                }
            }

            // Read the whirlpool digests if present
            if ((Flags & DataFlags.WhirlpoolDigests) != 0)
            {
                foreach (var file in Files.Values)
                {
                    file.Whirlpool = reader.ReadBytes(64);
                }
            }

            // Read the compressed and uncompressed sizes
            if ((Flags & DataFlags.Sizes) != 0)
            {
                foreach (var file in Files.Values)
                {
                    file.CompressedSize = reader.ReadInt32BigEndian();
                    file.UncompressedSize = reader.ReadInt32BigEndian();
                }
            }

            // Read the version numbers
            foreach (var file in Files.Values)
            {
                var version = reader.ReadInt32BigEndian();
                file.Version = version;
            }

            // Read the entry counts
            var entryCounts = new Dictionary<int, int>();
            foreach(var file in Files.Values)
            {
                entryCounts.Add(file.Id, Format >= 7 ? reader.ReadSmartInt() : reader.ReadUInt16BigEndian());
            }

            // Read the entry ids (delta encoded)
            foreach (var entryCountPair in entryCounts)
            {
                var entryCountFileId = entryCountPair.Key;
                var entryCount = entryCountPair.Value;

                var entryId = 0;
                for (var entryNumber = 0; entryNumber < entryCount; entryNumber++)
                {
                    entryId += Format >= 7 ? reader.ReadSmartInt() : reader.ReadUInt16BigEndian();
                    Files[entryCountFileId].Entries.Add(entryId, new ReferenceTableFileEntry(entryId));
                }
            }

            // Read the entry identifiers if present
            if ((Flags & DataFlags.Identifiers) != 0)
            {
                foreach (var file in Files.Values)
                {
                    foreach (var entry in file.Entries.Values)
                    {
                        entry.Identifier = reader.ReadInt32BigEndian();
                    }
                }
            }
        }

        /// <summary>
        ///     The format of this table.
        /// </summary>
        public int Format { get; set; }

        /// <summary>
        ///     The version of this table.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        ///     The flags of this table.
        /// </summary>
        public DataFlags Flags { get; set; }

        /// <summary>
        ///     The entries in this table.
        /// </summary>
        public IDictionary<int, ReferenceTableFile> Files { get; } = new Dictionary<int, ReferenceTableFile>();
    }
}