namespace Villermen.RuneScapeCacheTools.FileProcessors.Enums
{
	public enum EnumOpcode
	{
		DefaultString = 0x03,
		DefaultInteger = 0x04,
		StringDataMap = 0x05,
		IntegerDataMap = 0x06,
		StringDataArray = 0x07,
		IntegerDataArray = 0x08,
		KeyType = 0x65,
		ValueType = 0x66,
	}
}