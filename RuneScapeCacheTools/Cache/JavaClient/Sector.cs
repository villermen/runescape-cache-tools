using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.Cache.JavaClient
{
    /// <summary>
    /// Represents a sector in the data file, containing some metadata and the actual data contained in the sector.
    /// </summary>
    public class Sector
    {
        /// <summary>
        /// The total size of a sector in bytes.
        /// </summary>
        public const int Size = 520;

        /// <summary>
        /// The size of the header within a sector in bytes.
        /// </summary>
        private const int StandardHeaderLength = 8;

        /// <summary>
        /// The size of the data within a sector in bytes.
        /// </summary>
        private const int StandardDataLength = (Sector.Size - Sector.StandardHeaderLength);

        /// <summary>
        /// The extended header size.
        /// </summary>
        private const int ExtendedHeaderLength = 10;

        /// <summary>
        /// The extended data size.
        /// </summary>
        private const int ExtendedDataLength = (Sector.Size - Sector.ExtendedHeaderLength);

        /// <summary>
        /// Number of this <see cref="Sector" /> in the data file. Multiply by <see cref="Size" /> to get byte position
        /// in the file.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// The position of next sector.
        /// </summary>
        public int? NextSectorPosition { get; set; }

        /// <summary>
        /// The chunk within the file that this sector contains.
        /// </summary>
        public int ChunkIndex { get; private set; }

        /// <summary>
        /// The actual data in this sector.
        /// </summary>
        public byte[] Payload { get; private set; }

        /// <summary>
        /// The ID of the file this sector contains data of.
        /// </summary>
        public int FileId { get; private set; }

        /// <summary>
        /// The index of the file this sector contains data of.
        /// </summary>
        public CacheIndex Index { get; private set; }

        /// <summary>
        /// Returns whether this sector uses the extended metadata format. Extended sectors use 4 bytes instead of 2 for
        /// the file ID (and are left with 2 bytes less to use for the data). Jagex did not expect file indexes to
        /// surpass the size of a short =)
        /// </summary>
        public bool Extended => Sector.IsExtendedSector(this.FileId);

        /// <summary>
        /// Decodes the sector from the given data and verifies that the sector's metadata matches the expected
        /// parameters.
        /// </summary>
        public static Sector Decode(int position, byte[] data, CacheIndex expectedIndex, int expectedFileId, int expectedChunkIndex)
        {
            if (data.Length != Sector.Size)
            {
                throw new ArgumentException(
                    $"Sector data must be exactly {Sector.Size} bytes in length, {data.Length} given."
                );
            }

            var dataReader = new BinaryReader(new MemoryStream(data));

            // Verify expected file ID
            var extended = Sector.IsExtendedSector(expectedFileId);
            var actualFileId = (extended ? dataReader.ReadInt32BigEndian() : dataReader.ReadUInt16BigEndian());
            if (actualFileId != expectedFileId)
            {
                throw new DecodeException($"Expected sector for file {expectedFileId}, got {actualFileId}.");
            }

            // Verify expected chunk ID
            var actualChunkIndex = dataReader.ReadUInt16BigEndian();
            if (actualChunkIndex != expectedChunkIndex)
            {
                throw new DecodeException($"Expected sector for file chunk {expectedChunkIndex}, got {actualChunkIndex}.");
            }

            var nextSectorPosition = dataReader.ReadUInt24BigEndian();

            var actualIndex = (CacheIndex)dataReader.ReadByte();
            if (actualIndex != expectedIndex)
            {
                throw new DecodeException($"Expected sector for index {(int)expectedIndex}, got {(int)actualIndex}.");
            }

            var payload = dataReader.ReadBytesExactly(extended ? Sector.ExtendedDataLength : Sector.StandardDataLength);

            return new Sector
            {
                Position = position,
                FileId = actualFileId,
                Index = actualIndex,
                ChunkIndex = actualChunkIndex,
                NextSectorPosition = nextSectorPosition,
                Payload = payload,
            };
        }

        /// <summary>
        /// Returns the given data converted to sectors. <see cref="Position" /> and <see cref="NextSectorPosition" />
        /// must still be set before writing them out to the data file.
        /// </summary>
        public static IEnumerable<Sector> FromData(byte[] data, CacheIndex cacheIndex, int fileId)
        {
            var extended = Sector.IsExtendedSector(fileId);

            var remaining = data.Length;
            var chunkId = 0;
            while (remaining > 0)
            {
                var sector = new Sector
                {
                    ChunkIndex = chunkId++,
                    Index = cacheIndex,
                    FileId = fileId
                };

                var sectorDataLength = extended ? Sector.ExtendedDataLength : Sector.StandardDataLength;
                var dataLength = Math.Min(sectorDataLength, remaining);
                var sectorData = data.Skip(data.Length - remaining).Take(dataLength);

                // Fill sector
                if (dataLength < sectorDataLength)
                {
                    sectorData = sectorData.Concat(Enumerable.Repeat((byte)0, sectorDataLength - dataLength));
                }

                sector.Payload = sectorData.ToArray();

                remaining -= dataLength;

                yield return sector;
            }
        }

        private static bool IsExtendedSector(int fileId)
        {
            return (fileId > 65535);
        }

        /// <summary>
        /// Encodes this <see cref="Sector" /> into a byte array.
        /// </summary>
        public byte[] Encode()
        {
            using var dataStream = new MemoryStream(new byte[Sector.Size]);
            using var dataWriter = new BinaryWriter(dataStream);

            if (this.Extended)
            {
                dataWriter.WriteInt32BigEndian(this.FileId);
            }
            else
            {
                dataWriter.WriteUInt16BigEndian((ushort)this.FileId);
            }

            dataWriter.WriteUInt16BigEndian((ushort)this.ChunkIndex);

            if (!this.NextSectorPosition.HasValue)
            {
                throw new EncodeException("Sector's next sector position must be set before encoding.");
            }

            dataWriter.WriteUInt24BigEndian(this.NextSectorPosition.Value);
            dataWriter.Write((byte)this.Index);
            dataWriter.Write(this.Payload);

            return dataStream.ToArray();
        }
    }
}
