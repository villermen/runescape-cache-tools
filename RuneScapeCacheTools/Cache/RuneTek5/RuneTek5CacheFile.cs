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
    /// <summary>
    ///     A <see cref="RuneTek5CacheFile" /> holds raw file data.
    ///     This data can be decrypted and decompressed from the cache, and can be converted back into its encrypted and
    ///     compressed form.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Villermen</author>
    public class RuneTek5CacheFile : CacheFile
    {
        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="info">
        ///     The specification of this file according to the reference table describing it. Supply this with as
        ///     much obtained information as possible, so verification is performed.
        /// </param>
        /// <param name="key"></param>
        public RuneTek5CacheFile(byte[] data, CacheFileInfo info, uint[] key = null)
        {
            if (info != null)
            {
                this.Info = info;
            }

            this.Key = key;

            var dataReader = new BinaryReader(new MemoryStream(data));

            this.Info.CompressionType = (CompressionType)dataReader.ReadByte();
            var length = dataReader.ReadInt32BigEndian();

            // Decrypt the data if a key is given
            if (this.Key != null)
            {
                var byteKeyStream = new MemoryStream(16);
                var byteKeyWriter = new BinaryWriter(byteKeyStream);
                foreach (var keyValue in this.Key)
                {
                    byteKeyWriter.WriteUInt32BigEndian(keyValue);
                }

                var totalLength = length + (this.Info.CompressionType == CompressionType.None ? 5 : 9);

                var xtea = new XteaEngine();
                xtea.Init(false, new KeyParameter(byteKeyStream.ToArray()));
                var decrypted = new byte[totalLength];
                xtea.ProcessBlock(dataReader.ReadBytes(totalLength), 5, decrypted, 0);

                dataReader = new BinaryReader(new MemoryStream(decrypted));
            }

            byte[] continuousData;

            // Check if we should decompress the data or not
            if (this.Info.CompressionType == CompressionType.None)
            {
                continuousData = dataReader.ReadBytes(length);
            }
            else
            {
                // Decompress the data
                var uncompressedLength = dataReader.ReadInt32BigEndian();
                var compressedBytes = dataReader.ReadBytes(length);
                var uncompressedBytes = new byte[uncompressedLength];

                switch (this.Info.CompressionType)
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
                        throw new NotImplementedException();
                        break;

                    default:
                        throw new CacheException("Invalid compression type given.");
                }

                continuousData = uncompressedBytes;
            }

            this.Entries = this.Info.Entries.Count > 1 ? this.DecodeEntries(continuousData, this.Info.Entries.Count) : new[] { continuousData };

            // Verify supplied info where possible

            // Read and verify the version of the file
            if (this.Info.Version > -1)
            {
                var obtainedVersion = dataReader.ReadUInt16BigEndian();

                // The version is truncated to 2 bytes, so only the least significant 2 bytes are compared
                var truncatedInfoVersion = (int)(ushort)this.Info.Version;
                if (obtainedVersion != truncatedInfoVersion)
                {
                    throw new CacheException($"Obtained version part ({obtainedVersion}) did not match expected ({truncatedInfoVersion}).");
                }

                // Calculate and verify crc
                // CRC excludes the version of the file added to the end

                // There is no way to know if the CRC is zero or unset, so I've put it with the version check
                var crc = new Crc32();
                crc.Update(data, 0, data.Length - 2);

                var calculatedCRC = (int)crc.Value;

                if (calculatedCRC != this.Info.CRC)
                {
                    throw new CacheException($"Calculated checksum (0x{calculatedCRC:X}) did not match expected (0x{this.Info.CRC:X}).");
                }
            }

            // Calculate and verify the whirlpool digest if set in the info
            if (this.Info.WhirlpoolDigest != null)
            {
                var whirlpool = new WhirlpoolDigest();
                whirlpool.BlockUpdate(data, 0, data.Length - 2);

                var calculatedWhirlpool = new byte[whirlpool.GetDigestSize()];
                whirlpool.DoFinal(calculatedWhirlpool, 0);

                if (calculatedWhirlpool != this.Info.WhirlpoolDigest)
                {
                    throw new CacheException("Calculated whirlpool digest did not match expected.");
                }
            }
        }

        /// <summary>
        ///     The key used in decrypting and encrypting the data.
        /// </summary>
        public uint[] Key { get; set; }

        private byte[][] DecodeEntries(byte[] data, int amountOfEntries)
        {
            var entries = new byte[amountOfEntries][];

            var reader = new BinaryReader(new MemoryStream(data));

            reader.BaseStream.Position = reader.BaseStream.Length - 1;
            var amountOfChunks = reader.ReadByte();

            if (amountOfChunks > 1)
            {
                throw new Exception("I don't know how to deal with more than one chunk in an entry file...");
            }

            // Read the sizes of the child entries and individual chunks
            var chunkSizes = new int[amountOfChunks, amountOfEntries];
            // var entrySizes = new int[amountOfEntries];

            reader.BaseStream.Position = reader.BaseStream.Length - 1 - amountOfChunks * amountOfEntries * 4;

            for (var chunkId = 0; chunkId < amountOfChunks; chunkId++)
            {
                var chunkSize = 0;
                for (var entryId = 0; entryId < amountOfEntries; entryId++)
                {
                    // Read the delta encoded chunk length
                    var delta = reader.ReadInt32BigEndian();
                    chunkSize += delta;

                    // Store the size of this chunk
                    chunkSizes[chunkId, entryId] = chunkSize;

                    // Add it to the size of the whole file
                    // entrySizes[entryId] += chunkSize;
                }
            }

            // Read the data
            reader.BaseStream.Position = 0;
            for (var chunkId = 0; chunkId < amountOfChunks; chunkId++)
            {
                for (var entryId = 0; entryId < amountOfEntries; entryId++)
                {
                    // Read the bytes of the entry into the archive entries
                    var entrySize = chunkSizes[chunkId, entryId];
                    var entryData = reader.ReadBytes(entrySize);

                    if (entryData.Length != entrySize)
                    {
                        throw new CacheException("End of file reached while reading the archive.");
                    }

                    entries[entryId] = entryData.ToArray();
                }
            }

            return entries;
        }

        // TODO: For decoding and encoding of entries: Figure out what happens when amount of chunks > 1
        // Is it a way to increase the amount of entries above 256?
        // Right now I'm throwing exceptions on occurrences
        private byte[] EncodeEntries()
        {
            if (this.Entries.Length > 256)
            {
                throw new Exception("I don't know how to deal with more than 256 entries in a file...");
            }

            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);

            // Write the entries' data
            foreach (var entry in this.Entries)
            {
                writer.Write(entry);
            }

            byte amountOfChunks = 1;

            for (var chunkId = 0; chunkId < amountOfChunks; chunkId++)
            {
                // Write delta encoded entry sizes

                var previousEntrySize = 0;
                foreach (var entry in this.Entries)
                {
                    var entrySize = entry.Length;

                    var delta = entrySize - previousEntrySize;

                    writer.Write(delta);

                    previousEntrySize = entrySize;
                }
            }

            // Finish of with the amount of chunks
            writer.Write(amountOfChunks);

            return memoryStream.ToArray();
        }


        public override byte[] Encode()
        {
            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);

            writer.Write((byte)this.Info.CompressionType);

            // TODO: Add support for encryption

            // Encode data (file or entries)
            var data = this.Info.Entries.Count > 1 ? this.EncodeEntries() : this.Data;

            // Compression
            var uncompressedSize = data.Length;
            switch (this.Info.CompressionType)
            {
                case CompressionType.Bzip2:
                    var compressionStream = new MemoryStream();
                    using (var bzip2Stream = new BZip2OutputStream(compressionStream))
                    {
                        bzip2Stream.Write(data, 0, data.Length);
                    }

                    // TODO: Check if BZh1 is added, and remove it if it is

                    data = compressionStream.ToArray();
                    break;

                case CompressionType.Gzip:
                    throw new NotImplementedException();
                    break;

                case CompressionType.LZMA:
                    throw new NotImplementedException();
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
                writer.WriteInt32BigEndian(data.Length);
            }

            writer.Write(data);

            // TODO: What to do with crc and whirlpool that need to be stored in reference table but can only be calculated here?

            throw new NotImplementedException();

            return memoryStream.ToArray();
        }
    }
}