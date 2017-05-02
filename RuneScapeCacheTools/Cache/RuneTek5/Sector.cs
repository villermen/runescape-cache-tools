using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Exceptions;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    ///     Represents a sector in the data file, containing some metadata and the actual data contained in the sector.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Villermen</author>
    public class Sector
    {
        /// <summary>
        ///     The total size of a sector in bytes.
        /// </summary>
        public const int Length = 520;

        /// <summary>
        ///     The size of the header within a sector in bytes.
        /// </summary>
        private const int standardHeaderLength = 8;

        /// <summary>
        ///     The size of the data within a sector in bytes.
        /// </summary>
        private const int standardDataLength = 512;

        /// <summary>
        ///     The extended data size
        /// </summary>
        private const int extendedDataLength = 510;

        /// <summary>
        ///     The extended header size
        /// </summary>
        private const int extendedHeaderLength = 10;

        public Sector()
        {
        }

        /// <summary>
        ///     Decodes the given byte array into a <see cref="Sector" /> object.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="expectedIndex"></param>
        /// <param name="expectedFileId"></param>
        /// <param name="expectedChunkId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Sector(int position, Index expectedIndex, int expectedFileId, int expectedChunkId, byte[] data)
        {
            this.Position = position;

            if (data.Length != Sector.Length)
            {
                throw new ArgumentException(
                    $"Sector data must be exactly {Sector.Length} bytes in length, {data.Length} given.");
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
            this.Index = (Index)dataReader.ReadByte();

            if (this.Index != expectedIndex)
            {
                throw new DecodeException($"Index id mismatch. Expected {expectedIndex}, got {this.Index}.");
            }

            this.Data = dataReader.ReadBytes(this.IsExtended ? Sector.extendedDataLength : Sector.standardDataLength);
        }

        public int Position { get; set; }

        /// <summary>
        ///     The chunk within the file that this sector contains.
        /// </summary>
        public int ChunkId { get; set; }

        /// <summary>
        ///     The data in this sector.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        ///     The id of the file this sector contains.
        /// </summary>
        public int FileId { get; private set; }

        /// <summary>
        ///     The type of file this sector contains.
        /// </summary>
        public Index Index { get; set; }

        /// <summary>
        ///     The position of next sector.
        /// </summary>
        public int NextSectorPosition { get; set; }

        /// <summary>
        /// Gets whether the sector uses the extended format.
        /// Extended sectors use 4 bytes instead of 2 for the file id (and have 2 bytes less to use for the data).
        /// Jagex did not expect file indexes to surpass the size of a short =)
        /// </summary>
        public bool IsExtended => Sector.GetExtended(this.FileId);

        /// <summary>
        ///     Encodes this <see cref="Sector" /> into a byte array.
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
            dataWriter.Write((byte)this.Index);
            dataWriter.Write(this.Data);

            return dataStream.ToArray();
        }

        /// <summary>
        /// Returns the given data converted to sectors.
        /// Sectors will not have NextSectorId set.
        /// This is up to the <see cref="FileStore"/>.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Sector> FromData(byte[] data, Index index, int fileId)
        {
            var isExtended = Sector.GetExtended(fileId);

            var remaining = data.Length;
            var chunkId = 0;
            while (remaining > 0)
            {
                var sector = new Sector
                {
                    ChunkId = chunkId++,
                    Index = index,
                    FileId = fileId
                };

                var sectorDataLength = isExtended ? Sector.extendedDataLength : Sector.standardDataLength;
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