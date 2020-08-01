using System;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.GZip;
using Org.BouncyCastle.Crypto.Digests;
using Serilog;
using SevenZip.Compression.LZMA;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.File
{
    /// <summary>
    /// A file in the cache containing (decoded) binary data.
    /// </summary>
    public class CacheFile
    {
        /// <summary>
        /// Decode the cache file's encrypted/compressed data. The available properties in the passed
        /// <see cref="CacheFileInfo" /> will be used to validate the retrieved data.
        /// </summary>
        /// <exception cref="DecodeException"></exception>
        public static CacheFile Decode(byte[] encodedData, CacheFileInfo info)
        {
            using var dataStream = new MemoryStream(encodedData);
            using var dataReader = new BinaryReader(dataStream);

            // Decrypt the data if a key is given
            if (info.EncryptionKey != null)
            {
                throw new DecodeException(
                    "XTEA encryption not supported. If you encounter this please inform me about the index and file that triggered this message."
                );

                // var totalLength = dataStream.Position + dataLength;
                //
                // var xtea = new XteaEngine();
                // xtea.Init(false, new KeyParameter(info.EncryptionKey));
                // var decrypted = new byte[totalLength];
                // xtea.ProcessBlock(dataReader.ReadBytes(totalLength), 5, decrypted, 0);
                //
                // dataReader = new BinaryReader(new MemoryStream(decrypted));
            }

            // Decompress data
            var compressionType = (CompressionType)dataReader.ReadByte();
            var compressedSize = dataReader.ReadInt32BigEndian();
            var uncompressedSize = compressedSize;
            if (compressionType != CompressionType.None)
            {
                uncompressedSize = dataReader.ReadInt32BigEndian();
            }

            var data = CacheFile.DecompressData(compressionType, dataReader.ReadBytesExactly(compressedSize), uncompressedSize);

            if (compressionType == CompressionType.None)
            {
                // Uncompressed size includes meta bytes for info when not using compression.
                uncompressedSize = (int)dataStream.Position;
            }

            // Compressed size includes meta bytes for info.
            compressedSize = (int)dataStream.Position;

            // Verify compressed size. Info's compressed size includes meta bytes.
            if (info.CompressedSize != null && compressedSize != info.CompressedSize)
            {
                throw new DecodeException(
                    $"Compressed size ({compressedSize}) does not equal expected ({info.CompressedSize})."
                );
            }

            // Verify uncompressed size. Info's uncompressed size includes meta bytes
            if (info.UncompressedSize != null && uncompressedSize != info.UncompressedSize)
            {
                throw new DecodeException(
                    $"Uncompressed size ({data.Length}) does not match expected ({info.UncompressedSize})."
                );
            }

            // Calculate and verify CRC.
            var crcHasher = new Crc32();
            // CRC excludes the appended version.
            crcHasher.Update(encodedData.Take(compressedSize).ToArray());
            // Note that there is no way to distinguish between an unset CRC and one that is zero.
            var crc = (int)crcHasher.Value;

            if (info.Crc != null && crc != info.Crc)
            {
                throw new DecodeException($"Calculated checksum ({crc}) did not match expected ({info.Crc}).");
            }

            // Calculate and verify whirlpool digest.
            var whirlpoolHasher = new WhirlpoolDigest();
            whirlpoolHasher.BlockUpdate(encodedData, 0, compressedSize);
            var whirlpoolDigest = new byte[whirlpoolHasher.GetDigestSize()];
            whirlpoolHasher.DoFinal(whirlpoolDigest, 0);

            if (info.WhirlpoolDigest != null && !whirlpoolDigest.SequenceEqual(info.WhirlpoolDigest) )
            {
                throw new DecodeException("Calculated whirlpool digest did not match expected.");
            }

            if (dataStream.Position < dataStream.Length)
            {
                Log.Warning($"Input data not fully consumed while decoding RuneTek5CacheFile. {dataStream.Length - dataStream.Position} bytes remain.");

                // throw new DecodeException(
                //     $"Input data not fully consumed while decoding RuneTek5CacheFile. {dataStream.Length - dataStream.Position} bytes remain."
                // );
            }

            // Update info with obtained details.
            info.CompressionType = compressionType;
            info.CompressedSize = compressedSize;
            info.UncompressedSize = uncompressedSize;
            info.Crc = crc;

            return new CacheFile(data)
            {
                Info = info,
            };
        }

        private static byte[] DecompressData(CompressionType compressionType, byte[] compressedData, int uncompressedSize)
        {
            if (compressionType == CompressionType.None)
            {
                return compressedData;
            }

            using var compressedDataStream = new MemoryStream(compressedData);

            if (compressionType == CompressionType.Bzip2)
            {
                // Add the required bzip2 magic number as it is missing from the cache for whatever reason.
                using var bzip2InputStream = new MemoryStream(4 + compressedData.Length);
                bzip2InputStream.WriteByte((byte)'B');
                bzip2InputStream.WriteByte((byte)'Z');
                bzip2InputStream.WriteByte((byte)'h');
                bzip2InputStream.WriteByte((byte)'1');
                compressedDataStream.CopyTo(bzip2InputStream);
                bzip2InputStream.Position = 0;

                var sdjfojd = bzip2InputStream.ToArray();

                using var outputStream = new MemoryStream();
                BZip2.Decompress(bzip2InputStream, outputStream, true);
                return outputStream.ToArray();
            }

            if (compressionType == CompressionType.Gzip)
            {
                using var outputStream = new MemoryStream();
                GZip.Decompress(compressedDataStream, outputStream, true);
                return outputStream.ToArray();
            }

            if (compressionType == CompressionType.Lzma)
            {
                using var compressedDataReader = new BinaryReader(compressedDataStream);
                using var outputStream = new MemoryStream(uncompressedSize);
                var lzmaDecoder = new Decoder();
                lzmaDecoder.SetDecoderProperties(compressedDataReader.ReadBytesExactly(5));
                lzmaDecoder.Code(
                    compressedDataStream,
                    outputStream,
                    compressedDataStream.Length - compressedDataStream.Position,
                    uncompressedSize,
                    null
                );

                return outputStream.ToArray();
            }

            throw new DecodeException($"Unknown compression type {compressionType}.");
        }

        private static byte[] CompressData(CompressionType compressionType, byte[] data)
        {
            if (compressionType == CompressionType.None)
            {
                return data;
            }

            if (compressionType == CompressionType.Bzip2)
            {
                using var outputStream = new MemoryStream();
                BZip2.Compress(new MemoryStream(data), outputStream, true, 9);

                var sdajofdas = outputStream.ToArray();

                // Remove BZh1.
                return outputStream.ToArray().Skip(4).ToArray();
            }

            if (compressionType == CompressionType.Gzip)
            {
                using var outputStream = new MemoryStream();
                GZip.Compress(new MemoryStream(data), outputStream, true, 512, 9);
                return outputStream.ToArray();
            }

            if (compressionType == CompressionType.Lzma)
            {
                throw new NotImplementedException("LZMA compression is currently not implemented.");
            }

            throw new EncodeException($"Unknown compression type {compressionType}.");
        }

        public CacheFileInfo Info { get; set; } = new CacheFileInfo();

        public byte[] Data { get; set; }

        public CacheFile(byte[] data)
        {
            this.Data = data;
        }

        /// <summary>
        /// Encodes the data of this file into a byte array using the format settings on <see cref="Info" />. Updates
        /// <see cref="Info" /> with details of encoded data.
        /// </summary>
        /// <exception cref="DecodeException"></exception>
        public byte[] Encode()
        {
            // Encrypt data
            if (this.Info.EncryptionKey != null)
            {
                throw new EncodeException(
                    "XTEA encryption not supported. If you encounter this please inform me about the index and file that triggered this message."
                );
            }

            // Compression
            var uncompressedSize = this.Data.Length;
            var compressedData = CacheFile.CompressData(this.Info.CompressionType, this.Data);

            using var dataStream = new MemoryStream();
            using var dataWriter = new BinaryWriter(dataStream);

            dataWriter.Write((byte)this.Info.CompressionType);
            dataWriter.WriteInt32BigEndian(compressedData.Length);

            // Add uncompressed size if compression is used.
            if (this.Info.CompressionType != CompressionType.None)
            {
                dataWriter.WriteInt32BigEndian(uncompressedSize);
            }

            dataWriter.Write(compressedData);

            if (this.Info.CompressionType == CompressionType.None)
            {
                // Uncompressed size includes meta bytes for info when not using compression.
                uncompressedSize = (int)dataStream.Position;
            }

            // Compressed size includes meta bytes for info.
            var compressedSize = (int)dataStream.Position;

            var result = dataStream.ToArray();

            // Calculate new CRC.
            var crcHasher = new Crc32();
            crcHasher.Update(result);
            var crc = (int)crcHasher.Value;

            // Calculate new whirlpool digest.
            var whirlpoolHasher = new WhirlpoolDigest();
            whirlpoolHasher.BlockUpdate(result, 0, compressedSize);
            var whirlpoolDigest = new byte[whirlpoolHasher.GetDigestSize()];
            whirlpoolHasher.DoFinal(whirlpoolDigest, 0);

            // Update file info.
            this.Info.CompressedSize = compressedSize;
            this.Info.UncompressedSize = uncompressedSize;
            this.Info.Crc = crc;
            this.Info.WhirlpoolDigest = whirlpoolDigest;

            return result;
        }
    }
}
