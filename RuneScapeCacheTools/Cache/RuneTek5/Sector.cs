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
        /// <param name="data"></param>
        /// <param name="extended"></param>
        /// <returns></returns>
        public Sector(byte[] data, bool extended = false)
        {
            if (data.Length != Length)
            {
                throw new ArgumentException(
                    $"Sector data must be exactly {Length} bytes in length, {data.Length} given.");
            }

            var reader = new BinaryReader(new MemoryStream(data));

            FileId = extended ? reader.ReadInt32BigEndian() : reader.ReadUInt16BigEndian();
            ChunkId = reader.ReadUInt16BigEndian();
            NextSectorId = (int) reader.ReadUInt24BigEndian();
            IndexId = reader.ReadByte();
            Data = reader.ReadBytes(extended ? ExtendedDataLength : DataLength);
            Extended = extended;
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

        public bool Extended { get; }

        /// <summary>
        ///     Encodes this <see cref="Sector" /> into a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] Encode()
        {
            var stream = new MemoryStream(new byte[Length]);
            var writer = new BinaryWriter(stream);

            if (Extended)
            {
                writer.WriteInt32BigEndian(FileId);
            }
            else
            {
                writer.WriteUInt16BigEndian((ushort) FileId);
            }

            writer.WriteUInt16BigEndian((ushort) ChunkId);
            writer.WriteUInt24BigEndian((uint) NextSectorId);
            writer.Write((byte) IndexId);
            writer.Write(Data);

            return stream.ToArray();
        }
    }
}