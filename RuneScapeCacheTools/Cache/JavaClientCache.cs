using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache.JavaClient;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    /// Can read and write files to the "virtual filesystem" format that is used by the Java client consisting of a
    /// single data (.dat2) file and some index (.id#) files.
    /// </summary>
    public class JavaClientCache : ReferenceTableCache
    {
        public static string DefaultCacheDirectory => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jagexcache/runescape/LIVE/";

        private const int IndexPointerSize = 6; // filesize + firstSectorPosition

        /// <summary>
        /// The directory where the cache is located.
        /// </summary>
        public string CacheDirectory { get; }

        public bool ReadOnly { get; }

        private Stream _dataStream;

        private Dictionary<CacheIndex, Stream> _indexStreams = new Dictionary<CacheIndex, Stream>();

        /// <summary>
        /// Creates an interface on the cache stored in the given directory.
        /// </summary>
        /// <param name="cacheDirectory"></param>
        /// <param name="readOnly"></param>
        public JavaClientCache(string? cacheDirectory = null, bool readOnly = true)
        {
            this.CacheDirectory = PathExtensions.FixDirectory(cacheDirectory ?? JavaClientCache.DefaultCacheDirectory);
            this.ReadOnly = readOnly;

            // Create the cache directory if writing is allowed.
            if (!this.ReadOnly)
            {
                Directory.CreateDirectory(this.CacheDirectory);
            }

            this.OpenStreams();
        }

        public override IEnumerable<CacheIndex> GetAvailableIndexes()
        {
            return this._indexStreams.Keys.Where(index => index != CacheIndex.ReferenceTables);
        }

        public override void Dispose()
        {
            this.CloseStreams();
        }

        protected override byte[] GetFileData(CacheIndex index, int fileId)
        {
            // Read the sectors and take their payload data up to the size of the contained file.
            return this
                .GetFileSectors(index, fileId, out var filesize)
                .Aggregate(new List<byte>(), (bytes, sector) =>
                {
                    bytes.AddRange(sector.Payload);
                    return bytes;
                })
                .Take(filesize)
                .ToArray();
        }

        protected override void PutFileData(CacheIndex index, int fileId, byte[] data)
        {
            if (this.ReadOnly)
            {
                throw new CacheException("Can't write data in readonly mode.");
            }

            // Obtain existing sectors to overwrite in case the file already exists.
            var existingSectorPositions = new int[0];
            try
            {
                existingSectorPositions = this
                    .GetFileSectors(index, fileId, out _)
                    .Select(sector => sector.Position)
                    .ToArray();
            }
            catch (CacheFileNotFoundException)
            {
            }

            var dataWriter = new BinaryWriter(this._dataStream);
            var sectors = Sector.FromData(data, index, fileId).ToArray();
            foreach (var sector in sectors)
            {
                // Overwrite existing sector data if available, otherwise append to file.
                sector.Position = sector.ChunkIndex < existingSectorPositions.Length
                    ? existingSectorPositions[sector.ChunkIndex]
                    : (int)(dataWriter.BaseStream.Length / Sector.Size);

                // Set position of next sector
                sector.NextSectorPosition = sector.ChunkIndex + 1 < existingSectorPositions.Length
                    ? existingSectorPositions[sector.ChunkIndex + 1]
                    : (int)(dataWriter.BaseStream.Length / Sector.Size);

                // If both positions point toward the end of the stream, increase the next sector position to come after
                // the current one.
                if (sector.NextSectorPosition == sector.Position)
                {
                    sector.NextSectorPosition++;
                }

                // Write the encoded sector
                dataWriter.BaseStream.Position = sector.Position * Sector.Size;
                dataWriter.Write(sector.Encode());
            }

            // Create or overwrite the entry to the file in the index file.

            // Create index file if it does not exist yet.
            if (!this._indexStreams.ContainsKey(index))
            {
                var indexStream = System.IO.File.Open(
                    Path.Combine(this.CacheDirectory, "main_file_cache.idx" + (int)index),
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite
                );
                this._indexStreams.Add(index, indexStream);
            }

            var indexWriter = new BinaryWriter(this._indexStreams[index]);
            var pointerPosition = fileId * JavaClientCache.IndexPointerSize;

            // Write zeroes up to the desired position of the index stream if it is larger than its size.
            if (indexWriter.BaseStream.Length < pointerPosition)
            {
                indexWriter.BaseStream.Position = indexWriter.BaseStream.Length;
                indexWriter.Write(Enumerable.Repeat((byte)0, (int)(pointerPosition - indexWriter.BaseStream.Length)).ToArray());
            }

            indexWriter.BaseStream.Position = pointerPosition;
            indexWriter.WriteUInt24BigEndian(data.Length);
            indexWriter.WriteUInt24BigEndian(sectors[0].Position);
        }

        /// <exception cref="IOException"></exception>
        private void OpenStreams()
        {
            var fileAccess = (this.ReadOnly ? FileAccess.Read : FileAccess.ReadWrite);
            var dataFileMode = (this.ReadOnly ? FileMode.Open : FileMode.OpenOrCreate);
            var dataFilePath = Path.Combine(this.CacheDirectory, "main_file_cache.dat2");

            this._dataStream = System.IO.File.Open(dataFilePath, dataFileMode, fileAccess);

            // Open existing index files.
            for (var indexId = 0; indexId <= 255; indexId++)
            {
                var indexFile = Path.Combine(this.CacheDirectory, "main_file_cache.idx" + indexId);

                if (!System.IO.File.Exists(indexFile))
                {
                    continue;
                }

                this._indexStreams.Add((CacheIndex)indexId, System.IO.File.Open(indexFile, FileMode.Open, fileAccess));
            }
        }

        private void CloseStreams()
        {
            this._dataStream.Close();
            this._dataStream = null;

            foreach (var indexStream in this._indexStreams.Values)
            {
                indexStream.Close();
            }
            this._indexStreams.Clear();
        }

        /// <summary>
        /// Reads the sectors that make up the requested file.
        /// </summary>
        /// <param name="filesize">Contains the size of the file contained in the sectors</param>
        private IEnumerable<Sector> GetFileSectors(CacheIndex index, int fileId, out int filesize)
        {
            if (!this._indexStreams.ContainsKey(index))
            {
                throw new CacheFileNotFoundException($"Cannot read from index {(int)index} as it does not exist.");
            }

            var indexReader = new BinaryReader(this._indexStreams[index]);
            var indexPosition = (long)fileId * JavaClientCache.IndexPointerSize;
            if (indexPosition < 0 || indexPosition >= indexReader.BaseStream.Length)
            {
                throw new CacheFileNotFoundException($"File {fileId} is outside of index {(int)index}'s file bounds.");
            }

            var sectors = new List<Sector>();
            indexReader.BaseStream.Position = indexPosition;

            filesize = indexReader.ReadUInt24BigEndian();
            var firstSectorPosition = indexReader.ReadUInt24BigEndian();
            if (filesize <= 0)
            {
                throw new CacheFileNotFoundException(
                    $"File {fileId} in index {(int)index} has no size meaning it is not stored in the cache."
                );
            }

            var chunkId = 0;
            var remaining = filesize;
            var dataReader = new BinaryReader(this._dataStream);
            var sectorPosition = firstSectorPosition;
            do
            {
                dataReader.BaseStream.Position = sectorPosition * Sector.Size;

                var sectorBytes = dataReader.ReadBytesExactly(Sector.Size);
                var sector = Sector.Decode(sectorPosition, sectorBytes, index, fileId, chunkId++);

                var bytesRead = Math.Min(sector.Payload.Length, remaining);

                remaining -= bytesRead;

                sectors.Add(sector);

                sectorPosition = sector.NextSectorPosition.Value;
            }
            while (remaining > 0);

            return sectors;
        }
    }
}
