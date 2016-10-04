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
        /// <summary>
        ///     Decodes the reference table contained in the given data.
        /// </summary>
        /// <param name="cacheFile"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public ReferenceTable(CacheFile cacheFile, Index index)
        {
            var reader = new BinaryReader(new MemoryStream(cacheFile.Data));

            Format = reader.ReadByte();

            // Read header
            if ((Format < 5) || (Format > 7))
            {
                throw new CacheException($"Incorrect reference table protocol number: {Format}.");
            }

            if (Format >= 6)
            {
                Version = reader.ReadInt32BigEndian();
            }

            Options = (CacheFileOptions)reader.ReadByte();

            // Read the ids of the files (delta encoded)
            var fileCount = Format >= 7 ? reader.ReadSmartInt() : reader.ReadUInt16BigEndian();

            var fileId = 0;
            for (var fileNumber = 0; fileNumber < fileCount; fileNumber++)
            {
                fileId += Format >= 7 ? reader.ReadSmartInt() : reader.ReadUInt16BigEndian();

                Files.Add(fileId, new ReferenceTableFile(index, fileId));
            }

            // Read the identifiers if present
            if ((Options & CacheFileOptions.Identifiers) != 0)
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
            if ((Options & CacheFileOptions.MysteryHashes) != 0)
            {
                foreach (var file in Files.Values)
                {
                    file.MysteryHash = reader.ReadInt32BigEndian();
                }
            }

            // Read the whirlpool digests if present
            if ((Options & CacheFileOptions.WhirlpoolDigests) != 0)
            {
                foreach (var file in Files.Values)
                {
                    file.WhirlpoolDigest = reader.ReadBytes(64);
                }
            }

            // Read the compressed and uncompressed sizes
            if ((Options & CacheFileOptions.Sizes) != 0)
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
            foreach (var file in Files.Values)
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
            if ((Options & CacheFileOptions.Identifiers) != 0)
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
        ///     The entries in this table.
        /// </summary>
        public IDictionary<int, ReferenceTableFile> Files { get; } = new Dictionary<int, ReferenceTableFile>();

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