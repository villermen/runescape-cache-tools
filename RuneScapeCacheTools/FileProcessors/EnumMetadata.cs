namespace Villermen.RuneScapeCacheTools.FileProcessors
{
	public class EnumMetadata
	{
		public uint FilePosition { get; }
		public EnumValueType ValueType { get; }

		public EnumMetadata(uint filePosition, EnumValueType valueType)
		{
			FilePosition = filePosition;
			ValueType = valueType;
		}
	}
}
