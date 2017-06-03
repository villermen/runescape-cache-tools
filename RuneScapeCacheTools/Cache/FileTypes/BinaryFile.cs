using System;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.GZip;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Exceptions;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.FileTypes
{
    /// <summary>
    /// A cache file that just represents the raw bytes of a file.
    /// </summary>
    public sealed class BinaryFile : CacheFile
    {
        public byte[] Data { get; set; }

        public override void Decode(byte[] data)
        {
            if (this.Info == null)
            {
                throw new DecodeException("File info must be set before decoding binary file.");
            }

            var dataReader = new BinaryReader(new MemoryStream(data));

            this.Info.CompressionType = (CompressionType)dataReader.ReadByte();
            var dataLength = dataReader.ReadInt32BigEndian();

            // Total length includes already read bytes and the extra bytes read because of compression
            var totalLength = (this.Info.CompressionType == CompressionType.None ? 5 : 9) + dataLength;

            // Decrypt the data if a key is given
            if (this.Info.EncryptionKey != null)
            {
                var xtea = new XteaEngine();
                xtea.Init(false, new KeyParameter(this.Info.EncryptionKey));
                var decrypted = new byte[totalLength];
                xtea.ProcessBlock(dataReader.ReadBytes(totalLength), 5, decrypted, 0);

                dataReader = new BinaryReader(new MemoryStream(decrypted));
            }

            // Check if we should decompress the data or not
            if (this.Info.CompressionType == CompressionType.None)
            {
                this.Info.UncompressedSize = dataLength;
                this.Data = dataReader.ReadBytes(dataLength);
            }
            else
            {
                // Decompress the data
                this.Info.CompressedSize = dataLength;
                this.Info.UncompressedSize = dataReader.ReadInt32BigEndian();
                var compressedBytes = dataReader.ReadBytes(dataLength);
                var uncompressedBytes = new byte[this.Info.UncompressedSize.Value];

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

                        using (var bzip2Stream = new BZip2InputStream(new MemoryStream(bzipCompressedBytes)))
                        {
                            var readBzipBytes = bzip2Stream.Read(uncompressedBytes, 0, this.Info.UncompressedSize.Value);

                            if (readBzipBytes != this.Info.UncompressedSize)
                            {
                                throw new DecodeException("Uncompressed container data length does not match obtained length.");
                            }
                        }
                        break;

                    case CompressionType.Gzip:
                        using (var gzipStream = new GZipInputStream(new MemoryStream(compressedBytes)))
                        {
                            var readGzipBytes = gzipStream.Read(uncompressedBytes, 0, this.Info.UncompressedSize.Value);

                            if (readGzipBytes != this.Info.UncompressedSize)
                            {
                                throw new DecodeException("Uncompressed container data length does not match obtained length.");
                            }
                        }
                        break;

                    case CompressionType.Lzma:
                        using (var compressedStream = new MemoryStream(compressedBytes))
                        using (var uncompressedStream = new MemoryStream(uncompressedBytes))
                        {
                            var lzmaDecoder = new SevenZip.Compression.LZMA.Decoder();
                            lzmaDecoder.Code(compressedStream, uncompressedStream, compressedStream.Length, -1, null);

                            if (uncompressedStream.Length != this.Info.UncompressedSize)
                            {
                                throw new DecodeException("Uncompressed container data length does not match obtained length.");
                            }

                            uncompressedBytes = uncompressedStream.ToArray();
                        }
                        break;

                    default:
                        throw new DecodeException("Invalid compression type given.");
                }

                this.Data = uncompressedBytes;
            }

            // Update and verify obtained info
            // Read and verify the version of the file
            var versionRead = false;
            if (dataReader.BaseStream.Length - dataReader.BaseStream.Position >= 2)
            {
                var version = dataReader.ReadUInt16BigEndian();

                if (this.Info.Version != null)
                {
                    // The version is truncated to 2 bytes, so only the least significant 2 bytes are compared
                    var truncatedInfoVersion = (int)(ushort)this.Info.Version;
                    if (version != truncatedInfoVersion)
                    {
                        throw new DecodeException($"Obtained version part ({version}) did not match expected ({truncatedInfoVersion}).");
                    }
                }
                else
                {
                    // Set obtained version if previously unset
                    this.Info.Version = version;
                }

                versionRead = true;
            }

            // Calculate and verify CRC
            // CRC excludes the version of the file added to the end
            // There is no way to know if the CRC is zero or unset
            var crcHasher = new Crc32();
            crcHasher.Update(data, 0, data.Length - (versionRead ? 2 : 0));
            var crc = (int)crcHasher.Value;

            if (this.Info.Crc != null && crc != this.Info.Crc)
            {
                throw new DecodeException($"Calculated checksum (0x{crc:X}) did not match expected (0x{this.Info.Crc:X}).");
            }

            this.Info.Crc = crc;

            // Calculate and verify the whirlpool digest
            var whirlpoolHasher = new WhirlpoolDigest();
            whirlpoolHasher.BlockUpdate(data, 0, data.Length - (versionRead ? 2 : 0));

            var whirlpoolDigest = new byte[whirlpoolHasher.GetDigestSize()];
            whirlpoolHasher.DoFinal(whirlpoolDigest, 0);

            if (this.Info.WhirlpoolDigest != null && !whirlpoolDigest.SequenceEqual(this.Info.WhirlpoolDigest))
            {
                throw new DecodeException("Calculated whirlpool digest did not match expected.");
            }

            this.Info.WhirlpoolDigest = whirlpoolDigest;

            if (dataReader.BaseStream.Position < dataReader.BaseStream.Length)
            {
                throw new DecodeException($"Input data not fully consumed while decoding binary file. {dataReader.BaseStream.Length - dataReader.BaseStream.Position} bytes remain.");
            }
        }

        public override byte[] Encode()
        {
            if (this.Info == null)
            {
                throw new DecodeException("File info must be set before encoding binary file.");
            }

            // Encrypt data
            if (this.Info.EncryptionKey != null)
            {
                throw new NotImplementedException("RuneTek5 file encryption is not yet supported. Nag me about it if you encounter this error.");
            }

            // Compression
            var uncompressedSize = this.Data.Length;
            byte[] compressedData;
            switch (this.Info.CompressionType)
            {
                case CompressionType.Bzip2:
                    using (var bzip2CompressionStream = new MemoryStream())
                    {
                        using (var bzip2Stream = new BZip2OutputStream(bzip2CompressionStream, 1))
                        {
                            bzip2Stream.Write(this.Data, 0, this.Data.Length);
                        }

                        // Remove BZh1
                        compressedData = bzip2CompressionStream.ToArray().Skip(4).ToArray();
                    }
                    break;

                case CompressionType.Gzip:
                    using (var gzipCompressionStream = new MemoryStream())
                    {
                        using (var gzipStream = new GZipOutputStream(gzipCompressionStream))
                        {
                            gzipStream.Write(this.Data, 0, this.Data.Length);
                        }

                        compressedData = gzipCompressionStream.ToArray();
                    }
                    break;

                case CompressionType.Lzma:
                    using (var lzmaCompressionStream = new MemoryStream())
                    using (var dataStream = new MemoryStream(this.Data))
                    {
                        var lzmaEncoder = new SevenZip.Compression.LZMA.Encoder();
                        lzmaEncoder.Code(dataStream, lzmaCompressionStream, this.Data.Length, -1, null);

                       compressedData = lzmaCompressionStream.ToArray();
                    }
                    break;

                case CompressionType.None:
                    compressedData = this.Data;
                    break;

                default:
                    throw new ArgumentException("Invalid compression type.");
            }

            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);

            writer.Write((byte)this.Info.CompressionType);

            // Compressed/total size
            writer.WriteInt32BigEndian(compressedData.Length);

            // Add uncompressed size when compressing
            if (this.Info.CompressionType != CompressionType.None)
            {
                writer.WriteInt32BigEndian(uncompressedSize);
            }

            writer.Write(compressedData);

            // Suffix with version truncated to two bytes (not part of data for whatever reason)
            if (this.Info.Version != null)
            {
                writer.WriteUInt16BigEndian((ushort)this.Info.Version);
            }

            var result = memoryStream.ToArray();

            // Update file info with sizes
            this.Info.CompressedSize = compressedData.Length;
            this.Info.UncompressedSize = uncompressedSize;

            // Update file info with CRC
            var crc = new Crc32();
            crc.Update(result, 0, result.Length - 2);
            this.Info.Crc = (int)crc.Value;

            // Update file info with whirlpool digest
            var whirlpool = new WhirlpoolDigest();
            whirlpool.BlockUpdate(result, 0, result.Length - 2);

            this.Info.WhirlpoolDigest = new byte[whirlpool.GetDigestSize()];
            whirlpool.DoFinal(this.Info.WhirlpoolDigest, 0);

            return result;
        }

        public string GuessExtension()
        {
            return ExtensionGuesser.GuessExtension(this.Data);
        }
    }
}
