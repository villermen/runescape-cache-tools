using System;
using System.IO;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    ///     An <see cref="IndexPointer" /> points to a file inside a <see cref="FileStore" />.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Villermen</author>
    public class IndexPointer
    {
        /// <summary>
        ///     Length of index data in bytes.
        /// </summary>
        public const int Length = 6;

        public IndexPointer(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            this.Filesize = reader.ReadUInt24BigEndian();
            this.FirstSectorPosition = reader.ReadUInt24BigEndian();
        }

        public IndexPointer(int firstSectorPosition, int filesize)
        {
            this.FirstSectorPosition = firstSectorPosition;
            this.Filesize = filesize;
        }

        // TODO: Extension method to BinaryWriter?
        public void Encode(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.WriteUInt24BigEndian(this.Filesize);
            writer.WriteUInt24BigEndian(this.FirstSectorPosition);
        }

        /// <summary>
        ///     The number of the first sector that contains the file.
        /// </summary>
        public int FirstSectorPosition { get; private set; }

        /// <summary>
        ///     The size of the file in bytes.
        /// </summary>
        public int Filesize { get; private set; }
    }
}