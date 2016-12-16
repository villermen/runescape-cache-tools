using System;
using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    using System.Linq;
    using System.Reflection;
    using NAudio.Wave;
    using Org.BouncyCastle.Crypto.Tls;
    using Villermen.RuneScapeCacheTools.Cache.CacheFile;

    /// <summary>
    ///     A file store holds multiple files inside a "virtual" file system made up of several index files and a single data
    ///     file.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Villermen</author>
    // TODO: See if this class can be replaced by RuneTek5Cache and a Sector class replacement that can read full files from a stream.
    public class FileStore : IDisposable
    {
        /// <summary>
        ///     Lock that is used when reading data from the streams.
        /// </summary>
        private readonly object ioLock = new object();

        private readonly Dictionary<Index, Stream> indexStreams = new Dictionary<Index, Stream>();

        private readonly Stream dataStream;

        /// <summary>
        ///     Opens the file store in the specified directory.
        /// </summary>
        /// <param name="cacheDirectory">The directory containing the index and data files.</param>
        /// <param name="readOnly">No empty cache will be initialized if only reading, and writing will be disallowed.</param>
        /// <exception cref="CacheException">If any of the main_file_cache.* files could not be found.</exception>
        public FileStore(string cacheDirectory, bool readOnly = true)
        {
            this.CacheDirectory = PathExtensions.FixDirectory(cacheDirectory);
            this.ReadOnly = readOnly;

            if (!this.ReadOnly)
            {
                Directory.CreateDirectory(this.CacheDirectory);
            }

            var fileAccess = this.ReadOnly ? FileAccess.Read : FileAccess.ReadWrite;

            var dataFilePath = Path.Combine(this.CacheDirectory, "main_file_cache.dat2");

            if (this.ReadOnly && !File.Exists(dataFilePath))
            {
                throw new CacheException("Cache data file does not exist.");
            }

            this.dataStream = File.Open(dataFilePath, FileMode.OpenOrCreate, fileAccess);

            // Load in existing index files
            for (var indexId = 0; indexId <= 255; indexId++)
            {
                var indexFile = Path.Combine(this.CacheDirectory, "main_file_cache.idx" + indexId);

                if (!File.Exists(indexFile))
                {
                    continue;
                }

                this.indexStreams.Add((Index)indexId, File.Open(indexFile, FileMode.Open, fileAccess));
            }
        }

        public bool ReadOnly { get; private set; }

        public string CacheDirectory { get; private set; }

        /// <summary>
        ///     The number of indexes.
        /// </summary>
        public int IndexCount => this.indexStreams.Count;

        /// <summary>
        /// Reads the sectors
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public byte[] ReadFileData(Index index, int fileId)
        {
            int filesize;
            return this.ReadSectors(index, fileId, out filesize).Aggregate(new List<byte>(), (bytes, sector) =>
            {
                bytes.AddRange(sector.Data);
                return bytes;
            }).Take(filesize).ToArray();
        }

        private IEnumerable<Sector> ReadSectors(Index index, int fileId)
        {
            int filesize;
            return this.ReadSectors(index, fileId, out filesize);
        }

        private IEnumerable<Sector> ReadSectors(Index index, int fileId, out int filesize)
        {
            if (!this.indexStreams.ContainsKey(index))
            {
                throw new CacheFileNotFoundException($"Index does not exist for {index}/{fileId}.");
            }

            var indexReader = new BinaryReader(this.indexStreams[index]);

            var indexPosition = (long)fileId * IndexPointer.Length;

            if (indexPosition < 0 || indexPosition >= indexReader.BaseStream.Length)
            {
                throw new CacheFileNotFoundException($"{index}/{fileId} is outside of the index file's bounds.");
            }

            var sectors = new List<Sector>();

            // Lock stream, to allow multiple threads from calling this method at the same time
            lock (this.ioLock)
            {
                indexReader.BaseStream.Position = indexPosition;
                var indexPointer = new IndexPointer(indexReader.ReadBytes(IndexPointer.Length));

                filesize = indexPointer.Filesize;

                if (indexPointer.Filesize <= 0)
                {
                    throw new CacheFileNotFoundException($"{index}/{fileId} has no size, which means it is either empty or not present.");
                }

                var chunkId = 0;
                var remaining = indexPointer.Filesize;
                var dataReader = new BinaryReader(this.dataStream);
                var dataPosition = (long)indexPointer.FirstSectorPosition * Sector.Length;

                do
                {
                    dataReader.BaseStream.Position = dataPosition;

                    var sectorBytes = dataReader.ReadBytes(Sector.Length);

                    if (sectorBytes.Length != Sector.Length)
                    {
                        throw new CacheFileNotFoundException($"One of {index}/{fileId}'s sectors could not be fully read.");
                    }

                    var sector = new Sector((int)(dataPosition / Sector.Length), index, fileId, chunkId++, sectorBytes);

                    var bytesRead = Math.Min(sector.Data.Length, remaining);

                    remaining -= bytesRead;

                    dataPosition = (long)sector.NextSectorPosition * Sector.Length;

                    sectors.Add(sector);
                }
                while (remaining > 0);
            }

            return sectors;
        }

        /// <summary>
        /// If available, overwrites the space allocated to the previous file first to save space.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fileId"></param>
        /// <param name="data"></param>
        public void WriteFileData(Index index, int fileId, byte[] data)
        {
            if (this.ReadOnly)
            {
                throw new InvalidOperationException("Can't write data in readonly mode.");
            }

            var sectors = Sector.FromData(data, index, fileId);

            lock (this.ioLock)
            {
                // Obtain possibly existing sector positions to overwrite
                int[] existingSectorPositions;
                try
                {
                    existingSectorPositions = this.ReadSectors(index, fileId)
                        .Select(sector => sector.Position)
                        .ToArray();
                }
                catch (Exception ex) when (ex is CacheException || ex is SectorException)
                {
                    // Assume there are no existing sectors when the method fails
                    existingSectorPositions = new int[0];
                }

                var dataWriter = new BinaryWriter(this.dataStream);

                foreach (var sector in sectors)
                {
                    // Overwrite existing sector data if available, otherwise append to file
                    sector.Position = sector.ChunkId < existingSectorPositions.Length
                        ? existingSectorPositions[sector.ChunkId]
                        : (int)Math.Ceiling(dataWriter.BaseStream.Length / 520D);

                    // Set position of next sector
                    sector.NextSectorPosition = sector.ChunkId + 1 < existingSectorPositions.Length
                        ? existingSectorPositions[sector.ChunkId + 1]
                        : sector.Position + 1;

                    // Add to index
                    if (sector.ChunkId == 0)
                    {
                        var pointer = new IndexPointer(sector.Position, data.Length);

                        // Create index file if it does not exist yet
                        if (!this.indexStreams.ContainsKey(index))
                        {
                            this.indexStreams.Add(index, File.Open(
                                Path.Combine(this.CacheDirectory, "main_file_cache.idx" + (int)index),
                                FileMode.OpenOrCreate,
                                FileAccess.ReadWrite));
                        }

                        var indexWriter = new BinaryWriter(this.indexStreams[index]);
                        var pointerPosition = fileId * IndexPointer.Length;

                        // Write zeroes up to the desired position of the index stream if it is larger than its size
                        if (indexWriter.BaseStream.Length < pointerPosition)
                        {
                            indexWriter.BaseStream.Position = indexWriter.BaseStream.Length;
                            indexWriter.Write(Enumerable.Repeat((byte)0, (int)(pointerPosition - indexWriter.BaseStream.Length)).ToArray());
                        }

                        indexWriter.BaseStream.Position = pointerPosition;
                        pointer.Encode(indexWriter.BaseStream);
                    }

                    // Write the encoded sector
                    dataWriter.BaseStream.Position = sector.Position * Sector.Length;
                    dataWriter.Write(sector.Encode());
                }
            }
        }

        public void Dispose()
        {
            this.dataStream.Dispose();

            foreach (var indexStream in this.indexStreams.Values)
            {
                indexStream.Dispose();
            }
        }
    }
}