using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Checksums;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    using System.Text;

    /// <summary>
    /// A <see cref="CacheFile"/> that allows for conversion to and from binary file data in the RuneTek5 cache format.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Villermen</author>
    public class RuneTek5CacheFile : CacheFile
    {
        public RuneTek5CacheFile() { }

        public RuneTek5CacheFile(byte[] data, CacheFileInfo info) : base(data, info) { }

        public RuneTek5CacheFile(byte[][] entries, CacheFileInfo info) : base(entries, info) { }

        /// <summary>
        /// Decodes the given <see cref="data"/> to a <see cref="RuneTek5CacheFile"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="info">
        ///     The specification of this file according to the reference table describing it. Supply this with as
        ///     much obtained information as possible, so verification is performed.
        /// </param>
        /// <param name="key"></param>
        public static RuneTek5CacheFile Decode(byte[] data, CacheFileInfo info)
        {
            var file = new RuneTek5CacheFile
            {
                Info = info
            };

            var dataReader = new BinaryReader(new MemoryStream(data));

            file.Info.CompressionType = (CompressionType)dataReader.ReadByte();
            var dataLength = dataReader.ReadInt32BigEndian();

            // Total length includes already read bytes and the extra bytes read because of compression
            var totalLength = (file.Info.CompressionType == CompressionType.None ? 5 : 9) + dataLength;

            // Decrypt the data if a key is given
            if (file.Info.EncryptionKey != null)
            {
                var xtea = new XteaEngine();
                xtea.Init(false, new KeyParameter(file.Info.EncryptionKey));
                var decrypted = new byte[totalLength];
                xtea.ProcessBlock(dataReader.ReadBytes(totalLength), 5, decrypted, 0);

                dataReader = new BinaryReader(new MemoryStream(decrypted));
            }

            byte[] continuousData;

            // Check if we should decompress the data or not
            if (file.Info.CompressionType == CompressionType.None)
            {
                continuousData = dataReader.ReadBytes(dataLength);
            }
            else
            {
                // Decompress the data
                var uncompressedLength = dataReader.ReadInt32BigEndian();
                var compressedBytes = dataReader.ReadBytes(dataLength);
                var uncompressedBytes = new byte[uncompressedLength];

                switch (file.Info.CompressionType)
                {
                    case CompressionType.Bzip2:
                        // Add the bzip2 header as it is missing from the cache for whatever reason
                        var bzipCompressedBytes = new byte[compressedBytes.Length + 4];
                        bzipCompressedBytes[0] = (byte)'B';
                        bzipCompressedBytes[1] = (byte)'Z';
                        bzipCompressedBytes[2] = (byte)'h';
                        bzipCompressedBytes[3] = (byte)'1';
                        Array.Copy(compressedBytes, 0, bzipCompressedBytes, 4, compressedBytes.Length);
                        var bzip2Stream = new BZip2InputStream(new MemoryStream(bzipCompressedBytes));
                        var readBzipBytes = bzip2Stream.Read(uncompressedBytes, 0, uncompressedLength);

                        if (readBzipBytes != uncompressedLength)
                        {
                            throw new CacheException(
                                "Uncompressed container data length does not match obtained length.");
                        }
                        break;

                    case CompressionType.Gzip:
                        var gzipStream = new GZipStream(new MemoryStream(compressedBytes), CompressionMode.Decompress);
                        var readGzipBytes = gzipStream.Read(uncompressedBytes, 0, uncompressedLength);

                        if (readGzipBytes != uncompressedLength)
                        {
                            throw new CacheException(
                                "Uncompressed container data length does not match obtained length.");
                        }
                        break;

                    case CompressionType.LZMA:
                        throw new NotImplementedException("Decoding using LZMA decompression is not yet supported. Nag me about it if you encounter this error.");
                        break;

                    default:
                        throw new CacheException("Invalid compression type given.");
                }

                continuousData = uncompressedBytes;
            }

            file.Entries = file.Info.Entries.Count > 1 ? RuneTek5CacheFile.DecodeEntries(continuousData, file.Info.Entries.Count) : new[] { continuousData };

            // Verify supplied info where possible
            // Read and verify the version of the file
            if (file.Info.Version > -1)
            {
                var obtainedVersion = dataReader.ReadUInt16BigEndian();

                // The version is truncated to 2 bytes, so only the least significant 2 bytes are compared
                var truncatedInfoVersion = (int)(ushort)file.Info.Version;
                if (obtainedVersion != truncatedInfoVersion)
                {
                    throw new CacheException($"Obtained version part ({obtainedVersion}) did not match expected ({truncatedInfoVersion}).");
                }

                // Calculate and verify crc
                // CRC excludes the version of the file added to the end

                // There is no way to know if the CRC is zero or unset, so I've put it with the version check (whether info was given)
                var crc = new Crc32();
                crc.Update(data, 0, data.Length - 2);

                var calculatedCRC = (int)crc.Value;

                if (calculatedCRC != file.Info.CRC)
                {
                    throw new CacheException($"Calculated checksum (0x{calculatedCRC:X}) did not match expected (0x{file.Info.CRC:X}).");
                }
            }

            // Calculate and verify the whirlpool digest if set in the info
            if (file.Info.WhirlpoolDigest != null)
            {
                var whirlpool = new WhirlpoolDigest();
                whirlpool.BlockUpdate(data, 0, data.Length - 2);

                var calculatedWhirlpool = new byte[whirlpool.GetDigestSize()];
                whirlpool.DoFinal(calculatedWhirlpool, 0);

                if (!calculatedWhirlpool.SequenceEqual(file.Info.WhirlpoolDigest))
                {
                    throw new CacheException("Calculated whirlpool digest did not match expected.");
                }
            }

            return file;
        }

        /// <summary>
        /// Converts this <see cref="RuneTek5CacheFile"/> into a byte array to be written to the cache.
        /// Updates properties in <see cref="CacheFile.Info"/> to match the possibly changed data.
        /// </summary>
        /// <returns></returns>
        public byte[] Encode()
        {
            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);

            writer.Write((byte)this.Info.CompressionType);

            // Encrypt data
            if (this.Info.EncryptionKey != null)
            {
                throw new NotImplementedException("RuneTek5 file encryption is not yet supported. Nag me about it if you encounter this error.");
            }

            // Encode data (file or entries)
            var data = this.Info.Entries.Count > 1 ? this.EncodeEntries() : this.Data;

            // Compression
            var uncompressedSize = data.Length;
            switch (this.Info.CompressionType)
            {
                case CompressionType.Bzip2:
                    var bzip2CompressionStream = new MemoryStream();
                    using (var bzip2Stream = new BZip2OutputStream(bzip2CompressionStream, 1))
                    {
                        bzip2Stream.Write(data, 0, data.Length);
                    }

                    // Remove BZh1
                    data = bzip2CompressionStream.ToArray().Skip(4).ToArray();
                    break;

                case CompressionType.Gzip:
                    var gzipCompressionStream = new MemoryStream();
                    using (var gzipStream = new GZipStream(gzipCompressionStream, CompressionMode.Compress))
                    {
                        gzipStream.Write(data, 0, data.Length);
                    }
                    data = gzipCompressionStream.ToArray();
                    break;

                case CompressionType.LZMA:
                    throw new NotImplementedException("Encoding using LZMA compression is not yet supported. Nag me about it if you encounter this error.");
                    break;

                case CompressionType.None:
                    break;

                default:
                    throw new CacheException("Invalid compression type given.");
            }

            // Compressed/total size
            writer.WriteInt32BigEndian(data.Length);

            // Add uncompressed size when compressing
            if (this.Info.CompressionType != CompressionType.None)
            {
                writer.WriteInt32BigEndian(uncompressedSize);
            }

            writer.Write(data);

            // Suffix with version truncated to two bytes (not part of data for whatever reason)
            if (this.Info.Version > -1)
            {
                writer.WriteUInt16BigEndian((ushort)this.Info.Version);
            }

            var result = memoryStream.ToArray();

            // Update file info with sizes
            this.Info.CompressedSize = data.Length;
            this.Info.UncompressedSize = uncompressedSize;

            // Update file info with CRC
            var crc = new Crc32();
            crc.Update(result, 0, result.Length - 2);
            this.Info.CRC = (int)crc.Value;

            // Update file info with whirlpool digest
            var whirlpool = new WhirlpoolDigest();
            whirlpool.BlockUpdate(result, 0, result.Length - 2);

            this.Info.WhirlpoolDigest = new byte[whirlpool.GetDigestSize()];
            whirlpool.DoFinal(this.Info.WhirlpoolDigest, 0);

            return result;
        }

        /// <summary>
        /// Decodes the entries contained in the given data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="amountOfEntries"></param>
        /// <returns></returns>
        private static byte[][] DecodeEntries(byte[] data, int amountOfEntries)
        {
            /* 
             * Format visualization:                                 
             * chunk1 data:                      [entry1chunk1][entry2chunk1]
             * chunk2 data:                      [entry1chunk2][entry2chunk2]
             * delta-encoded chunk1 entry sizes: [entry1chunk1size][entry2chunk1size]
             * delta-encoded chunk2 entry sizes: [entry1chunk2size][entry2chunk2size]
             *                                   [chunkamount (2)]
             * 
             * Add entry1chunk2 to entry1chunk1 and voilà, unnecessarily complex bullshit solved.
             */

            var entries = new byte[amountOfEntries][];

            var reader = new BinaryReader(new MemoryStream(data));

            reader.BaseStream.Position = reader.BaseStream.Length - 1;
            var amountOfChunks = reader.ReadByte();

            // Read the sizes of the child entries and individual chunks
            var chunkEntrySizes = new int[amountOfChunks, amountOfEntries];

            reader.BaseStream.Position = reader.BaseStream.Length - 1 - amountOfChunks * amountOfEntries * 4;

            for (var chunkId = 0; chunkId < amountOfChunks; chunkId++)
            {
                var chunkSize = 0;
                for (var entryId = 0; entryId < amountOfEntries; entryId++)
                {
                    // Read the delta encoded chunk length
                    var delta = reader.ReadInt32BigEndian();
                    chunkSize += delta;

                    // Store the size of this entry in this chunk
                    chunkEntrySizes[chunkId, entryId] = chunkSize;
                }
            }

            // Read the data
            reader.BaseStream.Position = 0;
            for (var chunkId = 0; chunkId < amountOfChunks; chunkId++)
            {
                for (var entryId = 0; entryId < amountOfEntries; entryId++)
                {
                    // Read the bytes of the entry into the archive entries
                    var entrySize = chunkEntrySizes[chunkId, entryId];
                    var entryData = reader.ReadBytes(entrySize);

                    if (entryData.Length != entrySize)
                    {
                        throw new CacheException("End of file reached while reading the archive.");
                    }

                    // Put or append the entry data to the result
                    entries[entryId] = chunkId == 0 ? entryData : entries[entryId].Concat(entryData).ToArray();
                }
            }

            return entries;
        }

        private byte[] EncodeEntries()
        {
            // See format visualization in DecodeEntries method

            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);

            // Write the entries' data
            foreach (var entry in this.Entries)
            {
                writer.Write(entry);
            }

            // TODO: Split entries into multiple chunks (when?)
            byte amountOfChunks = 1;

            for (var chunkId = 0; chunkId < amountOfChunks; chunkId++)
            {
                // Write delta encoded entry sizes

                var previousEntrySize = 0;
                foreach (var entry in this.Entries)
                {
                    var entrySize = entry.Length;

                    var delta = entrySize - previousEntrySize;

                    writer.WriteInt32BigEndian(delta);

                    previousEntrySize = entrySize;
                }
            }

            // Finish of with the amount of chunks
            writer.Write(amountOfChunks);

            return memoryStream.ToArray();
        }
    }
}