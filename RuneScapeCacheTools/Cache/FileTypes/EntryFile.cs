using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.FileTypes
{
    /// <summary>
    /// A cache file that contains multiple files.
    /// </summary>
    public class EntryFile : CacheFile
    {
        // TODO: Return entries as new objects
        public IDictionary<int, BinaryFile> Entries = new SortedDictionary<int, BinaryFile>();

        public T GetEntry<T>(int entryId) where T : CacheFile
        {
            if (typeof(T) == typeof(BinaryFile))
            {
                return this.Entries[entryId] as T;
            }

            var file = Activator.CreateInstance<T>();
            file.FromFile(this.Entries[entryId]);

            return file;
        }

        public Dictionary<int, T> GetEntries<T>() where T : CacheFile
        {
            if (typeof(T) == typeof(BinaryFile))
            {
                return this.Entries as Dictionary<int, T>;
            }

            return this.Entries
                .Select(pair =>
                {
                    var file = Activator.CreateInstance<T>();
                    file.FromFile(pair.Value);

                    return new
                    {
                        Index = pair.Key,
                        File = file
                    };
                })
                .ToDictionary(pair => pair.Index, pair => pair.File);
        }

        public override void Decode(byte[] data)
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

            var entryDataCollection = new byte[this.Info.Entries.Count][];

            var reader = new BinaryReader(new MemoryStream(data));

            reader.BaseStream.Position = reader.BaseStream.Length - 1;
            var amountOfChunks = reader.ReadByte();

            // Read the sizes of the child entries and individual chunks
            var chunkEntrySizes = new int[amountOfChunks, this.Info.Entries.Count];

            reader.BaseStream.Position = reader.BaseStream.Length - 1 - amountOfChunks * this.Info.Entries.Count * 4;

            for (var chunkId = 0; chunkId < amountOfChunks; chunkId++)
            {
                var chunkSize = 0;
                for (var entryId = 0; entryId < this.Info.Entries.Count; entryId++)
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
                for (var entryId = 0; entryId < this.Info.Entries.Count; entryId++)
                {
                    // Read the bytes of the entry into the archive entries
                    var entrySize = chunkEntrySizes[chunkId, entryId];
                    var entryData = reader.ReadBytes(entrySize);

                    if (entryData.Length != entrySize)
                    {
                        throw new EndOfStreamException("End of file reached while reading the archive.");
                    }

                    // Put or append the entry data to the result
                    entryDataCollection[entryId] = chunkId == 0 ? entryData : entryDataCollection[entryId].Concat(entryData).ToArray();
                }
            }

            // Convert raw data to binary files
            this.Entries = entryDataCollection
                .Where(entryData => entryData.Length > 0)
                .Select((entryData, index) => new
                {
                    Index = index,
                    File = new BinaryFile
                    {
                        Data = entryData
                        // TODO: Add entry info
                    }
                })
                .ToDictionary(pair => pair.Index, pair => pair.File);
        }

        public override byte[] Encode()
        {
            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);

            // Get data for all entries, including empty ones
            var entryData = new byte[this.Entries.Last().Key + 1][];

            for (var entryId = 0; entryId < entryData.Length; entryId++)
            {
                entryData[entryId] = this.Entries.ContainsKey(entryId) ? this.Entries[entryId].Encode() : new byte[0];

                writer.Write(entryData[entryId]);
            }

            // TODO: Split entries into multiple chunks (when?)
            byte amountOfChunks = 1;

            for (var chunkId = 0; chunkId < amountOfChunks; chunkId++)
            {
                // Write delta encoded entry sizes
                var previousEntrySize = 0;
                foreach (var entry in entryData)
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