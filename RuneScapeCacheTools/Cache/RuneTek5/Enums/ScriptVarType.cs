using System.Collections.Generic;
using System.Linq;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5.Enums
{
	public class ScriptVarType
	{
		public static readonly ScriptVarType Unknown = new ScriptVarType(null, null, BaseVarType.None, null);
		public readonly BaseVarType BaseType;
		public readonly char? CharId;
		public readonly object DefaultValue;

		public readonly int? IntId;

		protected ScriptVarType(byte? intId, char? charId, BaseVarType baseType, object defaultValue)
		{
			IntId = intId;
			CharId = charId;
			BaseType = baseType;
			DefaultValue = defaultValue;
		}

		public static ScriptVarType FromValue(int value)
		{
			return GetTypes().FirstOrDefault(type => type.IntId == value) ?? Unknown;
		}

		public static ScriptVarType FromValue(char value)
		{
			return GetTypes().FirstOrDefault(type => type.CharId == value) ?? Unknown;
		}

		public static IEnumerable<ScriptVarType> GetTypes()
		{
			var fieldInfo = typeof(ScriptVarType).GetFields();
			return fieldInfo
				.Where(field => field.IsStatic && field.FieldType != typeof(ScriptVarType))
				.Select(field => (ScriptVarType) field.GetValue(null));
		}

		// TODO: Add all known
	}
}