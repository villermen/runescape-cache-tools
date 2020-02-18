using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Extension;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.File
{
    /// <summary>
    /// A cache file that contains multiple files.
    /// </summary>
    public class EntryFile : CacheFile
    {
        private readonly SortedDictionary<int, RawCacheFile> _entries = new SortedDictionary<int, RawCacheFile>();

        public int EntryCount => this._entries.Count;

        public bool Empty => !this._entries.Any();

        public bool HasEntry(int entryId) => this._entries.ContainsKey(entryId);

        public T GetEntry<T>(int entryId) where T : CacheFile
        {
            var binaryFile = this._entries[entryId];

            if (typeof(T) == typeof(RawCacheFile))
            {
                return binaryFile as T;
            }

            var file = Activator.CreateInstance<T>();
            file.FromBinaryFile(binaryFile);

            return file;
        }

        public IEnumerable<T> GetEntries<T>() where T : CacheFile
        {
            return this._entries.Keys.Select(this.GetEntry<T>);
        }

        /// <summary>
        /// </summary>
        /// <param name="entryId"></param>
        /// <param name="entry"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void AddEntry(int entryId, CacheFile entry)
        {
            var binaryFileEntry = entry.ToBinaryFile();

            if (binaryFileEntry.Info == null)
            {
                binaryFileEntry.Info = new CacheFileInfo();
            }

            binaryFileEntry.Info.Index = this.Info.CacheIndex;
            binaryFileEntry.Info.FileId = this.Info.FileId;
            binaryFileEntry.Info.EntryId = entryId;

            this._entries.Add(entryId, binaryFileEntry);
        }

        public void AddEntry(int entryId, byte[] entryData)
        {
            this.AddEntry(entryId, new RawCacheFile
            {
                Data = entryData
            });
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

            var entriesData = new byte[this.Info.EntryInfo.Count][];

            var reader = new BinaryReader(new MemoryStream(data));

            reader.BaseStream.Position = reader.BaseStream.Length - 1;
            var amountOfChunks = reader.ReadByte();

            if (amountOfChunks == 0)
            {
                throw new DecodeException("Entry file contains no chunks.");
            }

            // Read the sizes of the child entries and individual chunks
            var sizesStartPosition = reader.BaseStream.Length - 1 - amountOfChunks * this.Info.EntryInfo.Count * 4;
            reader.BaseStream.Position = sizesStartPosition;

            var chunkEntrySizes = new int[amountOfChunks, this.Info.EntryInfo.Count];

            for (var chunkId = 0; chunkId < amountOfChunks; chunkId++)
            {
                var chunkSize = 0;
                for (var entryIndex = 0; entryIndex < this.Info.EntryInfo.Count; entryIndex++)
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
                for (var entryIndex = 0; entryIndex < this.Info.EntryInfo.Count; entryIndex++)
                {
                    // Read the bytes of the entry into the archive entries
                    var entrySize = chunkEntrySizes[chunkId, entryIndex];
                    var entryData = reader.ReadBytes(entrySize);

                    if (entryData.Length != entrySize)
                    {
                        throw new EndOfStreamException("End of file reached while reading the archive.");
                    }

                    // Put or append the entry data to the result
                    entriesData[entryIndex] = chunkId == 0 ? entryData : entriesData[entryIndex].Concat(entryData).ToArray();
                }
            }

            // Convert to binary files and store with the right id
            var entryIds = this.Info.EntryInfo.Keys.ToArray();
            for (var entryIndex = 0; entryIndex < entriesData.Length; entryIndex++)
            {
                this.AddEntry(entryIds[entryIndex], entriesData[entryIndex]);
            }

            if (reader.BaseStream.Position != sizesStartPosition)
            {
                throw new DecodeException($"Not all data or too much data was read while constructing entry file. {sizesStartPosition - reader.BaseStream.Position} bytes remain.");
            }
        }

        public override byte[] Encode()
        {
            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);

            foreach (var entry in this._entries.Values)
            {
                writer.Write(entry.Data);
            }

            // Split entries into multiple chunks TODO: when to split?
            byte amountOfChunks = 1;

            for (var chunkId = 0; chunkId < amountOfChunks; chunkId++)
            {
                // Write delta encoded entry sizes
                var previousEntrySize = 0;
                foreach(var entry in this._entries.Values)
                {
                    var entrySize = entry.Data.Length;

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
