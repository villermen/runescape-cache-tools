using System.Collections.Generic;
using System.IO;

namespace Villermen.RuneScapeCacheTools
{
	public static class BinaryStreamExtensions
	{
		/// <summary>
		///   Reads a 2-byte unsigned big endian integer and advances the current position of the stream by 2 bytes.
		/// </summary>
		public static ushort ReadUInt16BigEndian(this BinaryReader reader)
		{
			return (ushort)((reader.ReadByte() << 8) + reader.ReadByte());
		}

		/// <summary>
		///   Reads a 3-byte unsigned big endian integer and advances the current position of the stream by 3 bytes.
		/// </summary>
		public static uint ReadUInt24BigEndian(this BinaryReader reader)
		{
			return (uint) ((reader.ReadByte() << 16) + (reader.ReadByte() << 8) + reader.ReadByte());
		}

		/// <summary>
		///   Reads a 4-byte unsigned big endian integer and advances the current position of the stream by 4 bytes.
		/// </summary>
		public static uint ReadUInt32BigEndian(this BinaryReader reader)
		{
			return (uint) ((reader.ReadByte() << 24) + (reader.ReadByte() << 16) + (reader.ReadByte() << 8) + reader.ReadByte());
		}

		/// <summary>
		///   Reads a 6-byte unsigned big endian integer and advances the current position of the stream by 6 bytes.
		/// </summary>
		public static ulong ReadUInt48BigEndian(this BinaryReader reader)
		{
			return (ulong)((reader.ReadByte() << 40) + (reader.ReadByte() << 32) + (reader.ReadByte() << 24) + (reader.ReadByte() << 16) + (reader.ReadByte() << 8) + reader.ReadByte());
		}

        /// <summary>
		///   Reads a 2-byte signed big endian integer and advances the current position of the stream by 2 bytes.
		/// </summary>
		public static short ReadInt16BigEndian(this BinaryReader reader)
        {
            return (short)((reader.ReadByte() << 8) + reader.ReadByte());
        }

        /// <summary>
		///   Reads a 4-byte signed big endian integer and advances the current position of the stream by 4 bytes.
		/// </summary>
		public static int ReadInt32BigEndian(this BinaryReader reader)
        {
            return (reader.ReadByte() << 24) + (reader.ReadByte() << 16) + (reader.ReadByte() << 8) + reader.ReadByte();
        }

        /// <summary>
        ///   Reads characters based on the current stream text encoding into a string until \0 or EOF occurs.
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

        public static void WriteUInt16BigEndian(this BinaryWriter writer, ushort value)
        {
            writer.Write((byte) (value >> 8));
            writer.Write((byte) value);
        }

        public static void WriteUInt24BigEndian(this BinaryWriter writer, uint value)
        {
            writer.Write((byte) (value >> 16));
            writer.Write((byte) (value >> 8));
            writer.Write((byte) value);
        }

        public static void WriteInt16BigEndian(this BinaryWriter writer, short value)
	    {
	        writer.Write((byte) (value >> 8));
            writer.Write((byte) value);
	    }

        public static void WriteInt32BigEndian(this BinaryWriter writer, int value)
        {
            writer.Write((byte) (value >> 24));
            writer.Write((byte) (value >> 16));
            writer.Write((byte) (value >> 8));
            writer.Write((byte) value);
        }

        /// <summary>
        /// Writes characters into a string and suffixes it with \0.
        /// </summary>
        public static void WriteNullTerminatedString(this BinaryWriter writer, string str)
	    {
	        var chars = str.ToCharArray();

	        writer.Write(chars);
            writer.Write((byte) 0);
	    }

        #region RuneScape cache specific extensions

	    public static int ReadSmartInt(this BinaryReader reader)
	    {
	        var readByte = reader.ReadSByte();
	        if (readByte < 0)
	        {
	            return ((readByte << 24) + (reader.ReadByte() << 16) + (reader.ReadByte() << 8) + reader.ReadByte()) & 0x7fffffff;
	        }

	        var f = ((readByte << 8) + reader.ReadByte()) & 0xffff;
            return f == 32767 ? -1 : f;
	    }


	    public static void WriteSmartInt(this BinaryWriter writer, int value)
	    {
	        if ((value & 0xffff) < 32768)
		        writer.WriteInt16BigEndian((short) value);
	        else
		        writer.WriteInt32BigEndian((int) (value | 0x80000000));
        }

        #endregion
    }
}
