using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.File
{
    /// <summary>
    /// A cache file that contains multiple files.
    /// </summary>
    public class EntryFile
    {
        public readonly SortedDictionary<int, byte[]> Entries = new SortedDictionary<int, byte[]>();

        /// <exception cref="DecodeException"></exception>
        public static EntryFile DecodeFromCacheFile(CacheFile cacheFile)
        {
            if (!cacheFile.Info.HasEntries)
            {
                throw new DecodeException("Passed CacheFile does not have entries to decode.");
            }

            return EntryFile.Decode(cacheFile.Data, cacheFile.Info.Entries.Keys.ToArray());
        }

        /// <exception cref="DecodeException"></exception>
        public static EntryFile Decode(byte[] data, int[] entryIds)
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
                    $"Not all or too much data was consumed while constructing entry file. {sizesStartPosition - dataStream.Position} bytes remain."
                );
            }

            // Create file and add the entries.
            var entryFile = new EntryFile();
            for (var entryIndex = 0; entryIndex < amountOfEntries; entryIndex++)
            {
                entryFile.Entries.Add(entryIds[entryIndex], entryData[entryIndex]);
            }

            return entryFile;
        }

        public byte[] Encode(out int[] entryIds)
        {
            using var dataStream = new MemoryStream();
            using var dataWriter = new BinaryWriter(dataStream);

            // I don't know why splitting into chunks is necessary/desired so I just use one. This also happens to
            // greatly simplify this logic.
            foreach (var entryData in this.Entries.Values)
            {
                dataWriter.Write(entryData);
            }

            // Write delta encoded entry sizes.
            var previousEntrySize = 0;
            foreach (var entryData in this.Entries.Values)
            {
                var entrySize = entryData.Length;
                var delta = entrySize - previousEntrySize;

                dataWriter.WriteInt32BigEndian(delta);

                previousEntrySize = entrySize;
            }

            // Write amount of chunks.
            dataWriter.Write((byte)1);

            // Technically you could obtain the entry IDs by doing this yourself but this is more explicit.
            entryIds = this.Entries.Keys.ToArray();

            return dataStream.ToArray();
        }

        /// <summary>
        /// Creates a new <see cref="CacheFile" /> with the encoded entry data and entry info set.
        /// </summary>
        public CacheFile EncodeToCacheFile()
        {
            var data = this.Encode(out var entryIds);
            return new CacheFile(data)
            {
                Info =
                {
                    Entries = new SortedDictionary<int, CacheFileEntryInfo>(entryIds.ToDictionary(
                        elementId => elementId,
                        entryId => new CacheFileEntryInfo()
                    ))
                }
            };
        }
    }
}
