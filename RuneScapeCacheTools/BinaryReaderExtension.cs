using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RuneScapeCacheTools
{
	public static class BinaryReaderExtension
	{
		/// <summary>
		/// Reads a 3-byte unsigned big endian integer and advances the current position of the stream by 3 bytes.
		/// </summary>
		public static uint ReadUInt24BigEndian(this BinaryReader reader)
		{
			return (uint) ((reader.ReadByte() << 16) + (reader.ReadByte() << 8) + reader.ReadByte());
		}

		/// <summary>
		/// Reads a 4-byte unsigned big endian integer and advances the current position of the stream by 4 bytes.
		/// </summary>
		public static uint ReadUInt32BigEndian(this BinaryReader reader)
		{
			return (uint) ((reader.ReadByte() << 24) + (reader.ReadByte() << 16) + (reader.ReadByte() << 8) + reader.ReadByte());
		}

		/// <summary>
		/// Reads ANSI characters into a string until \0 or EOF occurs.
		/// </summary>
		public static string ReadNullTerminatedString(this BinaryReader reader)
		{
			List<char> chars = new List<char>();

			while (true)
			{
				char readChar = reader.ReadChar();

				if (readChar == 0)
				{
					break;
				}

				chars.Add(readChar);
			}

			return new string(chars.ToArray());
		}

		/// <summary>
		/// Returns the stream location of the matchNumber-th occurence of needle, or -1 when there are no(t enough) matches.
		/// </summary>
		public static long IndexOf(this Stream stream, byte[] needle, int matchNumber = 1, int bufferSize = 10000)
		{
			//for resetting after method
			var startPosition = stream.Position;

			var buffer = new byte[bufferSize];
			int offset = 0, readBytes, matches = 0;

			do
			{
				stream.Position = offset;
				readBytes = stream.Read(buffer, 0, bufferSize);

				for (var pos = 0; pos < readBytes - needle.Length + 1; pos++)
				{
					//try to find the rest of the match if the first byte matches
					var matchIndex = 0;
					while (buffer[pos + matchIndex] == needle[matchIndex])
					{
						//full match found
						if (matchIndex == needle.Length - 1)
						{
							//this is the chosen one, return the position
							if (++matches == matchNumber)
							{
								stream.Position = 0;
								return offset + pos;
							}

							break;
						}

						matchIndex++;
					}
				}

				//don't fully add readBytes, so the next string can find the full match if it started on the end of this buffer but couldn't complete
				offset += readBytes - needle.Length + 1;
			} while (readBytes == bufferSize);

			//no result
			stream.Position = startPosition;
			return -1;
		}
	}
}
