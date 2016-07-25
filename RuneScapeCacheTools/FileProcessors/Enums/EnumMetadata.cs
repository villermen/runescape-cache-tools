using System.IO;

namespace Villermen.RuneScapeCacheTools.FileProcessors.Enums
{
	public class EnumMetadata
	{
		public EnumKeyType KeyType { get; private set; }

		public EnumValueType ValueType { get; private set; }

		public EnumMetadataType MetadataType { get; private set; }

		public ushort NextEntryId { get; private set; }

		public ushort Count { get; private set; }

		public ushort ValueThatIDoNotKnowTheDetailsOf { get; private set; }

		public uint FilePosition { get; private set; }

		public int MetadataLength { get; private set; }

		public uint DataFilePosition => (uint) (FilePosition + MetadataLength);

		public static EnumMetadata FromStream(Stream stream)
		{
			var metadata = new EnumMetadata();

			var reader = new BinaryReader(stream);

			metadata.FilePosition = (uint) stream.Position;

			try
			{
				// Verify the enum signature (e(keyType)f)
				if (reader.ReadByte() != 0x65)
				{
					return null;
				}

				metadata.KeyType = (EnumKeyType) reader.ReadByte();

				if (reader.ReadByte() != 0x66)
				{
					return null;
				}
			}
			catch (EndOfStreamException)
			{
				return null;
			}

			metadata.ValueType = (EnumValueType) reader.ReadByte();
			metadata.MetadataType = (EnumMetadataType) reader.ReadByte();

			switch (metadata.MetadataType)
			{
				case EnumMetadataType.Count1:
				case EnumMetadataType.Count2:
					metadata.Count = reader.ReadUInt16BigEndian();
					break;

				case EnumMetadataType.NextIdCount:
					metadata.NextEntryId = reader.ReadUInt16BigEndian();
					metadata.Count = reader.ReadUInt16BigEndian();
					break;

				case EnumMetadataType.NextIdCountUnknown:
					metadata.NextEntryId = reader.ReadUInt16BigEndian();
					metadata.Count = reader.ReadUInt16BigEndian();
					metadata.ValueThatIDoNotKnowTheDetailsOf = reader.ReadUInt16BigEndian();
					break;

				case EnumMetadataType.Unknown:
					break;

				default:
					throw new UnregisteredEnumTypeException(
						$"No parser is defined for enum's metadata type \"{metadata.MetadataType}\".");
			}

			// All read bytes between start and end must be metadata bytes
			metadata.MetadataLength = (int) (stream.Position - metadata.FilePosition);

			return metadata;
		}
	}
}