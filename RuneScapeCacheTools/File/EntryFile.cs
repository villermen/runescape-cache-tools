using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.File
{
    /// <summary>
    /// A cache file that contains multiple files.
    /// </summary>
    public class EntryFile
    {
        public readonly SortedDictionary<int, byte[]> Entries = new SortedDictionary<int, byte[]>();

        public static EntryFile Decode(RuneTek5CacheFile file)
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

            var entriesData = new byte[file.Info.Entries.Count][];

            var reader = new BinaryReader(new MemoryStream(file.Data));

            reader.BaseStream.Position = reader.BaseStream.Length - 1;
            var amountOfChunks = reader.ReadByte();

            if (amountOfChunks == 0)
            {
                throw new DecodeException("Entry file contains no chunks.");
            }

            // Read the sizes of the child entries and individual chunks
            var sizesStartPosition = reader.BaseStream.Length - 1 - amountOfChunks * file.Info.Entries.Count * 4;
            reader.BaseStream.Position = sizesStartPosition;

            var chunkEntrySizes = new int[amountOfChunks, file.Info.Entries.Count];

            for (var chunkId = 0; chunkId < amountOfChunks; chunkId++)
            {
                var chunkSize = 0;
                for (var entryIndex = 0; entryIndex < file.Info.Entries.Count; entryIndex++)
                {
                    // Read the delta encoded chunk length
                    var delta = reader.ReadInt32BigEndian();
                    chunkSize += delta;

                    // Store the size of this entry in this chunk
                    chunkEntrySizes[chunkId, entryIndex] = chunkSize;
                }
            }

            // Read the data
            reader.BaseStream.Position = 0;
            for (var chunkId = 0; chunkId < amountOfChunks; chunkId++)
            {
                for (var entryIndex = 0; entryIndex < file.Info.Entries.Count; entryIndex++)
                {
                    // Read the bytes of the entry into the archive entries
                    var entrySize = chunkEntrySizes[chunkId, entryIndex];
                    var entryData = reader.ReadBytesExactly(entrySize);

                    if (entryData.Length != entrySize)
                    {
                        throw new EndOfStreamException("End of file reached while reading the archive.");
                    }

                    // Put or append the entry data to the result
                    entriesData[entryIndex] = chunkId == 0 ? entryData : entriesData[entryIndex].Concat(entryData).ToArray();
                }
            }

            if (reader.BaseStream.Position != sizesStartPosition)
            {
                throw new DecodeException($"Not all data or too much data was read while constructing entry file. {sizesStartPosition - reader.BaseStream.Position} bytes remain.");
            }

            // Create file and add the entries.
            var entryFile = new EntryFile();
            var entryIds = file.Info.Entries.Keys.ToArray();
            for (var entryIndex = 0; entryIndex < entriesData.Length; entryIndex++)
            {
                entryFile.Entries.Add(entryIds[entryIndex], entriesData[entryIndex]);
            }

            return entryFile;
        }

        // TODO: Encode to RuneTek5File with info instead? Or allow passing of info that will have entry info filled out?
        public byte[] Encode()
        {
            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);

            foreach (var entryData in this.Entries.Values)
            {
                writer.Write(entryData);
            }

            // Split entries into multiple chunks TODO: when to split?
            byte amountOfChunks = 1;

            for (var chunkId = 0; chunkId < amountOfChunks; chunkId++)
            {
                // Write delta encoded entry sizes
                var previousEntrySize = 0;
                foreach (var entryData in this.Entries.Values)
                {
                    var entrySize = entryData.Length;

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
