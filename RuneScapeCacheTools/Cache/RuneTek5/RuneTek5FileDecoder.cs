using System;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Checksums;
using Noemax.BZip2;
using Noemax.GZip;
using Noemax.Lzma;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Villermen.RuneScapeCacheTools.Cache.CacheFile;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    public class RuneTek5FileDecoder
    {
        /// <summary>
        /// Decodes the given <see cref="data"/> to a <see cref="RuneTek5CacheFile"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="info">
        ///     The specification of this file according to the reference table describing it. Supply this with as
        ///     much obtained information as possible, so verification is performed.
        /// </param>
        public static DataCacheFile DecodeFile(byte[] data, CacheFileInfo info)
        {
            var dataReader = new BinaryReader(new MemoryStream(data));

            info.CompressionType = (CompressionType)dataReader.ReadByte();
            var dataLength = dataReader.ReadInt32BigEndian();

            // Total length includes already read bytes and the extra bytes read because of compression
            var totalLength = (info.CompressionType == CompressionType.None ? 5 : 9) + dataLength;

            // Decrypt the data if a key is given
            if (info.EncryptionKey != null)
            {
                var xtea = new XteaEngine();
                xtea.Init(false, new KeyParameter(info.EncryptionKey));
                var decrypted = new byte[totalLength];
                xtea.ProcessBlock(dataReader.ReadBytes(totalLength), 5, decrypted, 0);

                dataReader = new BinaryReader(new MemoryStream(decrypted));
            }

            byte[] continuousData;

            // Check if we should decompress the data or not
            if (info.CompressionType == CompressionType.None)
            {
                continuousData = dataReader.ReadBytes(dataLength);
            }
            else
            {
                // Decompress the data
                var uncompressedLength = dataReader.ReadInt32BigEndian();
                var compressedBytes = dataReader.ReadBytes(dataLength);
                var uncompressedBytes = new byte[uncompressedLength];

                switch (info.CompressionType)
                {
                    case CompressionType.Bzip2:
                        // Add the bzip2 header as it is missing from the cache for whatever reason
                        var bzipCompressedBytes = new byte[compressedBytes.Length + 4];
                        bzipCompressedBytes[0] = (byte)'B';
                        bzipCompressedBytes[1] = (byte)'Z';
                        bzipCompressedBytes[2] = (byte)'h';
                        bzipCompressedBytes[3] = (byte)'1';
                        Array.Copy(compressedBytes, 0, bzipCompressedBytes, 4, compressedBytes.Length);

                        using (var bzip2Stream = new BZip2InputStream(new MemoryStream(bzipCompressedBytes)))
                        {
                            var readBzipBytes = bzip2Stream.Read(uncompressedBytes, 0, uncompressedLength);

                            if (readBzipBytes != uncompressedLength)
                            {
                                throw new DecodeException("Uncompressed container data length does not match obtained length.");
                            }
                        }
                        break;

                    case CompressionType.Gzip:
                        using (var gzipStream = new GZipStream(new MemoryStream(compressedBytes), CompressionMode.Decompress))
                        {
                            var readGzipBytes = gzipStream.Read(uncompressedBytes, 0, uncompressedLength);

                            if (readGzipBytes != uncompressedLength)
                            {
                                throw new DecodeException("Uncompressed container data length does not match obtained length.");
                            }
                        }
                        break;

                    case CompressionType.Lzma:
                        using (var lzmaStream = new LzmaInputStream(new MemoryStream(compressedBytes)))
                        {
                            var readLzmaBytes = lzmaStream.Read(uncompressedBytes, 0, uncompressedLength);

                            if (readLzmaBytes != uncompressedLength)
                            {
                                throw new DecodeException("Uncompressed container data length does not match obtained length.");
                            }
                        }
                        break;

                    default:
                        throw new DecodeException("Invalid compression type given.");
                }

                continuousData = uncompressedBytes;
            }

            // Update and verify obtained info
            // Read and verify the version of the file
            var versionRead = false;
            if (dataReader.BaseStream.Length - dataReader.BaseStream.Position >= 2)
            {
                var version = dataReader.ReadUInt16BigEndian();

                if (info.Version != -1)
                {
                    // The version is truncated to 2 bytes, so only the least significant 2 bytes are compared
                    var truncatedInfoVersion = (int)(ushort)info.Version;
                    if (version != truncatedInfoVersion)
                    {
                        throw new DecodeException($"Obtained version part ({version}) did not match expected ({truncatedInfoVersion}).");
                    }

                    info.Version = version;
                }

                versionRead = true;
            }

            // Calculate and verify CRC
            // CRC excludes the version of the file added to the end
            // There is no way to know if the CRC is zero or unset
            var crcHasher = new Crc32();
            crcHasher.Update(data, 0, data.Length - (versionRead ? 2 : 0));
            var crc = (int)crcHasher.Value;

            if (info.Crc != null && crc != info.Crc)
            {
                throw new DecodeException($"Calculated checksum (0x{crc:X}) did not match expected (0x{info.Crc:X}).");
            }

            info.Crc = crc;

            // Calculate and verify the whirlpool digest
            var whirlpoolHasher = new WhirlpoolDigest();
            whirlpoolHasher.BlockUpdate(data, 0, data.Length - 2);

            var whirlpoolDigest = new byte[whirlpoolHasher.GetDigestSize()];
            whirlpoolHasher.DoFinal(whirlpoolDigest, 0);

            if (info.WhirlpoolDigest != null && !whirlpoolDigest.SequenceEqual(info.WhirlpoolDigest))
            {
                throw new DecodeException("Calculated whirlpool digest did not match expected.");
            }

            info.WhirlpoolDigest = whirlpoolDigest;

            // Construct the result object
            return new DataCacheFile
            {
                Entries = info.Entries.Count <= 1 ? new byte[][] { continuousData } : RuneTek5FileDecoder.DecodeEntries(continuousData, info.Entries.Count),
                Info = info
            };
        }

        /// <summary>
        /// Decodes the entries contained in the given data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="amountOfEntries"></param>
        /// <returns></returns>
        public static byte[][] DecodeEntries(byte[] data, int amountOfEntries)
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
                        throw new EndOfStreamException("End of file reached while reading the archive.");
                    }

                    // Put or append the entry data to the result
                    entries[entryId] = chunkId == 0 ? entryData : entries[entryId].Concat(entryData).ToArray();
                }
            }

            return entries;
        }

        public static byte[] EncodeFile(DataCacheFile file)
        {
            var data = file.UsesEntries ? RuneTek5FileDecoder.EncodeEntries(file.Entries) : file.Data;
            return RuneTek5FileDecoder.EncodeData(data, file.Info);
        }

        private static byte[] EncodeData(byte[] data, CacheFileInfo info)
        {
            // Encrypt data
            if (info.EncryptionKey != null)
            {
                throw new NotImplementedException("RuneTek5 file encryption is not yet supported. Nag me about it if you encounter this error.");
            }

            // Compression
            var uncompressedSize = data.Length;
            switch (info.CompressionType)
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

                case CompressionType.Lzma:
                    var lzmaCompressionStream = new MemoryStream();
                    using (var lzmaStream = new LzmaOutputStream(lzmaCompressionStream, 9))
                    {
                        lzmaStream.Write(data, 0, data.Length);
                    }
                    data = lzmaCompressionStream.ToArray();
                    break;

                case CompressionType.None:
                    break;

                default:
                    throw new ArgumentException("Invalid compression type.");
            }

            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);

            writer.Write((byte)info.CompressionType);

            // Compressed/total size
            writer.WriteInt32BigEndian(data.Length);

            // Add uncompressed size when compressing
            if (info.CompressionType != CompressionType.None)
            {
                writer.WriteInt32BigEndian(uncompressedSize);
            }

            writer.Write(data);

            // Suffix with version truncated to two bytes (not part of data for whatever reason)
            if (info.Version > -1)
            {
                writer.WriteUInt16BigEndian((ushort)info.Version);
            }

            var result = memoryStream.ToArray();

            // Update file info with sizes
            info.CompressedSize = data.Length;
            info.UncompressedSize = uncompressedSize;

            // Update file info with CRC
            var crc = new Crc32();
            crc.Update(result, 0, result.Length - 2);
            info.Crc = (int)crc.Value;

            // Update file info with whirlpool digest
            var whirlpool = new WhirlpoolDigest();
            whirlpool.BlockUpdate(result, 0, result.Length - 2);

            info.WhirlpoolDigest = new byte[whirlpool.GetDigestSize()];
            whirlpool.DoFinal(info.WhirlpoolDigest, 0);

            return result;
        }

        public static byte[] EncodeEntries(byte[][] entries)
        {
            // See format visualization in DecodeEntries method

            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);

            // Write the entries' data
            foreach (var entry in entries)
            {
                writer.Write(entry);
            }

            // TODO: Split entries into multiple chunks (when?)
            byte amountOfChunks = 1;

            for (var chunkId = 0; chunkId < amountOfChunks; chunkId++)
            {
                // Write delta encoded entry sizes

                var previousEntrySize = 0;
                foreach (var entry in entries)
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