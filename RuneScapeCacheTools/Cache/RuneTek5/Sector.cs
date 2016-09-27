using System;
using System.IO;

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
        ///     The size of the header within a sector in bytes.
        /// </summary>
        public const int HeaderLength = 8;

        /// <summary>
        ///     The size of the data within a sector in bytes.
        /// </summary>
        public const int DataLength = 512;

        /// <summary>
        ///     The extended data size
        /// </summary>
        public const int ExtendedDataLength = 510;

        /// <summary>
        ///     The extended header size
        /// </summary>
        public const int ExtendedHeaderLength = 10;

        /// <summary>
        ///     The total size of a sector in bytes.
        /// </summary>
        public const int Length = HeaderLength + DataLength;

        /// <summary>
        ///     Decodes the given byte array into a <see cref="Sector" /> object.
        /// </summary>
        /// <param name="expectedIndexId"></param>
        /// <param name="expectedFileId"></param>
        /// <param name="expectedChunkId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Sector(int expectedIndexId, int expectedFileId, int expectedChunkId, byte[] data)
        {
            if (data.Length != Length)
            {
                throw new ArgumentException(
                    $"Sector data must be exactly {Length} bytes in length, {data.Length} given.");
            }

            // Extended sectors use 4 bytes instead of 2 for the file id (and have 2 bytes less to use for the data)
            var extended = expectedFileId > 65535;

            var dataReader = new BinaryReader(new MemoryStream(data));

            // Obtain and verify if the chunk contains what we expect
            FileId = extended ? dataReader.ReadInt32BigEndian() : dataReader.ReadUInt16BigEndian();

            if (FileId != expectedFileId)
            {
                throw new SectorException("File id mismatch.");
            }

            ChunkId = dataReader.ReadUInt16BigEndian();

            if (ChunkId != expectedChunkId)
            {
                throw new SectorException("Chunk id mismatch.");
            }

            NextSectorId = dataReader.ReadUInt24BigEndian();
            IndexId = dataReader.ReadByte();

            if (IndexId != expectedIndexId)
            {
                throw new SectorException("Index id mismatch.");
            }

            Data = dataReader.ReadBytes(extended ? ExtendedDataLength : DataLength);
        }

        /// <summary>
        ///     The type of file this sector contains.
        /// </summary>
        public int IndexId { get; }

        /// <summary>
        ///     The id of the file this sector contains.
        /// </summary>
        public int FileId { get; }

        /// <summary>
        ///     The chunk within the file that this sector contains.
        /// </summary>
        public int ChunkId { get; }

        /// <summary>
        ///     The position of next sector.
        /// </summary>
        public int NextSectorId { get; }

        /// <summary>
        ///     The data in this sector.
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        ///     Encodes this <see cref="Sector" /> into a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] Encode()
        {
            var dataStream = new MemoryStream(new byte[Length]);
            var dataWriter = new BinaryWriter(dataStream);

            var extended = FileId > 65535;

            if (extended)
            {
                dataWriter.WriteInt32BigEndian(FileId);
            }
            else
            {
                dataWriter.WriteUInt16BigEndian((ushort) FileId);
            }

            dataWriter.WriteUInt16BigEndian((ushort) ChunkId);
            dataWriter.WriteUInt24BigEndian(NextSectorId);
            dataWriter.Write((byte) IndexId);
            dataWriter.Write(Data);

            return dataStream.ToArray();
        }
    }
}