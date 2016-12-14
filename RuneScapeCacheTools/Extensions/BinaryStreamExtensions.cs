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
            return
                (uint)
                ((reader.ReadByte() << 24) + (reader.ReadByte() << 16) + (reader.ReadByte() << 8) +
                 reader.ReadByte());
        }

        /// <summary>
        ///     Reads a 6-byte unsigned big endian integer and advances the current position of the stream by 6 bytes.
        /// </summary>
        public static long ReadUInt48BigEndian(this BinaryReader reader)
        {
            return
                (reader.ReadByte() << 40) + (reader.ReadByte() << 32) + (reader.ReadByte() << 24) +
                (reader.ReadByte() << 16) +
                (reader.ReadByte() << 8) + reader.ReadByte();
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
    }
}