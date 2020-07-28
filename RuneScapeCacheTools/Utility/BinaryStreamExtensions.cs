using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Villermen.RuneScapeCacheTools.Utility
{
    public static class BinaryStreamExtensions
    {
        /// <summary>
        ///     Reads a 2-byte signed big endian integer and advances the current position of the stream by 2 bytes.
        /// </summary>
        public static short ReadInt16BigEndian(this BinaryReader reader)
        {
            return (short)((reader.ReadByte() << 8) + reader.ReadByte());
        }

        /// <summary>
        ///     Reads a 4-byte signed big endian integer and advances the current position of the stream by 4 bytes.
        /// </summary>
        public static int ReadInt32BigEndian(this BinaryReader reader)
        {
            return (reader.ReadByte() << 24) + (reader.ReadByte() << 16) + (reader.ReadByte() << 8) + reader.ReadByte();
        }

        /// <summary>
        ///     Reads a 2-byte unsigned big endian integer and advances the current position of the stream by 2 bytes.
        /// </summary>
        public static ushort ReadUInt16BigEndian(this BinaryReader reader)
        {
            return (ushort)((reader.ReadByte() << 8) + reader.ReadByte());
        }

        /// <summary>
        ///     Reads a 3-byte unsigned big endian integer and advances the current position of the stream by 3 bytes.
        /// </summary>
        public static int ReadUInt24BigEndian(this BinaryReader reader)
        {
            return (reader.ReadByte() << 16) + (reader.ReadByte() << 8) + reader.ReadByte();
        }

        /// <summary>
        ///     Reads a 4-byte unsigned big endian integer and advances the current position of the stream by 4 bytes.
        /// </summary>
        public static uint ReadUInt32BigEndian(this BinaryReader reader)
        {
            return (uint)((reader.ReadByte() << 24) + (reader.ReadByte() << 16) +
                (reader.ReadByte() << 8) + reader.ReadByte());
        }

        /// <summary>
        ///     Reads a 6-byte unsigned big endian integer and advances the current position of the stream by 6 bytes.
        /// </summary>
        public static long ReadUInt48BigEndian(this BinaryReader reader)
        {
            return ((long)reader.ReadByte() << 40) + ((long)reader.ReadByte() << 32) + (reader.ReadByte() << 24) +
                (reader.ReadByte() << 16) + (reader.ReadByte() << 8) + reader.ReadByte();
        }

        /// <summary>
        /// Reads <paramref name="count" /> bytes from the current stream into a byte array and advances the current
        /// position by that number of bytes. If the stream ends before the specified amount of bytes can be read, a
        /// <see cref="EndOfStreamException" /> will be thrown. Identical to <see cref="BinaryReader.ReadBytes" />
        /// otherwise.
        /// </summary>
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.IO.IOException" />
        /// <exception cref="System.ObjectDisposedException" />
        /// <exception cref="System.ArgumentOutOfRangeException" />
        /// <exception cref="EndOfStreamException" />
        /// <returns></returns>
        public static byte[] ReadBytesExactly(this BinaryReader reader, int count)
        {
            var bytes = reader.ReadBytes(count);
            if (bytes.Length != count)
            {
                throw new EndOfStreamException(
                    $"The requested amount of bytes ({count}) could not be read. Only {bytes.Length} bytes were available."
                );
            }

            return bytes;
        }

        public static void WriteInt16BigEndian(this BinaryWriter writer, short value)
        {
            writer.Write((byte)(value >> 8));
            writer.Write((byte)value);
        }

        public static void WriteInt32BigEndian(this BinaryWriter writer, int value)
        {
            writer.Write((byte)(value >> 24));
            writer.Write((byte)(value >> 16));
            writer.Write((byte)(value >> 8));
            writer.Write((byte)value);
        }

        public static void WriteUInt16BigEndian(this BinaryWriter writer, ushort value)
        {
            writer.Write((byte)(value >> 8));
            writer.Write((byte)value);
        }

        public static void WriteUInt24BigEndian(this BinaryWriter writer, int value)
        {
            writer.Write((byte)(value >> 16));
            writer.Write((byte)(value >> 8));
            writer.Write((byte)value);
        }

        public static void WriteUInt32BigEndian(this BinaryWriter writer, uint value)
        {
            writer.Write((byte)(value >> 24));
            writer.Write((byte)(value >> 16));
            writer.Write((byte)(value >> 8));
            writer.Write((byte)value);
        }

        #region Jagex specific (and specific they are)

        private static readonly Encoding Charset = Encoding.GetEncoding("iso-8859-1");

        private static readonly char[] AwkwardCharacters =
        {
            '\u20AC', '\0', '\u201A', '\u0192', '\u201E', '\u2026', '\u2020',
            '\u2021', '\u02C6', '\u2030', '\u0160', '\u2039', '\u0152', '\0',
            '\u017D', '\0', '\0', '\u2018', '\u2019', '\u201C', '\u201D',
            '\u2022', '\u2013', '\u2014', '\u02DC', '\u2122', '\u0161',
            '\u203A', '\u0153', '\0', '\u017E', '\u0178'
        };

        /// <summary>
        ///     Reads characters based on the current stream text encoding into a string until \0 or EOF occurs.
        /// </summary>
        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            var bytes = new List<byte>();

            try
            {
                while (true)
                {
                    var readByte = reader.ReadByte();

                    if (readByte == 0)
                    {
                        break;
                    }

                    bytes.Add(readByte);
                }
            }
            catch (EndOfStreamException)
            {
            }

            return BinaryStreamExtensions.Charset.GetString(bytes.ToArray());
        }

        /// <summary>
        ///     Reads a byte, and turns it into a char using some awkward ruleset Jagex came up with.
        ///     I mean...
        ///     It could've just been a regular char.
        ///     But no, that would've been too normal for Jagex.
        /// </summary>
        /// <param name="reader"></param>
        public static char ReadAwkwardChar(this BinaryReader reader)
        {
            var value = reader.ReadByte();
            if (value == 0)
            {
                throw new IOException("Non cp1252 character provided, 0x00 given.");
            }

            if ((value < 128) || (value >= 160))
            {
                return (char)value;
            }

            value = (byte)BinaryStreamExtensions.AwkwardCharacters[value - 128];

            if (value == 0)
            {
                value = 63;
            }

            return (char)value;
        }

        /// <summary>
        /// Reads either an unsigned int or an unsigned short depending on the first byte read.
        /// Oh if the short is its maximum value -1 is returned. Wat?
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static int ReadAwkwardInt(this BinaryReader reader)
        {
            var firstByte = reader.ReadSByte();
            if (firstByte < 0)
            {
                return ((firstByte << 24) + (reader.ReadByte() << 16) + (reader.ReadByte() << 8) + reader.ReadByte()) & 0x7fffffff;
            }

            var f = (firstByte << 8) + reader.ReadByte();
            return f == short.MaxValue ? -1 : f;
        }

        /// <summary>
        /// Reads either a byte or an unsigned short depending on the first byte read.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static short ReadAwkwardShort(this BinaryReader reader)
        {
            var firstByte = reader.ReadByte();

            if (firstByte < 128)
            {
                return firstByte;
            }

            return (short)((firstByte << 8) + reader.ReadByte() - short.MinValue);
        }

        /// <summary>
        ///     Writes characters into a string and suffixes it with \0.
        /// </summary>
        public static void WriteNullTerminatedString(this BinaryWriter writer, string str)
        {
            writer.Write(BinaryStreamExtensions.Charset.GetBytes(str));
            writer.Write((byte)0);
        }

        public static void WriteAwkwardInt(this BinaryWriter writer, int value)
        {
            if (value < -1)
            {
                throw new ArgumentException("Awkward int can not be less than -1.");
            }

            if (value == -1)
            {
                writer.WriteInt16BigEndian(short.MaxValue);
            }
            else if (value < short.MaxValue)
            {
                writer.WriteInt16BigEndian((short)value);
            }
            else
            {
                writer.WriteInt32BigEndian((int)(value | 0x80000000));
            }
        }

#endregion

    }
}
