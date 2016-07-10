using System.IO;

namespace Villermen.RuneScapeCacheTools.FileProcessors.Enums
{
	public class EnumMetadata
	{
		public uint FilePosition { get; }
		public EnumType Type { get; }
		public ushort NextEntryId { get; }
		public ushort Count { get; }

		/// <summary>
		/// Yes, that's metadata of metadata https://viller.men/soundboard/154ffbca.
		/// </summary>
		public int MetadataLength { get; }

		public EnumMetadata(uint filePosition, EnumType type, ushort nextEntryId, ushort count, int metadataLength)
		{
			FilePosition = filePosition;
			Type = type;
			NextEntryId = nextEntryId;
			Count = count;
			MetadataLength = metadataLength;
		}

		public static EnumMetadata FromStream(Stream stream)
		{
			var reader = new BinaryReader(stream);

			var position = stream.Position;

			// Verify the enum signature (e(something)f)
			try
			{
				var signature = reader.ReadUInt24BigEndian();
				if ((signature >> 16) != 0x65 || (signature & 0xff) != 0x66)
				{
					return null;
				}
			}
			catch (EndOfStreamException)
			{
				return null;
			}

			var iHonestlyDoNotKnowYet = reader.ReadByte();
			var valueTypeIdentifier = reader.ReadByte();
			ushort nextEntryId = 0;
			ushort count;

			var type = (EnumType) valueTypeIdentifier;

			switch (type)
			{
				case EnumType.LoneInt:
					count = 1;
					break;

				case EnumType.IntInt:
					count = reader.ReadUInt16BigEndian();
					break;

				default:
					nextEntryId = reader.ReadUInt16BigEndian();
					count = reader.ReadUInt16BigEndian();
					break;
			}		

			var metadataLength = stream.Position - position;

			return new EnumMetadata((uint) position, (EnumType)valueTypeIdentifier, nextEntryId, count, (int) metadataLength);
		}
	}
}
