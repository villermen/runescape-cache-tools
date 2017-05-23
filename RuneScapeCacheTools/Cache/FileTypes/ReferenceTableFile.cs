using System.Collections.Generic;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Exceptions;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.FileTypes
{
    /// <summary>
    ///     A <see cref="ReferenceTableFile" /> holds metadata for all registered files in an index, such as checksums, versions
    ///     and archive members.
    ///     Note that the data of registered files does not have to be present in the index for them to be listed.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Sean</author>
    /// <author>Villermen</author>
    public class ReferenceTableFile : CacheFile
    {
        /// <summary>
        ///     The entries in this table.
        /// </summary>
        private readonly SortedDictionary<int, CacheFileInfo> _files = new SortedDictionary<int, CacheFileInfo>();

        /// <summary>
        /// Gets the ids of the files listed in this <see cref="ReferenceTableFile"/>.
        /// </summary>
        /// <returns></returns>
        public int[] FileIds => this._files.Keys.ToArray();

        /// <summary>
        ///     The format of this table.
        /// </summary>
        public byte Format { get; set; } = 7;

        /// <summary>
        ///     The flags of this table.
        /// </summary>
        public CacheFileOptions Options { get; set; }

        /// <summary>
        ///     The version of this table.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets the <see cref="CacheFileInfo"/> for the given file in the index described by this <see cref="ReferenceTableFile"/>.
        /// </summary>
        /// <returns></returns>
        public CacheFileInfo GetFileInfo(int fileId)
        {
            if (!this._files.ContainsKey(fileId))
            {
                throw new FileNotFoundException($"File {fileId} does not exist in this reference table.");
            }

            return this._files[fileId].Clone();
        }

        public void SetFileInfo(int fileId, CacheFileInfo info)
        {
            if (this._files.ContainsKey(fileId))
            {
                this._files[fileId] = info;
            }
            else
            {
                this._files.Add(fileId, info);
            }
        }

        public override void Decode(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));

            this.Format = reader.ReadByte();

            // Read header
            if (this.Format < 5 || this.Format > 7)
            {
                throw new DecodeException($"Incorrect reference table format version {this.Format}.");
            }

            if (this.Format >= 6)
            {
                this.Version = reader.ReadInt32BigEndian();
            }

            this.Options = (CacheFileOptions)reader.ReadByte();

            // Read the ids of the files (delta encoded)
            var fileCount = this.Format >= 7 ? reader.ReadAwkwardInt() : reader.ReadUInt16BigEndian();

            var fileId = 0;
            for (var fileNumber = 0; fileNumber < fileCount; fileNumber++)
            {
                var delta = this.Format >= 7 ? reader.ReadAwkwardInt() : reader.ReadUInt16BigEndian();
                fileId += delta;

                this._files.Add(fileId, new CacheFileInfo
                {
                    Index = (Index)this.Info.FileId,
                    FileId = fileId
                });
            }

            // Read the identifiers if present
            if (this.Options.HasFlag(CacheFileOptions.Identifiers))
            {
                foreach (var file in this._files.Values)
                {
                    file.Identifier = reader.ReadInt32BigEndian();
                }
            }

            // Read the CRC32 checksums
            foreach (var file in this._files.Values)
            {
                file.Crc = reader.ReadInt32BigEndian();
            }

            // Read some type of hash
            if (this.Options.HasFlag(CacheFileOptions.MysteryHashes))
            {
                foreach (var file in this._files.Values)
                {
                    file.MysteryHash = reader.ReadInt32BigEndian();
                }
            }

            // Read the whirlpool digests if present
            if (this.Options.HasFlag(CacheFileOptions.WhirlpoolDigests))
            {
                foreach (var file in this._files.Values)
                {
                    file.WhirlpoolDigest = reader.ReadBytes(64);
                }
            }

            // Read the compressed and uncompressed sizes
            if (this.Options.HasFlag(CacheFileOptions.Sizes))
            {
                foreach (var file in this._files.Values)
                {
                    file.CompressedSize = reader.ReadInt32BigEndian();
                    file.UncompressedSize = reader.ReadInt32BigEndian();
                }
            }

            // Read the version numbers
            foreach (var file in this._files.Values)
            {
                file.Version = reader.ReadInt32BigEndian();
            }

            // Read the entry counts
            var entryCounts = new Dictionary<int, int>();
            foreach (var file in this._files.Values)
            {
                entryCounts.Add(file.FileId, this.Format >= 7 ? reader.ReadAwkwardInt() : reader.ReadUInt16BigEndian());
            }

            // Read the delta encoded entry ids
            foreach (var entryCountPair in entryCounts)
            {
                var entryCountFileId = entryCountPair.Key;
                var entryCount = entryCountPair.Value;

                var entryId = 0;
                for (var entryNumber = 0; entryNumber < entryCount; entryNumber++)
                {
                    var delta = this.Format >= 7 ? reader.ReadAwkwardInt() : reader.ReadUInt16BigEndian();
                    entryId += delta;

                    this._files[entryCountFileId].Entries.Add(entryId, new CacheFileEntryInfo
                    {
                        EntryId = entryId
                    });
                }
            }

            // Read the entry identifiers if present
            if (this.Options.HasFlag(CacheFileOptions.Identifiers))
            {
                foreach (var file in this._files.Values)
                {
                    foreach (var entry in file.Entries.Values)
                    {
                        entry.Identifier = reader.ReadInt32BigEndian();
                    }
                }
            }
        }

        public override byte[] Encode()
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
                writer.WriteAwkwardInt(this._files.Count);
            }
            else
            {
                writer.WriteUInt16BigEndian((ushort)this._files.Count);
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
                foreach (var file in this._files.Values)
                {
                    writer.WriteInt32BigEndian(file.Identifier);
                }
            }

            // Write CRC checksums
            foreach (var file in this._files.Values)
            {
                writer.WriteInt32BigEndian(file.Crc.Value);
            }

            // Write some type of hash
            if (this.Options.HasFlag(CacheFileOptions.MysteryHashes))
            {
                foreach (var file in this._files.Values)
                {
                    writer.WriteInt32BigEndian(file.MysteryHash);
                }
            }

            // Write the whirlpool digests if option is set
            if (this.Options.HasFlag(CacheFileOptions.WhirlpoolDigests))
            {
                foreach (var file in this._files.Values)
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
                foreach (var file in this._files.Values)
                {
                    writer.WriteInt32BigEndian(file.CompressedSize);
                    writer.WriteInt32BigEndian(file.UncompressedSize);
                }
            }

            // Write the version numbers
            foreach (var file in this._files.Values)
            {
                writer.WriteInt32BigEndian(file.Version);
            }

            // Write the entry counts
            foreach (var file in this._files.Values)
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
            foreach (var file in this._files.Values)
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
                foreach (var file in this._files.Values)
                {
                    foreach (var entry in file.Entries.Values)
                    {
                        writer.WriteInt32BigEndian(entry.Identifier);
                    }
                }
            }

            return memoryStream.ToArray();
        }
    }
}