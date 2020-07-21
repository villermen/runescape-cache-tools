using System.Collections.Generic;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    /// Holds metadata for all registered files in an index. Note that the data of registered files does not have to be
    /// present in the index for the files to be listed in a <see cref="ReferenceTable" />.
    /// </summary>
    public class ReferenceTable
    {
        /// <exception cref="DecodeException"></exception>
        public static ReferenceTable Decode(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));

            var format = reader.ReadByte();
            if (format < 5 || format > 7)
            {
                throw new DecodeException($"Unsupported reference table format version \"{format}\".");
            }

            int? version = null;
            if (format >= 6)
            {
                version = reader.ReadInt32BigEndian();
            }

            var options = (ReferenceTableOptions)reader.ReadByte();

            var fileCount = format >= 7 ? reader.ReadAwkwardInt() : reader.ReadUInt16BigEndian();
            var fileInfos = new SortedDictionary<int, CacheFileInfo>();

            // Read delta encoded file IDs. Create empty info objects for them.
            var infoFileId = 0;
            for (var i = 0; i < fileCount; i++)
            {
                var delta = format >= 7 ? reader.ReadAwkwardInt() : reader.ReadUInt16BigEndian();
                infoFileId += delta;

                fileInfos.Add(infoFileId, new CacheFileInfo());
            }

            // Read file identifiers
            if (options.HasFlag(ReferenceTableOptions.Identifiers))
            {
                foreach (var fileInfo in fileInfos.Values)
                {
                    fileInfo.Identifier = reader.ReadInt32BigEndian();
                }
            }

            // Read file CRC32 checksums
            foreach (var fileInfo in fileInfos.Values)
            {
                fileInfo.Crc = reader.ReadInt32BigEndian();
            }

            // Read file mystery hashes
            if (options.HasFlag(ReferenceTableOptions.MysteryHashes))
            {
                foreach (var fileInfo in fileInfos.Values)
                {
                    fileInfo.MysteryHash = reader.ReadInt32BigEndian();
                }
            }

            // Read file whirlpool digests
            if (options.HasFlag(ReferenceTableOptions.WhirlpoolDigests))
            {
                foreach (var fileInfo in fileInfos.Values)
                {
                    fileInfo.WhirlpoolDigest = reader.ReadBytes(64);
                }
            }

            // Read file sizes
            if (options.HasFlag(ReferenceTableOptions.Sizes))
            {
                foreach (var fileInfo in fileInfos.Values)
                {
                    fileInfo.CompressedSize = reader.ReadInt32BigEndian();
                    fileInfo.UncompressedSize = reader.ReadInt32BigEndian();
                }
            }

            // Read file versions
            foreach (var fileInfo in fileInfos.Values)
            {
                fileInfo.Version = reader.ReadInt32BigEndian();
            }

            // Read file entry counts
            var entryCounts = new Dictionary<int, int>();
            foreach (var fileId in fileInfos.Keys)
            {
                entryCounts.Add(fileId, format >= 7 ? reader.ReadAwkwardInt() : reader.ReadUInt16BigEndian());
            }

            // Read delta encoded entry IDs. Create info objects for them.
            foreach (var entryCountPair in entryCounts)
            {
                var fileId = entryCountPair.Key;
                var entryCount = entryCountPair.Value;

                fileInfos[fileId].Entries = new SortedDictionary<int, CacheFileEntryInfo>();

                var entryId = 0;
                for (var i = 0; i < entryCount; i++)
                {
                    var delta = format >= 7 ? reader.ReadAwkwardInt() : reader.ReadUInt16BigEndian();
                    entryId += delta;

                    fileInfos[fileId].Entries.Add(entryId, new CacheFileEntryInfo());
                }
            }

            // Read the entry identifiers
            if (options.HasFlag(ReferenceTableOptions.Identifiers))
            {
                foreach (var fileInfo in fileInfos.Values)
                {
                    foreach (var entryInfo in fileInfo.Entries.Values)
                    {
                        entryInfo.Identifier = reader.ReadInt32BigEndian();
                    }
                }
            }

            if (reader.BaseStream.Position < reader.BaseStream.Length - 1)
            {
                throw new DecodeException(
                    $"Input data not fully consumed while decoding reference table. {reader.BaseStream.Length - 1 - reader.BaseStream.Position} bytes remain."
                );
            }

            return new ReferenceTable
            {
                Format = format,
                Version = version,
                Options = options,
                _fileInfos = fileInfos,
            };
        }

        /// <summary>
        /// The files described by this <see cref="ReferenceTable" />.
        /// </summary>
        private SortedDictionary<int, CacheFileInfo> _fileInfos = new SortedDictionary<int, CacheFileInfo>();

        /// <summary>
        /// Gets the IDs of the files listed in this <see cref="ReferenceTable" />.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> FileIds => this._fileInfos.Keys.ToArray();

        /// <summary>
        /// The format version of this table.
        /// </summary>
        public byte Format { get; set; } = 7;

        /// <summary>
        /// The flags of this table.
        /// </summary>
        public ReferenceTableOptions Options { get; set; }

        /// <summary>
        /// The version of this <see cref="ReferenceTable" />. Only available when <see cref="Format" /> >= 6.
        /// </summary>
        public int? Version { get; set; }

        /// <summary>
        /// Gets the <see cref="CacheFileInfo"/> for the given file in the index described by this
        /// <see cref="ReferenceTable" />. The info will be a clone and can be freely altered.
        /// </summary>
        public CacheFileInfo GetFileInfo(int fileId)
        {
            if (!this._fileInfos.ContainsKey(fileId))
            {
                throw new FileNotFoundException($"File {fileId} does not exist in this reference table.");
            }

            return this._fileInfos[fileId].Clone();
        }

        public void SetFileInfo(int fileId, CacheFileInfo info)
        {
            if (this._fileInfos.ContainsKey(fileId))
            {
                this._fileInfos[fileId] = info;
            }
            else
            {
                this._fileInfos.Add(fileId, info);
            }
        }

        public byte[] Encode()
        {
            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);

            // Write format
            writer.Write(this.Format);

            // Write version
            if (this.Format >= 6)
            {
                if (!this.Version.HasValue)
                {
                    throw new EncodeException("ReferenceTable version must be set if format is 6 or greater.");
                }

                writer.WriteInt32BigEndian(this.Version.Value);
            }

            // Write amount of files
            writer.Write((byte)this.Options);
            if (this.Format >= 7)
            {
                writer.WriteAwkwardInt(this._fileInfos.Count);
            }
            else
            {
                writer.WriteUInt16BigEndian((ushort)this._fileInfos.Count);
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
            if (this.Options.HasFlag(ReferenceTableOptions.Identifiers))
            {
                foreach (var fileInfo in this._fileInfos.Values)
                {
                    if (!fileInfo.Identifier.HasValue)
                    {
                        throw new EncodeException(
                            "FileInfo must have an identifier according to ReferenceTable options."
                        );
                    }

                    writer.WriteInt32BigEndian(fileInfo.Identifier.Value);
                }
            }

            // Write CRC checksums
            foreach (var fileInfo in this._fileInfos.Values)
            {
                if (!fileInfo.Crc.HasValue)
                {
                    throw new EncodeException("FileInfo must have a CRC checksum.");
                }

                writer.WriteInt32BigEndian(fileInfo.Crc.Value);
            }

            // Write some type of hash
            if (this.Options.HasFlag(ReferenceTableOptions.MysteryHashes))
            {
                foreach (var fileInfo in this._fileInfos.Values)
                {
                    if (!fileInfo.MysteryHash.HasValue)
                    {
                        throw new EncodeException("File must have a mystery hash according to ReferenceTable options.");
                    }

                    writer.WriteInt32BigEndian(fileInfo.MysteryHash.Value);
                }
            }

            // Write the whirlpool digests if option is set
            if (this.Options.HasFlag(ReferenceTableOptions.WhirlpoolDigests))
            {
                foreach (var fileInfo in this._fileInfos.Values)
                {
                    // Do a small check to verify the size before messing the whole file up
                    if (fileInfo.WhirlpoolDigest.Length != 64)
                    {
                        throw new EncodeException("File info's whirlpool digest is not 64 bytes in length.");
                    }

                    writer.Write(fileInfo.WhirlpoolDigest);
                }
            }

            // Write the compressed and uncompressed sizes if option is specified
            if (this.Options.HasFlag(ReferenceTableOptions.Sizes))
            {
                foreach (var fileInfo in this._fileInfos.Values)
                {
                    if (!fileInfo.CompressedSize.HasValue || !fileInfo.UncompressedSize.HasValue)
                    {
                        throw new EncodeException("FileInfo must have sizes according to ReferenceTable options.");
                    }

                    writer.WriteInt32BigEndian(fileInfo.CompressedSize.Value);
                    writer.WriteInt32BigEndian(fileInfo.UncompressedSize.Value);
                }
            }

            // Write the version numbers
            foreach (var fileInfo in this._fileInfos.Values)
            {
                if (!fileInfo.Version.HasValue)
                {
                    throw new EncodeException("FileInfo must have a version.");
                }

                writer.WriteInt32BigEndian(fileInfo.Version.Value);
            }

            // Write the entry counts
            foreach (var fileInfo in this._fileInfos.Values)
            {
                if (this.Format >= 7)
                {
                    writer.WriteAwkwardInt(fileInfo.Entries.Count);
                }
                else
                {
                    writer.WriteUInt16BigEndian((ushort)fileInfo.Entries.Count);
                }
            }

            // Write the delta encoded entry ids
            foreach (var fileInfo in this._fileInfos.Values)
            {
                var previousEntryId = 0;

                foreach (var entryId in fileInfo.Entries.Keys)
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
            if (this.Options.HasFlag(ReferenceTableOptions.Identifiers))
            {
                foreach (var fileInfo in this._fileInfos.Values)
                {
                    foreach (var entryInfo in fileInfo.Entries.Values)
                    {
                        if (!entryInfo.Identifier.HasValue)
                        {
                            throw new EncodeException(
                                "EntryInfo must have an identifier according to ReferenceTable options."
                            );
                        }

                        writer.WriteInt32BigEndian(entryInfo.Identifier.Value);
                    }
                }
            }

            return memoryStream.ToArray();
        }
    }
}
