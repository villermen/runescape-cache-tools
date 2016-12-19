using System.IO;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    /// An <see cref="IndexPointer" /> points to a file inside a <see cref="FileStore" />.
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

        /// <summary>
        ///     The number of the first sector that contains the file.
        /// </summary>
        public int FirstSectorPosition { get; set; }

        /// <summary>
        ///     The size of the file in bytes.
        /// </summary>
        public int Filesize { get; set; }

        /// <summary>
        /// Decodes an <see cref="IndexPointer"/> object from the given stream and advances the stream's position by 6 bytes.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IndexPointer Decode(Stream stream)
        {
            var reader = new BinaryReader(stream);

            return new IndexPointer
            {
                Filesize = reader.ReadUInt24BigEndian(),
                FirstSectorPosition = reader.ReadUInt24BigEndian()
            };
        }

        /// <summary>
        /// Encodes the <see cref="IndexPointer"/> to the given stream, advancing its position by 6 bytes.
        /// </summary>
        /// <param name="stream"></param>
        public void Encode(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.WriteUInt24BigEndian(this.Filesize);
            writer.WriteUInt24BigEndian(this.FirstSectorPosition);
        }
    }
}