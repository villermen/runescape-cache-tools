using System.Collections.Generic;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache.CacheFile;
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
    public class ReferenceTable : BaseCacheFile
    {
        /// <summary>
        ///     The entries in this table.
        /// </summary>
        private readonly Dictionary<int, CacheFileInfo> files = new Dictionary<int, CacheFileInfo>();

        public static ReferenceTable Decode(DataCacheFile cacheFile)
        {
            var referenceTable = new ReferenceTable
            {
                Info = cacheFile.Info
            };

            var reader = new BinaryReader(new MemoryStream(cacheFile.Data));

            referenceTable.Format = reader.ReadByte();

            // Read header
            if (referenceTable.Format < 5 || referenceTable.Format > 7)
            {
                throw new DecodeException($"Incorrect reference table format version {referenceTable.Format}.");
            }

            if (referenceTable.Format >= 6)
            {
                referenceTable.Version = reader.ReadInt32BigEndian();
            }

            referenceTable.Options = (CacheFileOptions)reader.ReadByte();

            // Read the ids of the files (delta encoded)
            var fileCount = referenceTable.Format >= 7 ? reader.ReadAwkwardInt() : reader.ReadUInt16BigEndian();

            var fileId = 0;
            for (var fileNumber = 0; fileNumber < fileCount; fileNumber++)
            {
                fileId += referenceTable.Format >= 7 ? reader.ReadAwkwardInt() : reader.ReadUInt16BigEndian();

                referenceTable.files.Add(fileId, new CacheFileInfo
                {
                    Index = (Index)referenceTable.Info.FileId,
                    FileId = fileId
                });
            }

            // Read the identifiers if present
            if (referenceTable.Options.HasFlag(CacheFileOptions.Identifiers))
            {
                foreach (var file in referenceTable.files.Values)
                {
                    file.Identifier = reader.ReadInt32BigEndian();
                }
            }

            // Read the CRC32 checksums
            foreach (var file in referenceTable.files.Values)
            {
                file.Crc = reader.ReadInt32BigEndian();
            }

            // Read some type of hash
            if (referenceTable.Options.HasFlag(CacheFileOptions.MysteryHashes))
            {
                foreach (var file in referenceTable.files.Values)
                {
                    file.MysteryHash = reader.ReadInt32BigEndian();
                }
            }

            // Read the whirlpool digests if present
            if (referenceTable.Options.HasFlag(CacheFileOptions.WhirlpoolDigests))
            {
                foreach (var file in referenceTable.files.Values)
                {
                    file.WhirlpoolDigest = reader.ReadBytes(64);
                }
            }

            // Read the compressed and uncompressed sizes
            if (referenceTable.Options.HasFlag(CacheFileOptions.Sizes))
            {
                foreach (var file in referenceTable.files.Values)
                {
                    file.CompressedSize = reader.ReadInt32BigEndian();
                    file.UncompressedSize = reader.ReadInt32BigEndian();
                }
            }

            // Read the version numbers
            foreach (var file in referenceTable.files.Values)
            {
                file.Version = reader.ReadInt32BigEndian();
            }

            // Read the entry counts
            var entryCounts = new Dictionary<int, int>();
            foreach (var file in referenceTable.files.Values)
            {
                entryCounts.Add(file.FileId, referenceTable.Format >= 7 ? reader.ReadAwkwardInt() : reader.ReadUInt16BigEndian());
            }

            // Read the delta encoded entry ids
            foreach (var entryCountPair in entryCounts)
            {
                var entryCountFileId = entryCountPair.Key;
                var entryCount = entryCountPair.Value;

                var entryId = 0;
                for (var entryNumber = 0; entryNumber < entryCount; entryNumber++)
                {
                    entryId += referenceTable.Format >= 7 ? reader.ReadAwkwardInt() : reader.ReadUInt16BigEndian();

                    referenceTable.files[entryCountFileId].Entries.Add(entryId, new CacheFileEntryInfo
                    {
                        EntryId = entryId
                    });
                }
            }

            // Read the entry identifiers if present
            if (referenceTable.Options.HasFlag(CacheFileOptions.Identifiers))
            {
                foreach (var file in referenceTable.files.Values)
                {
                    foreach (var entry in file.Entries.Values)
                    {
                        entry.Identifier = reader.ReadInt32BigEndian();
                    }
                }
            }

            return referenceTable;
        }

        /// <summary>
        /// Gets the ids of the files listed in this <see cref="ReferenceTable"/>.
        /// </summary>
        /// <returns></returns>
        public int[] FileIds => this.files.Keys.ToArray();

        /// <summary>
        ///     The format of this table.
        /// </summary>
        public byte Format { get; set; }

        /// <summary>
        ///     The flags of this table.
        /// </summary>
        public CacheFileOptions Options { get; set; }

        /// <summary>
        ///     The version of this table.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets the <see cref="CacheFileInfo"/> for the given file in the index described by this <see cref="ReferenceTable"/>.
        /// </summary>
        /// <returns></returns>
        public CacheFileInfo GetFileInfo(int fileId)
        {
            return this.files[fileId].Clone();
        }

        internal void SetFileInfo(int fileId, CacheFileInfo info)
        {
            if (this.files.ContainsKey(fileId))
            {
                this.files[fileId] = info;
            }
            else
            {
                this.files.Add(fileId, info);
            }
        }

        public DataCacheFile Encode()
        {
            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);

            // Write format
            writer.Write(this.Format);

            // Write version
            if (this.Format >= 6)
            {
                writer.WriteInt32BigEndian(this.Version);
            }

            // Write amount of files
            writer.Write((byte)this.Options);
            if (this.Format >= 7)
            {
                writer.WriteAwkwardInt(this.files.Count);
            }
            else
            {
                writer.WriteUInt16BigEndian((ushort)this.files.Count);
            }

            // Write delta encoded file ids
            var previousFileId = 0;
            foreach (var fileId in this.FileIds)
            {
                var delta = fileId - previousFileId;

                if (this.Format >= 7)
                {
                    writer.WriteAwkwardInt(delta);
                }
                else
                {
                    writer.WriteUInt16BigEndian((ushort)delta);
                }

                previousFileId = fileId;
            }

            // Write identifiers if option is set
            if (this.Options.HasFlag(CacheFileOptions.Identifiers))
            {
                foreach (var file in this.files.Values)
                {
                    writer.WriteInt32BigEndian(file.Identifier);
                }
            }

            // Write CRC checksums
            foreach (var file in this.files.Values)
            {
                writer.WriteInt32BigEndian(file.Crc.Value);
            }

            // Write some type of hash
            if (this.Options.HasFlag(CacheFileOptions.MysteryHashes))
            {
                foreach (var file in this.files.Values)
                {
                    writer.WriteInt32BigEndian(file.MysteryHash);
                }
            }

            // Write the whirlpool digests if option is set
            if (this.Options.HasFlag(CacheFileOptions.WhirlpoolDigests))
            {
                foreach (var file in this.files.Values)
                {
                    // Do a small check to verify the size before messing the whole file up
                    if (file.WhirlpoolDigest.Length != 64)
                    {
                        throw new DecodeException("File info's whirlpool digest is not 64 bytes in length.");
                    }

                    writer.Write(file.WhirlpoolDigest);
                }
            }

            // Write the compressed and uncompressed sizes if option is specified
            if (this.Options.HasFlag(CacheFileOptions.Sizes))
            {
                foreach (var file in this.files.Values)
                {
                    writer.WriteInt32BigEndian(file.CompressedSize);
                    writer.WriteInt32BigEndian(file.UncompressedSize);
                }
            }

            // Write the version numbers
            foreach (var file in this.files.Values)
            {
                writer.WriteInt32BigEndian(file.Version);
            }

            // Write the entry counts
            foreach (var file in this.files.Values)
            {
                if (this.Format >= 7)
                {
                    writer.WriteAwkwardInt(file.Entries.Count);
                }
                else
                {
                    writer.WriteUInt16BigEndian((ushort)file.Entries.Count);
                }
            }

            // Write the delta encoded entry ids
            foreach (var file in this.files.Values)
            {
                var previousEntryId = 0;
                foreach (var entryId in file.Entries.Keys)
                {
                    var delta = entryId - previousEntryId;

                    if (this.Format >= 7)
                    {
                        writer.WriteAwkwardInt(delta);
                    }
                    else
                    {
                        writer.WriteUInt16BigEndian((ushort)delta);
                    }

                    previousEntryId = entryId;
                }
            }

            // Write the entry identifiers if option is specified
            if (this.Options.HasFlag(CacheFileOptions.Identifiers))
            {
                foreach (var file in this.files.Values)
                {
                    foreach (var entry in file.Entries.Values)
                    {
                        writer.WriteInt32BigEndian(entry.Identifier);
                    }
                }
            }

            return new DataCacheFile
            {
                Data = memoryStream.ToArray(),
                Info = this.Info
            };
        }
    }
}