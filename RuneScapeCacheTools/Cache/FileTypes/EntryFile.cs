using System;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Exceptions;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.FileTypes
{
    /// <summary>
    /// A cache file that contains multiple files.
    /// </summary>
    public class EntryFile : CacheFile
    {
        // TODO: Store as BinaryFile and clone when requested as one
        private byte[][] _entryData;

        public int EntryCount => this._entryData.Length;

        public T GetEntry<T>(int entryId) where T : CacheFile
        {
            var binaryFile = new BinaryFile
            {
                Data = this._entryData[entryId],
                Info =  new CacheFileInfo
                {
                    Index = this.Info.Index,
                    FileId =  this.Info.FileId,
                    EntryId = entryId
                }
            };

            if (typeof(T) == typeof(BinaryFile))
            {
                return binaryFile as T;
            }

            var file = Activator.CreateInstance<T>();
            file.FromBinaryFile(binaryFile);

            return file;
        }

        public T[] GetEntries<T>() where T : CacheFile
        {
            return Enumerable.Range(0, this.EntryCount)
                .Select(this.GetEntry<T>)
                .ToArray();
        }

        public void AddEntry(int entryId, CacheFile entry)
        {
            throw new NotImplementedException();
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

            this._entryData = new byte[this.Info.Entries.Count][];

            var reader = new BinaryReader(new MemoryStream(data));

            reader.BaseStream.Position = reader.BaseStream.Length - 1;
            var amountOfChunks = reader.ReadByte();

            if (amountOfChunks == 0)
            {
                throw new DecodeException("Entry file contains no chunks.");
            }

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
                    this._entryData[entryId] = chunkId == 0 ? entryData : this._entryData[entryId].Concat(entryData).ToArray();
                }
            }
        }

        public override byte[] Encode()
        {
            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);

            foreach (var entryData in this._entryData)
            {
                writer.Write(entryData);
            }

            // TODO: Split entries into multiple chunks (when?)
            byte amountOfChunks = 1;

            for (var chunkId = 0; chunkId < amountOfChunks; chunkId++)
            {
                // Write delta encoded entry sizes
                var previousEntrySize = 0;
                foreach (var entry in this._entryData)
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