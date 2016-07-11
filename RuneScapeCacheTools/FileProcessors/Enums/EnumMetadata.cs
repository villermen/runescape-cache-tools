using System.IO;

namespace Villermen.RuneScapeCacheTools.FileProcessors.Enums
{
	public class EnumMetadata
	{
		public uint FilePosition { get; private set; }

		public EnumDataType Type { get; private set; }

		public ushort NextEntryId { get; private set; }

		public ushort Count { get; private set; }

		public ushort ThirdValueThatIDoNotKnowTheDetailsOf { get; private set; }

		/// <summary>
		/// Yes, that's metadata of metadata https://viller.men/soundboard/154ffbca.
		/// </summary>
		public int MetadataLength { get; private set; }

		public static EnumMetadata FromStream(Stream stream)
		{
			var reader = new BinaryReader(stream);

			var position = stream.Position;

			byte keyTypeIdentifier;

			try
			{
				// Verify the enum signature (e(keyType)f)
				var signatureByte1 = reader.ReadByte();
				keyTypeIdentifier = reader.ReadByte();
				var signatureByte2 = reader.ReadByte();

				if (!(signatureByte1 == 0x65 && signatureByte2 == 0x66))
				{
					return null;
				}
			}
			catch (EndOfStreamException)
			{
				return null;
			}

			var valueTypeIdentifier = reader.ReadByte();
			var metaTypeIdentifier = reader.ReadByte();
			ushort nextEntryId = 0;
			ushort count;
			ushort thirdValueThatIDoNotKnowTheDetailsOf = 0;

			var type = (EnumDataType) valueTypeIdentifier;

			switch (type)
			{
				case EnumDataType.LoneInt:
					count = 1;
					break;

				case EnumDataType.IntInt:
				case EnumDataType.IntHexabyte:
					count = reader.ReadUInt16BigEndian();
					break;

				default:
					nextEntryId = reader.ReadUInt16BigEndian();
					count = reader.ReadUInt16BigEndian();
					break;
			}		

			var metadataLength = stream.Position - position;

			return new EnumMetadata()
			{
				FilePosition = (uint) position,
				Type = type,
				NextEntryId = nextEntryId,
				Count = count,
				ThirdValueThatIDoNotKnowTheDetailsOf = thirdValueThatIDoNotKnowTheDetailsOf,
				MetadataLength = (int) metadataLength
			};
		}
	}
}
