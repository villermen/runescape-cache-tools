using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Utility
{
    public class RuneTek7CacheFileDecoder : RuneTek5CacheFileDecoder
    {
        public override CacheFile DecodeFile(byte[] encodedData, CacheFileInfo? info)
        {
            info ??= new CacheFileInfo();

            using var encodedDataStream = new MemoryStream(encodedData);
            using var encodedDataReader = new BinaryReader(encodedDataStream);

            // Decode zlib wrapper.
            byte[] data;
            if (
                // Not sure if the final 0x01 has a special meaning and can be different.
                encodedData.Length >= 4 &&
                encodedDataReader.ReadBytesExactly(4).SequenceEqual(new byte[] { (byte)'Z', (byte)'L', (byte)'B', 0x01 })
            )
            {
                var uncompressedSize = encodedDataReader.ReadInt32BigEndian();
                data = this.DecompressZlib(encodedDataStream, uncompressedSize);

                // The file's info lists details of the RT5 variant of the file so we can't verify much here.
                info.CompressionType = CompressionType.Zlib;
                info.UncompressedSize = uncompressedSize;
            }
            else
            {
                data = this.DecodeData(encodedData, info);
            }

            if (!info.HasEntries)
            {
                return new CacheFile(data, info);
            }

            var entries = this.DecodeEntries(data, info.Entries.Keys.ToArray());
            return new CacheFile(entries, info);
        }

        private byte[] DecompressZlib(Stream compressedDataStream, int uncompressedSize)
        {
            using var compressedDataReader = new BinaryReader(compressedDataStream);

            // Remove the Zlib file header. We only want the DEFLATE stream.
            var zlibFileHeader = compressedDataReader.ReadBytesExactly(2);
            if (!zlibFileHeader.SequenceEqual(new byte[] { 0x78, 0x9C }))
            {
                throw new DecodeException("Unexpected continuation of zlib-compressed file.");
            }

            var decompressionStream = new DeflateStream(compressedDataStream, CompressionMode.Decompress);
            var decompressionReader = new BinaryReader(decompressionStream);
            return decompressionReader.ReadBytesExactly(uncompressedSize);
        }

        public new SortedDictionary<int, byte[]> DecodeEntries(byte[] data, int[] entryIds)
        {
            /*
             * Format visualization (e = entry, c = chunk):
             * [amountOfChunks]
             * [sizeOfHeader]
             * Delta-difference-encoded chunk sizes (starts with size of header): [e1c1][e2c1][e3c1] [e1c2][e2c2][e3c2]
             * Chunk data: [e1c1][e2c1][e3c1] [e1c2][e2c2][e3c1]
             */

            using var dataStream = new MemoryStream(data, false);
            using var dataReader = new BinaryReader(dataStream);

            var amountOfEntries = entryIds.Length;

            // Read the amount of chunks.
            var amountOfChunks = dataReader.ReadByte();
            if (amountOfChunks == 0)
            {
                throw new DecodeException("Entry file contains no chunks = no entries.");
            }

            var headerLength = dataReader.ReadInt32BigEndian();

            // Read the delta-encoded chunk sizes.
            var entryChunkSizes = new int[amountOfEntries, amountOfChunks];
            var delta = headerLength;
            for (var chunkIndex = 0; chunkIndex < amountOfChunks; chunkIndex++)
            {
                for (var entryIndex = 0; entryIndex < amountOfEntries; entryIndex++)
                {
                    var previousDelta = delta;
                    delta = dataReader.ReadInt32BigEndian();

                    var chunkSize = delta - previousDelta;
                    entryChunkSizes[entryIndex, chunkIndex] = chunkSize;
                }
            }

            if (dataStream.Position != headerLength)
            {
                throw new DecodeException(
                    $"Not all or too much header data was consumed while decoding entries. {headerLength - dataStream.Position} bytes remain."
                );
            }

            // Read the entry data.
            var entryData = new byte[amountOfEntries][];
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

            if (dataStream.Position != dataStream.Length)
            {
                throw new DecodeException(
                    $"Not all or too much data was consumed while decoding entries. {dataStream.Length - dataStream.Position} bytes remain."
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

        public override byte[] EncodeFile(CacheFile file, CacheFileInfo? info)
        {
            var data = file.HasEntries ? this.EncodeEntries(file.Entries, info) : file.Data;

            var compressionType = info?.CompressionType ?? CompressionType.Zlib;
            if (compressionType == CompressionType.Zlib)
            {
                using var encodedDataStream = new MemoryStream();
                using var encodedDataWriter = new BinaryWriter(encodedDataStream);

                encodedDataWriter.Write((byte)'Z');
                encodedDataWriter.Write((byte)'L');
                encodedDataWriter.Write((byte)'B');
                encodedDataWriter.Write((byte)0x01);
                encodedDataWriter.WriteInt32BigEndian(data.Length);
                encodedDataWriter.Write((byte)0x78);
                encodedDataWriter.Write((byte)0x9C);

                using (var compressionStream = new DeflateStream(encodedDataStream, CompressionMode.Compress))
                using (var compressionWriter = new BinaryWriter(compressionStream))
                {
                    compressionWriter.Write(data);
                }

                // Info is not changed for zlib because info does not describe zlib data.

                return encodedDataStream.ToArray();
            }

            return this.EncodeData(data, info);
        }

        public new byte[] EncodeEntries(Dictionary<int, byte[]> entries, CacheFileInfo? info)
        {
            // Sort entries (encodes more efficiently).
            entries = entries.OrderBy(entryPair => entryPair.Key).ToDictionary(
                entryPair => entryPair.Key,
                entryPair => entryPair.Value
            );

            using var dataStream = new MemoryStream();
            using var dataWriter = new BinaryWriter(dataStream);

            // Write amount of chunks.
            dataWriter.Write((byte)1);

            // Write header size.
            var headerSize = 1 + 4 + entries.Count * 4;
            dataWriter.WriteInt32BigEndian(headerSize);

            // Write delta-difference encoded entry sizes.
            var delta = headerSize;
            foreach (var entryData in entries.Values)
            {
                delta += entryData.Length;
                dataWriter.WriteInt32BigEndian(delta);
            }

            // I don't know why splitting into chunks is necessary/desired so I just use one. This also happens to
            // greatly simplify this logic.
            foreach (var entryData in entries.Values)
            {
                dataWriter.Write(entryData);
            }

            // Update info.
            if (info != null)
            {
                info.Entries = entries.Keys.ToDictionary(
                    entryId => entryId,
                    entryId => new CacheFileEntryInfo()
                );
            }

            return dataStream.ToArray();
        }
    }
}
