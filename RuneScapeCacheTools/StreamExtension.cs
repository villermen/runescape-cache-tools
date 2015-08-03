using System;
using System.IO;
using System.Text;

namespace RuneScapeCacheTools
{
	public static class StreamExtension
	{
		/// <summary>
		/// Reads a given amount of unsigned bytes from the stream and combines them into one unsigned integer.
		/// </summary>
		public static uint ReadBytes(this Stream stream, int bytes)
		{
			if (bytes < 1 || bytes > 4)
				throw new ArgumentOutOfRangeException();

			uint result = 0;

			for (int i = 0; i < bytes; i++)
				result += (uint)stream.ReadByte() << (bytes - i - 1) * 8;

			return result;
		}

		/// <summary>
		/// Reads ANSI characters into a string until \0 or EOF occurs.
		/// </summary>
		public static string ReadNullTerminatedString(this Stream stream)
		{
			string result = "";
			int readByte = stream.ReadByte();

			while (readByte > 0)
			{
				result += Encoding.Default.GetString(new byte[] { (byte)readByte });
				readByte = stream.ReadByte();
			}

			return result;
		}

		/// <summary>
		/// Returns the stream location of the matchNumber-th occurence of needle, or -1 when there are no(t enough) matches.
		/// </summary>
		public static long IndexOf(this Stream stream, byte[] needle, int matchNumber = 1, int bufferSize = 10000)
		{
			//for resetting after method
			long startPosition = stream.Position;

			byte[] buffer = new byte[bufferSize];
			int offset = 0, readBytes, matches = 0;

			do
			{
				stream.Position = offset;
				readBytes = stream.Read(buffer, 0, bufferSize);

				for (int pos = 0; pos < readBytes - needle.Length + 1; pos++)
				{
					//try to find the rest of the match if the first byte matches
					int matchIndex = 0;
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
			}
			while (readBytes == bufferSize);

			//no result
			stream.Position = startPosition;
			return -1;
		}
	}
}
