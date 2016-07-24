using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ICSharpCode.SharpZipLib.BZip2;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

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

        private static readonly uint[] NullKey = new uint[4];

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
        public Container(byte[] data) : this(data, NullKey)
        {
        }

        /// <summary>
        /// Decodes and decompresses the container.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        public Container(byte[] data, uint[] key)
        {
            var dataReader = new BinaryReader(new MemoryStream(data));

            Type = (CompressionType) dataReader.ReadByte();
            var length = dataReader.ReadInt32BigEndian();

            // Decrypt the data
            if (key.Sum(value => value) != 0)
            {
                var byteKeyStream = new MemoryStream(16);
                var byteKeyWriter = new BinaryWriter(byteKeyStream);
                foreach (var keyValue in key)
                {
                    byteKeyWriter.WriteUInt32BigEndian(keyValue);
                }

                var xtea = new XteaEngine();
                xtea.Init(false, new KeyParameter(byteKeyStream.ToArray()));
                var decrypted = new byte[length + (Type == CompressionType.None ? 5 : 9)];
                xtea.ProcessBlock(data, 5, decrypted, 0);

                data = decrypted;
            }

            // Check if we should decompress the data or not
            if (Type == CompressionType.None)
            {
                Data = dataReader.ReadBytes(length);
            }
            else
            {
                // Decompress the data
                var uncompressedLength = dataReader.ReadInt32BigEndian();
                var compressedBytes = dataReader.ReadBytes(length);
                var uncompressedBytes = new byte[uncompressedLength];

                switch (Type)
                {
                    case CompressionType.Bzip2:
                        // Add the bzip2 header as it is missing from the cache for whatever reason
                        var bzipCompressedBytes = new byte[compressedBytes.Length + 4];
                        bzipCompressedBytes[0] = (byte) 'B';
                        bzipCompressedBytes[1] = (byte) 'Z';
                        bzipCompressedBytes[2] = (byte) 'h';
                        bzipCompressedBytes[3] = (byte) '1';
                        Array.Copy(compressedBytes, 0, bzipCompressedBytes, 4, compressedBytes.Length);
                        var bzip2Stream = new BZip2InputStream(new MemoryStream(bzipCompressedBytes));
                        var readBzipBytes = bzip2Stream.Read(uncompressedBytes, 0, uncompressedLength);

                        if (readBzipBytes != uncompressedLength)
                        {
                            throw new CacheException("Uncompressed container data length does not match obtained length.");
                        }
                        break;

                    case CompressionType.Gzip:
                        var gzipStream = new GZipStream(new MemoryStream(compressedBytes), CompressionMode.Decompress);
                        var readGzipBytes = gzipStream.Read(uncompressedBytes, 0, uncompressedLength);

                        if (readGzipBytes != uncompressedLength)
                        {
                            throw new CacheException("Uncompressed container data length does not match obtained length.");
                        }
                        break;

                    case CompressionType.LZMA:
                        // TODO: Needs other library
                        throw new NotImplementedException();
                        break;

                    default:
                        throw new CacheException("Invalid compression type given.");
                }

                data = uncompressedBytes;
            }

            // Obtain the version if present
            Version = -1;
            if (dataReader.BaseStream.Length - dataReader.BaseStream.Position - 1 >= 2)
            {
                Version = dataReader.ReadInt16BigEndian();
            }

            Data = data;
        }

        public byte[] Encode()
        {
            throw new NotImplementedException();
        }
    }
}