using System.Collections.Generic;
using System.IO;

namespace Villermen.RuneScapeCacheTools.FileProcessors
{
	public class EnumFileProcessor
	{
		private IDictionary<int, uint> _validEnumPositions;
		private readonly string _filePath;

		public EnumFileProcessor(string filePath)
		{
			_filePath = filePath;
		}

		/// <summary>
		/// Returns a dictionary containing all valid enum id's and their starting positions within the file.
		/// </summary>
		/// <returns></returns>
		private IDictionary<int, uint> GetValidEnumPositions()
		{
			if (_validEnumPositions != null)
			{
				return _validEnumPositions;
			}

			using (var reader = new BinaryReader(File.OpenRead(_filePath)))
			{
				// First byte should be 1, for reasons unknown
				if (reader.ReadByte() != 1)
				{
					throw new InvalidDataException("Enum file does not start with 0x01.");
				}

				// Create a list of enum start positions
				var fileSize = reader.BaseStream.Length;
				var enumPositions = new Dictionary<int, uint>();
				var enumIndex = 0;

				while (true)
				{
					var enumPosition = reader.ReadUInt32BigEndian();

					// Last enum position (seems to) always points to the file's size, so we can use that to determine where the positions end
					if (enumPosition >= fileSize)
					{
						break;
					}

					// Only add the position if it is pointing to a valid enum
					var returnFilePosition = reader.BaseStream.Position;
					reader.BaseStream.Position = enumPosition;

					// Check validity of enum position by verifying that it starts with e(something)f
					var enumBytes = reader.ReadBytes(3);
					if (enumBytes.Length == 3 && enumBytes[0] == 0x65 && enumBytes[2] == 0x66)
					{
						enumPositions.Add(enumIndex, enumPosition);
					}

					reader.BaseStream.Position = returnFilePosition;

					enumIndex++;
				}

				return _validEnumPositions = enumPositions;
			}
		}
	}
}
