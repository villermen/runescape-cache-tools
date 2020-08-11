using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.GZip;
using Org.BouncyCastle.Crypto.Digests;
using Serilog;
using SevenZip.Compression.LZMA;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Utility
{
    public class RuneTek5CacheFileDecoder : ICacheFileDecoder
    {
        public virtual CacheFile DecodeFile(byte[] encodedData, CacheFileInfo? info)
        {
            info ??= new CacheFileInfo();

            var data = this.DecodeData(encodedData, info);

            if (!info.HasEntries)
            {
                return new CacheFile(data, info);
            }

            var entries = this.DecodeEntries(data, info.Entries.Keys.ToArray());
            return new CacheFile(entries, info);
        }

        protected byte[] DecodeData(byte[] encodedData, CacheFileInfo info)
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

            var compressionType = (CompressionType)dataReader.ReadByte();
            var compressedSize = dataReader.ReadInt32BigEndian();
            var uncompressedSize = compressedSize;
            if (compressionType != CompressionType.None)
            {
                uncompressedSize = dataReader.ReadInt32BigEndian();
            }

            var data = this.Decompress(
                compressionType,
                dataReader.ReadBytesExactly(compressedSize),
                uncompressedSize
            );

            // Compressed size includes meta bytes for info.
            compressedSize = (int)dataStream.Position;

            // Verify compressed size. Info's compressed size includes meta bytes.
            if (info.CompressedSize != null && compressedSize != info.CompressedSize)
            {
                throw new DecodeException(
                    $"Compressed size ({compressedSize}) does not equal expected ({info.CompressedSize})."
                );
            }

            // Verify uncompressed size.
            if (info.UncompressedSize != null && uncompressedSize != info.UncompressedSize)
            {
                // Some uncompressed files _do_ seem to include meta bytes into the uncompressed size. Allow for now.
                // TODO: Figure out when uncompressed size includes the meta bytes. Is this only true for audio files?
                var message = $"Uncompressed size ({uncompressedSize}) does not equal expected ({info.UncompressedSize}).";
                if (compressionType == CompressionType.None && uncompressedSize + 5 == info.UncompressedSize)
                {
                    Log.Debug(message + " (allowed)");
                }
                else
                {
                    throw new DecodeException(message);
                }
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
                throw new DecodeException(
                    $"Input data not fully consumed while decoding RuneTek5CacheFile. {dataStream.Length - dataStream.Position} bytes remain."
                );
            }

            // Update info with obtained details.
            info.CompressionType = compressionType;
            info.CompressedSize = compressedSize;
            info.UncompressedSize = uncompressedSize;
            info.Crc = crc;

            return data;
        }

        public virtual SortedDictionary<int, byte[]> DecodeEntries(byte[] data, int[] entryIds)
        {
            /*
             * Format visualization (e = entry, c = chunk):
             * Chunk data: [e1c1][e2c1][e3c1] [e1c2][e2c2][e3c1]
             * Delta-encoded chunk sizes: [e1c1][e2c1][e3c1] [e1c2][e2c2][e3c2]
             * [amountOfChunks]
             *
             * I have no idea why it works back to front either =S
             */

            using var dataStream = new MemoryStream(data, false);
            using var dataReader = new BinaryReader(dataStream);

            var amountOfEntries = entryIds.Length;

            // Read the amount of chunks.
            dataStream.Position = dataStream.Length - 1;
            var amountOfChunks = dataReader.ReadByte();
            if (amountOfChunks == 0)
            {
                throw new DecodeException("Entry file contains no chunks = no entries.");
            }

            // Read the delta-encoded chunk sizes.
            var sizesStartPosition = dataStream.Length - 1 - 4 * amountOfChunks * amountOfEntries;
            dataStream.Position = sizesStartPosition;

            var entryChunkSizes = new int[amountOfEntries, amountOfChunks];
            for (var chunkIndex = 0; chunkIndex < amountOfChunks; chunkIndex++)
            {
                var chunkSize = 0;
                for (var entryIndex = 0; entryIndex < amountOfEntries; entryIndex++)
                {
                    var delta = dataReader.ReadInt32BigEndian();
                    chunkSize += delta;
                    entryChunkSizes[entryIndex, chunkIndex] = chunkSize;
                }
            }

            // Read the entry data.
            var entryData = new byte[amountOfEntries][];
            dataStream.Position = 0;
            for (var chunkIndex = 0; chunkIndex < amountOfChunks; chunkIndex++)
            {
                for (var entryIndex = 0; entryIndex < amountOfEntries; entryIndex++)
                {
                    // Read the chunk data.
                    var entrySize = entryChunkSizes[entryIndex, chunkIndex];
                    var chunkData = dataReader.ReadBytesExactly(entrySize);

                    // Add the chunk data to the entry data.
                    entryData[entryIndex] = chunkIndex == 0 ? chunkData : entryData[entryIndex].Concat(chunkData).ToArray();
                }
            }

            if (dataStream.Position != sizesStartPosition)
            {
                throw new DecodeException(
                    $"Not all or too much data was consumed while decoding entries. {sizesStartPosition - dataStream.Position} bytes remain."
                );
            }

            // Combine entry keys and values.
            var entries = new SortedDictionary<int, byte[]>();
            for (var entryIndex = 0; entryIndex < amountOfEntries; entryIndex++)
            {
                entries.Add(entryIds[entryIndex], entryData[entryIndex]);
            }
            return entries;
        }

        private byte[] Decompress(CompressionType compressionType, byte[] compressedData, int uncompressedSize)
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

        public virtual byte[] EncodeFile(CacheFile file)
        {
            // Encrypt data
            if (file.Info.EncryptionKey != null)
            {
                throw new EncodeException(
                    "XTEA encryption not supported. If you encounter this please inform me about the index and file that triggered this message."
                );
            }

            // Encode entries
            if (file.HasEntries)
            {
                this.EncodeEntries(file.Entries, file.Info);
            }

            // Compression
            var uncompressedSize = file.Data.Length;
            var compressedData = this.CompressData(file.Info.CompressionType, file.Data);

            using var dataStream = new MemoryStream();
            using var dataWriter = new BinaryWriter(dataStream);

            dataWriter.Write((byte)file.Info.CompressionType);
            dataWriter.WriteInt32BigEndian(compressedData.Length);

            // Add uncompressed size if compression is used.
            if (file.Info.CompressionType != CompressionType.None)
            {
                dataWriter.WriteInt32BigEndian(uncompressedSize);
            }

            dataWriter.Write(compressedData);

            if (file.Info.CompressionType == CompressionType.None)
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
            file.Info.CompressedSize = compressedSize;
            file.Info.UncompressedSize = uncompressedSize;
            file.Info.Crc = crc;
            file.Info.WhirlpoolDigest = whirlpoolDigest;

            return result;
        }

        private byte[] CompressData(CompressionType compressionType, byte[] data)
        {
            if (compressionType == CompressionType.None)
            {
                return data;
            }

            if (compressionType == CompressionType.Bzip2)
            {
                using var outputStream = new MemoryStream();
                BZip2.Compress(new MemoryStream(data), outputStream, true, 1);
                // Remove BZh1 (note that 1 is the block size/compression level).
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

        public virtual byte[] EncodeEntries(SortedDictionary<int, byte[]> entries, CacheFileInfo info)
        {
            using var dataStream = new MemoryStream();
            using var dataWriter = new BinaryWriter(dataStream);

            // I don't know why splitting into chunks is necessary/desired so I just use one. This also happens to
            // greatly simplify this logic.
            foreach (var entryData in entries.Values)
            {
                dataWriter.Write(entryData);
            }

            // Write delta encoded entry sizes.
            var previousEntrySize = 0;
            foreach (var entryData in entries.Values)
            {
                var entrySize = entryData.Length;
                var delta = entrySize - previousEntrySize;

                dataWriter.WriteInt32BigEndian(delta);

                previousEntrySize = entrySize;
            }

            // Write amount of chunks.
            dataWriter.Write((byte)1);

            // Update info.
            info.Entries = new SortedDictionary<int, CacheFileEntryInfo>(entries.Keys.ToDictionary(
                entryId => entryId,
                entryId => new CacheFileEntryInfo()
            ));

            return dataStream.ToArray();
        }
    }
}
