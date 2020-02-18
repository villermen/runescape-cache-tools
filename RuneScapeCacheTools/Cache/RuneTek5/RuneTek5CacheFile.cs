using System;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.GZip;
using Org.BouncyCastle.Crypto.Digests;
using SevenZip.Compression.LZMA;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Extension;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    public class RuneTek5CacheFile : CacheFile
    {
        /// <exception cref="DecodeException"></exception>
        public static RuneTek5CacheFile Decode(byte[] encodedData, RuneTek5CacheFileInfo info)
        {
            var dataReader = new BinaryReader(new MemoryStream(encodedData));

            var compressionType = (CompressionType)dataReader.ReadByte();
            if (!Enum.IsDefined(typeof(CompressionType), compressionType))
            {
                throw new DecodeException($"Unknown compression type {compressionType}.");
            }

            // Decrypt the data if a key is given
            if (info.EncryptionKey != null)
            {
                throw new DecodeException(
                    "XTEA encryption not supported. If you encounter this please inform me about the index and file that triggered this message."
                );

                // var totalLength = dataReader.BaseStream.Position + dataLength;
                //
                // var xtea = new XteaEngine();
                // xtea.Init(false, new KeyParameter(info.EncryptionKey));
                // var decrypted = new byte[totalLength];
                // xtea.ProcessBlock(dataReader.ReadBytes(totalLength), 5, decrypted, 0);
                //
                // dataReader = new BinaryReader(new MemoryStream(decrypted));
            }

            var dataLength = dataReader.ReadInt32BigEndian();
            var data = RuneTek5CacheFile.DecompressData(compressionType, dataReader.ReadBytes(dataLength));

            // TODO: Check if uncompressed size is defined when compression type is none.
            if (data.Length != info.UncompressedSize)
            {
                throw new DecodeException(
                    $"Uncompressed size ({data.Length}) does not match expected ({info.UncompressedSize})."
                );
            }

            // Note that version is excluded from compressed size.
            var compressedSize = (int)dataReader.BaseStream.Position;
            if (compressedSize != info.CompressedSize)
            {
                throw new DecodeException(
                    $"Compressed size ({compressedSize}) does not equal expected ({info.CompressedSize})."
                );
            }

            // Read and verify the version of the file.
            // TODO: Is version optional?: && dataReader.BaseStream.Length - dataReader.BaseStream.Position >= 2
            if (info.Version != null)
            {
                var version = dataReader.ReadUInt16BigEndian();
                // The version is truncated to 2 bytes, so only the least significant 2 bytes are compared.
                var truncatedInfoVersion = (int)(ushort)info.Version;
                if (version != truncatedInfoVersion)
                {
                    throw new DecodeException($"Obtained version {version} did not match expected {truncatedInfoVersion}.");
                }
            }

            // Calculate and verify CRC.
            if (info.Crc != null)
            {
                var crcHasher = new Crc32();
                // CRC excludes the appended version.
                crcHasher.Update(encodedData, 0, compressedSize);
                // Note that there is no way to distinguish between an unset CRC and one that is zero.
                var crc = (int)crcHasher.Value;

                if (crc != info.Crc)
                {
                    throw new DecodeException($"Calculated checksum (0x{crc:X}) did not match expected (0x{info.Crc:X}).");
                }
            }

            // Calculate and verify whirlpool digest.
            if (info.WhirlpoolDigest != null)
            {
                var whirlpoolHasher = new WhirlpoolDigest();
                whirlpoolHasher.BlockUpdate(encodedData, 0, compressedSize);

                var whirlpoolDigest = new byte[whirlpoolHasher.GetDigestSize()];
                whirlpoolHasher.DoFinal(whirlpoolDigest, 0);

                if (!whirlpoolDigest.SequenceEqual(info.WhirlpoolDigest))
                {
                    throw new DecodeException("Calculated whirlpool digest did not match expected.");
                }
            }

            if (dataReader.BaseStream.Position < dataReader.BaseStream.Length)
            {
                throw new DecodeException($"Input data not fully consumed while decoding binary file. {dataReader.BaseStream.Length - dataReader.BaseStream.Position} bytes remain.");
            }

            return new RuneTek5CacheFile(data, info);
        }

        private static byte[] DecompressData(CompressionType compressionType, byte[] compressedData)
        {
            // No compression used, return the input
            if (compressionType == CompressionType.None)
            {
                return compressedData;
            }

            using (var dataReader = new BinaryReader(new MemoryStream(compressedData)))
            {
                var uncompressedSize = dataReader.ReadInt32BigEndian();

                if (compressionType == CompressionType.Bzip2)
                {
                    // Add the required bzip2 magic number as it is missing from the cache for whatever reason.
                    using (var bzip2FixedStream = new MemoryStream((int)(4 + dataReader.BaseStream.Length - dataReader.BaseStream.Position)))
                    {
                        bzip2FixedStream.WriteByte((byte)'B');
                        bzip2FixedStream.WriteByte((byte)'Z');
                        bzip2FixedStream.WriteByte((byte)'h');
                        bzip2FixedStream.WriteByte((byte)'1');
                        dataReader.BaseStream.CopyTo(bzip2FixedStream);
                        bzip2FixedStream.Position = 0;

                        using (var bzip2InputStream = new BZip2InputStream(bzip2FixedStream))
                        {
                            // Decompress the data and resize the resulting array to the bytes actually read.
                            var result = new byte[uncompressedSize];
                            var readBytes = bzip2InputStream.Read(result, 0, uncompressedSize);
                            Array.Resize(ref result, readBytes);
                            return result;
                        }
                    }
                }

                if (compressionType == CompressionType.Gzip)
                {
                    using (var gzipStream = new GZipInputStream(dataReader.BaseStream))
                    {
                        // Decompress the data and resize the resulting array to the bytes actually read.
                        var result = new byte[uncompressedSize];
                        var readBytes = gzipStream.Read(result, 0, uncompressedSize);
                        Array.Resize(ref result, readBytes);
                        return result;
                    }
                }

                if (compressionType == CompressionType.Lzma)
                {
                    using (var outputStream = new MemoryStream(uncompressedSize))
                    {
                        var lzmaDecoder = new Decoder();
                        lzmaDecoder.Code(
                            dataReader.BaseStream,
                            outputStream,
                            dataReader.BaseStream.Length - dataReader.BaseStream.Position,
                            -1,
                            null
                        );

                        return outputStream.ToArray();
                    }
                }

                throw new DecodeException($"Unknown compression type {compressionType}.");
            }
        }

        private static byte[] CompressData(CompressionType compressionType, byte[] data)
        {
            if (compressionType == CompressionType.None)
            {
                return data;
            }

            if (compressionType == CompressionType.Bzip2) {
                using (var memoryStream = new MemoryStream())
                using (var bzip2Stream = new BZip2OutputStream(memoryStream, 1))
                {
                    bzip2Stream.Write(data, 0, data.Length);

                    // Skip BZh1
                    memoryStream.Position = 4;
                    var result = new byte[memoryStream.Length - 4];
                    memoryStream.Read(result, 0, result.Length);
                    return result;
                }
            }

            if (compressionType == CompressionType.Gzip)
            {
                using (var memoryStream = new MemoryStream())
                using (var gzipStream = new GZipOutputStream(memoryStream))
                {
                    gzipStream.Write(data, 0, data.Length);
                    return memoryStream.ToArray();
                }
            }

            if (compressionType == CompressionType.Lzma)
            {
                using (var outputStream = new MemoryStream())
                using (var dataStream = new MemoryStream(data))
                {
                    var lzmaEncoder = new Encoder();
                    lzmaEncoder.Code(dataStream, outputStream, data.Length, -1, null);
                    return outputStream.ToArray();
                }
            }

            throw new DecodeException($"Unknown compression type {compressionType}.");
        }

        public RuneTek5CacheFileInfo Info { get; set; }

        public RuneTek5CacheFile(byte[] data, RuneTek5CacheFileInfo info) : base(data)
        {
            this.Info = info;
        }

        /// <summary>
        /// Updates <see cref="Info" /> with details of encoded data.
        /// </summary>
        /// <exception cref="DecodeException"></exception>
        public byte[] Encode()
        {
            if (this.Info == null)
            {
                throw new DecodeException("File info must be set before encoding.");
            }

            // Encrypt data
            if (this.Info.EncryptionKey != null)
            {
                throw new DecodeException(
                    "XTEA encryption not supported. If you encounter this please inform me about the index and file that triggered this message."
                );
            }

            // Compression
            var uncompressedSize = this.Data.Length;
            var compressedData = RuneTek5CacheFile.CompressData(this.Info.CompressionType, this.Data);

            using (var memoryStream = new MemoryStream())
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write((byte)this.Info.CompressionType);
                writer.WriteInt32BigEndian(compressedData.Length);

                // Add uncompressed size if compression is used
                if (this.Info.CompressionType != CompressionType.None)
                {
                    writer.WriteInt32BigEndian(uncompressedSize);
                }

                writer.Write(compressedData);

                // Version is not included in compressed size.
                var compressedSize = (int)writer.BaseStream.Position;

                // Suffix with version truncated to two bytes.
                if (this.Info.Version != null)
                {
                    writer.WriteUInt16BigEndian((ushort)this.Info.Version);
                }

                var result = memoryStream.ToArray();

                // Update file info with sizes.
                this.Info.CompressedSize = compressedSize;
                this.Info.UncompressedSize = uncompressedSize;

                // Update file info with CRC.
                var crc = new Crc32();
                crc.Update(result, 0, compressedSize);
                this.Info.Crc = (int)crc.Value;

                // Update file info with whirlpool digest.
                var whirlpool = new WhirlpoolDigest();
                whirlpool.BlockUpdate(result, 0, compressedSize);

                this.Info.WhirlpoolDigest = new byte[whirlpool.GetDigestSize()];
                whirlpool.DoFinal(this.Info.WhirlpoolDigest, 0);

                return result;
            }
        }
    }
}
