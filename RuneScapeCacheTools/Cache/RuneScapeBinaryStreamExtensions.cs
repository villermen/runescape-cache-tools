using System.IO;

namespace Villermen.RuneScapeCacheTools.Cache
{
	public static class RuneScapeBinaryStreamExtensions
	{
		private static readonly char[] AwkwardCharacters =
		{
			'\u20AC', '\0', '\u201A', '\u0192', '\u201E', '\u2026', '\u2020',
			'\u2021', '\u02C6', '\u2030', '\u0160', '\u2039', '\u0152', '\0',
			'\u017D', '\0', '\0', '\u2018', '\u2019', '\u201C', '\u201D',
			'\u2022', '\u2013', '\u2014', '\u02DC', '\u2122', '\u0161',
			'\u203A', '\u0153', '\0', '\u017E', '\u0178'
		};

		public static short ReadSmartShort(this BinaryReader reader)
		{
			var firstByte = reader.ReadByte();

			if (firstByte < 128)
			{
				return firstByte;
			}

			return (short) ((firstByte << 8) + reader.ReadByte() - 32768);
		}

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

		/// <summary>
		///   Reads a byte, and turns it into a char using some awkward ruleset Jagex came up with.
		///   I mean...
		///   It could've just been a regular char.
		///   But no, that would've been too normal for Jagex.
		/// </summary>
		/// <param name="reader"></param>
		public static char ReadAwkwardChar(this BinaryReader reader)
		{
			var value = reader.ReadByte();
			if (value == 0)
			{
				throw new IOException("Non cp1252 character provided, 0x00 given.");
			}

			if (value < 128 || value >= 160)
			{
				return (char) value;
			}

			value = (byte) AwkwardCharacters[value - 128];

			if (value == 0)
			{
				value = 63;
			}

			return (char) value;
		}

		public static void WriteSmartInt(this BinaryWriter writer, int value)
		{
			if ((value & 0xffff) < 32768)
			{
				writer.WriteInt16BigEndian((short) value);
			}
			else
			{
				writer.WriteInt32BigEndian((int) (value | 0x80000000));
			}
		}
	}
}