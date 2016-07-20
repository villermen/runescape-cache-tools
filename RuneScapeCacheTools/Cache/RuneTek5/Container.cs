using System;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    /// A <see cref="Container"/> holds an optionally compressed file.
    /// This class can be used to decompress and compress containers.
    /// A container can also have a two byte trailer which specifies the version of the file within it.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    public class Container
    {
        // TODO: Describe what the key is used for

        public enum CompressionType
        {
            None = 0,
            Bzip2 = 1,
            Gzip = 2,
            LZMA = 3
        }

        public CompressionType Type { get; set; }

        /// <summary>
        /// The decompressed data.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// The version of the file within this container.
        /// </summary>
        public int Version { get; set; }

        private int[] NullKey { get; } = new int[4];

        /// <summary>
        /// Creates a new unversioned container.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        public Container(CompressionType type, byte[] data) : this(type, data, -1)
        {
        }

        /// <summary>
        /// Creates a new versioned container.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        public Container(CompressionType type, byte[] data, int version)
        {
            Type = type;
            Data = data;
            Version = version;
        }

        /// <summary>
        /// Decodes and decompressed the container.
        /// </summary>
        /// <param name="data"></param>
        public Container(byte[] data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Decodes and decompresses the container.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        public Container(byte[] data, int[] key)
        {
            throw new NotImplementedException();
        }

        public byte[] Encode()
        {
            throw new NotImplementedException();
        }
    }
}