using System;
using System.Collections.Generic;
using System.IO;

namespace Villermen.RuneScapeCacheTools.FileProcessors
{
	public class EnumFileProcessor
	{
		private readonly string _filePath;
		private IDictionary<int, EnumMetadata> _validEnumMetadata;

		public EnumFileProcessor(string filePath)
		{
			_filePath = filePath;
		}

		/// <summary>
		///   Returns a dictionary containing all existing enum ids and their relevant metadata.
		/// </summary>
		/// <returns></returns>
		public IDictionary<int, EnumMetadata> GetMetadata()
		{
			if (_validEnumMetadata != null)
			{
				return _validEnumMetadata;
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
				var enumMetadata = new Dictionary<int, EnumMetadata>();
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
						var valueTypeIdentifier = reader.ReadUInt16BigEndian();

						enumMetadata.Add(enumIndex, new EnumMetadata(enumPosition, (EnumValueType) valueTypeIdentifier));
					}

					reader.BaseStream.Position = returnFilePosition;

					enumIndex++;
				}

				return _validEnumMetadata = enumMetadata;
			}
		}

		public EnumMetadata GetMetadata(int enumId)
		{
			var enumMetadata = GetMetadata();

			if (!enumMetadata.ContainsKey(enumId))
			{
				throw new ArgumentException($"{enumId} is not an existing enum id.", nameof(enumId));
			}

			return enumMetadata[enumId];
		}

		public IDictionary<ushort, object> GetEnum(int enumId)
		{
			// TODO: Differentiate between types, obtain types in indexer?

			var metadata = GetMetadata(enumId);

			using (var reader = new BinaryReader(File.OpenRead(_filePath)))
			{
				// Move to start of enum
				reader.BaseStream.Position = metadata.FilePosition;

				// Read past data that is not relevant for this method (named for explanation)
				var identifier = reader.ReadBytes(3);
				var typeIdentifierValue = reader.ReadUInt16BigEndian();
				var nextEntryId = reader.ReadUInt16BigEndian();

				var entryCount = reader.ReadUInt16BigEndian();

				var enumData = new Dictionary<ushort, object>();

				for (ushort i = 0; i < entryCount; i++)
				{
					var entryId = reader.ReadUInt16BigEndian();
					object entryValue;

					switch (metadata.ValueType)
					{
						case EnumValueType.String:
							entryValue = reader.ReadNullTerminatedString();
							break;

						case EnumValueType.Int:
							entryValue = reader.ReadUInt32BigEndian();
							break;

						default:
							throw new EnumParseException($"No parser is defined for enum's value type \"{metadata.ValueType}\".");
					}

					enumData.Add(entryId, entryValue);
				}

				return enumData;
			}
		}
	}
}