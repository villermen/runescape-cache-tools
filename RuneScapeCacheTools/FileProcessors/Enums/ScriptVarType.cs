using System.Collections.Generic;
using System.Linq;

namespace Villermen.RuneScapeCacheTools.FileProcessors.Enums
{
	public class ScriptVarType
	{
		public static readonly ScriptVarType Unknown = new ScriptVarType(null, null, BaseVarType.None, null);

		public readonly byte? ByteId;
		public readonly char? CharId;
		public readonly BaseVarType BaseType;
		public readonly object DefaultValue;

		protected ScriptVarType(byte? byteId, char? charId, BaseVarType baseType, object defaultValue)
		{
			ByteId = byteId;
			CharId = charId;
			BaseType = baseType;
			DefaultValue = defaultValue;
		}

		public static ScriptVarType FromByteId(byte byteId)
		{
			return GetTypes().FirstOrDefault(type => type.ByteId == byteId) ?? Unknown;
		}

		public static ScriptVarType FromCharId(char charId)
		{
			return GetTypes().FirstOrDefault(type => type.CharId == charId) ?? Unknown;
		}

		public static IEnumerable<ScriptVarType> GetTypes()
		{
			var fieldInfo = typeof(ScriptVarType).GetFields();
			return fieldInfo
				.Where(field => field.IsStatic && field.FieldType != typeof(ScriptVarType))
				.Select(field => (ScriptVarType) field.GetValue(null));
		}
	}
}
