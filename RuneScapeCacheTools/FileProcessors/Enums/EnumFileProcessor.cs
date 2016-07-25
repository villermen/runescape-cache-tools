using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Villermen.RuneScapeCacheTools.FileProcessors.Enums
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

					var metadata = EnumMetadata.FromStream(reader.BaseStream);

					if (metadata != null)
					{
						enumMetadata.Add(enumIndex, metadata);
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

		public IDictionary<int, ILookup<object, object>> GetEnums()
		{
			return GetMetadata().Keys.ToDictionary(enumId => enumId, GetEnum);
		}

		public ILookup<object, object> GetEnum(int enumId)
		{
			var metadata = GetMetadata(enumId);

			using (var reader = new BinaryReader(File.OpenRead(_filePath), Encoding.ASCII))
			{
				// Move to start of enum data
				reader.BaseStream.Position = metadata.DataFilePosition;

				// Initialize the resulting dataset
				// No dictionary, because there can be duplicate keys
				var enumData = new List<Tuple<object, object>>();

				for (var i = 0; i < metadata.Count; i++)
				{
					object entryKey;
					switch (metadata.KeyType)
					{
						case EnumKeyType.UShort:
							entryKey = reader.ReadUInt16BigEndian();
							break;

						case EnumKeyType.UInt1:
						case EnumKeyType.UInt2:
							entryKey = reader.ReadUInt32BigEndian();
							break;

						default:
							throw new UnregisteredEnumTypeException($"No parser is defined for enum's key type \"{metadata.KeyType}\".");
					}

					object entryValue;
					switch (metadata.ValueType)
					{
						case EnumValueType.UInt1:
						case EnumValueType.UInt2:
						case EnumValueType.UInt3:
						case EnumValueType.UInt4:
						case EnumValueType.UInt5:
						case EnumValueType.UInt6:
							entryValue = reader.ReadUInt32BigEndian();
							break;

						case EnumValueType.NullTerminatedString:
							entryValue = reader.ReadNullTerminatedString();
							break;

						case EnumValueType.UShort:
							entryValue = reader.ReadUInt16BigEndian();
							break;

						default:
							throw new UnregisteredEnumTypeException($"No parser is defined for enum's value type \"{metadata.ValueType}\".");
					}

					enumData.Add(new Tuple<object, object>(entryKey, entryValue));
				}

				return enumData.ToLookup(tuple => tuple.Item1, tuple => tuple.Item2);
			}
		}
	}
}