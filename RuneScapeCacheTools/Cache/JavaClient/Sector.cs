using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Extension;

namespace Villermen.RuneScapeCacheTools.Model
{
    /// <summary>
    /// Represents a sector in the data file, containing some metadata and the actual data contained in the sector.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Villermen</author>
    public class Sector
    {
        /// <summary>
        /// The total size of a sector in bytes.
        /// </summary>
        public const int Length = 520;

        /// <summary>
        /// The size of the header within a sector in bytes.
        /// </summary>
        private const int StandardHeaderLength = 8;

        /// <summary>
        /// The size of the data within a sector in bytes.
        /// </summary>
        private const int StandardDataLength = 512;

        /// <summary>
        /// The extended data size
        /// </summary>
        private const int ExtendedDataLength = 510;

        /// <summary>
        /// The extended header size
        /// </summary>
        private const int ExtendedHeaderLength = 10;

        public Sector()
        {
        }

        /// <summary>
        /// Decodes the given byte array into a <see cref="Sector" /> object.
        /// </summary>
        public Sector(int position, CacheIndex expectedCacheIndex, int expectedFileId, int expectedChunkId, byte[] data)
        {
            this.Position = position;

            if (data.Length != Sector.Length)
            {
                throw new ArgumentException(
                    $"Sector data must be exactly {Sector.Length} bytes in length, {data.Length} given."
                );
            }

            var dataReader = new BinaryReader(new MemoryStream(data));

            // Obtain and verify if the chunk contains what we expect
            this.FileId = (Sector.GetExtended(expectedFileId) ? dataReader.ReadInt32BigEndian() : dataReader.ReadUInt16BigEndian());

            if (this.FileId != expectedFileId)
            {
                throw new DecodeException($"File id mismatch. Expected {expectedFileId}, got {this.FileId}.");
            }

            this.ChunkId = dataReader.ReadUInt16BigEndian();

            if (this.ChunkId != expectedChunkId)
            {
                throw new DecodeException($"Chunk id mismatch. Expected {expectedChunkId}, got {this.ChunkId}.");
            }

            this.NextSectorPosition = dataReader.ReadUInt24BigEndian();
            this.CacheIndex = (CacheIndex)dataReader.ReadByte();

            if (this.CacheIndex != expectedCacheIndex)
            {
                throw new DecodeException($"Index id mismatch. Expected {expectedCacheIndex}, got {this.CacheIndex}.");
            }

            this.Data = dataReader.ReadBytes(this.IsExtended ? Sector.ExtendedDataLength : Sector.StandardDataLength);
        }

        public int Position { get; set; }

        /// <summary>
        /// The chunk within the file that this sector contains.
        /// </summary>
        public int ChunkId { get; set; }

        /// <summary>
        /// The data in this sector.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// The id of the file this sector contains.
        /// </summary>
        public int FileId { get; private set; }

        /// <summary>
        /// The type of file this sector contains.
        /// </summary>
        public CacheIndex CacheIndex { get; set; }

        /// <summary>
        /// The position of next sector.
        /// </summary>
        public int NextSectorPosition { get; set; }

        /// <summary>
        /// Gets whether the sector uses the extended format.
        /// Extended sectors use 4 bytes instead of 2 for the file id (and have 2 bytes less to use for the data).
        /// Jagex did not expect file indexes to surpass the size of a short =)
        /// </summary>
        public bool IsExtended => Sector.GetExtended(this.FileId);

        /// <summary>
        /// Encodes this <see cref="Sector" /> into a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] Encode()
        {
            var dataStream = new MemoryStream(new byte[Sector.Length]);
            var dataWriter = new BinaryWriter(dataStream);

            if (this.IsExtended)
            {
                dataWriter.WriteInt32BigEndian(this.FileId);
            }
            else
            {
                dataWriter.WriteUInt16BigEndian((ushort)this.FileId);
            }

            dataWriter.WriteUInt16BigEndian((ushort)this.ChunkId);
            dataWriter.WriteUInt24BigEndian(this.NextSectorPosition);
            dataWriter.Write((byte)this.CacheIndex);
            dataWriter.Write(this.Data);

            return dataStream.ToArray();
        }

        /// <summary>
        /// Returns the given data converted to sectors.
        /// Sectors will not have NextSectorId set.
        /// This is up to the <see cref="FileStore"/>.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Sector> FromData(byte[] data, CacheIndex cacheIndex, int fileId)
        {
            var isExtended = Sector.GetExtended(fileId);

            var remaining = data.Length;
            var chunkId = 0;
            while (remaining > 0)
            {
                var sector = new Sector
                {
                    ChunkId = chunkId++,
                    CacheIndex = cacheIndex,
                    FileId = fileId
                };

                var sectorDataLength = isExtended ? Sector.ExtendedDataLength : Sector.StandardDataLength;
                var dataLength = Math.Min(sectorDataLength, remaining);

                var sectorData = data.Skip(data.Length - remaining)
                    .Take(dataLength);

                // Fill sector
                if (dataLength < sectorDataLength)
                {
                    sectorData = sectorData.Concat(Enumerable.Repeat((byte)0, sectorDataLength - dataLength));
                }

                sector.Data = sectorData.ToArray();

                remaining -= dataLength;

                yield return sector;
            }
        }

        /// <summary>
        /// Returns whether the extended format should be used for data with the given file id.
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        private static bool GetExtended(int fileId)
        {
            return fileId > 65535;
        }
    }
}
