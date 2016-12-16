using System.Collections.Generic;
using System.IO;

namespace Villermen.RuneScapeCacheTools.Extensions
{
    public static class BinaryStreamExtensions
    {
        // TODO: Change methods to WriteBigEndian(type)? 24 still has to be explicitly specified no matter what

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
        ///     Reads characters based on the current stream text encoding into a string until \0 or EOF occurs.
        /// </summary>
        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            var chars = new List<char>();

            try
            {
                while (true)
                {
                    var readChar = reader.ReadChar();

                    if (readChar == 0)
                    {
                        break;
                    }

                    chars.Add(readChar);
                }
            }
            catch (EndOfStreamException)
            {
            }

            return new string(chars.ToArray());
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

        /// <summary>
        ///     Writes characters into a string and suffixes it with \0.
        /// </summary>
        public static void WriteNullTerminatedString(this BinaryWriter writer, string str)
        {
            var chars = str.ToCharArray();

            writer.Write(chars);
            writer.Write((byte)0);
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

        private static readonly char[] awkwardCharacters =
        {
            '\u20AC', '\0', '\u201A', '\u0192', '\u201E', '\u2026', '\u2020',
            '\u2021', '\u02C6', '\u2030', '\u0160', '\u2039', '\u0152', '\0',
            '\u017D', '\0', '\0', '\u2018', '\u2019', '\u201C', '\u201D',
            '\u2022', '\u2013', '\u2014', '\u02DC', '\u2122', '\u0161',
            '\u203A', '\u0153', '\0', '\u017E', '\u0178'
        };

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

            value = (byte)BinaryStreamExtensions.awkwardCharacters[value - 128];

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
                return ((firstByte << 24) + (reader.ReadByte() << 16) + (reader.ReadByte() << 8) + reader.ReadByte()) &
                       0x7fffffff;
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
            var firstByte = reader.ReadSByte();

            if (firstByte < 0)
            {
                return firstByte;
            }

            // TODO: This doesn't seem right, write tests for it once this is certain
            return (short)((firstByte << 8) + reader.ReadByte() - short.MinValue);
        }

        public static void WriteAwkwardInt(this BinaryWriter writer, int value)
        {
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