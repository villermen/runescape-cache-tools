using System;
using System.Collections.Generic;
using System.Linq;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5.Enums
{
	public class ScriptVarType
	{
		public static readonly ScriptVarType Unlisted = new ScriptVarType(null, null, BaseVarType.None, null);

		public static readonly ScriptVarType Integer = new ScriptVarType(0, 'i', BaseVarType.Integer, 0);
		public static readonly ScriptVarType Boolean = new ScriptVarType(1, '1', BaseVarType.Integer, 0);
		public static readonly ScriptVarType Type2 = new ScriptVarType(2, '2', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type3 = new ScriptVarType(3, ':', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type4 = new ScriptVarType(4, ',', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Cursor = new ScriptVarType(5, '@', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Animation = new ScriptVarType(6, 'A', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type7 = new ScriptVarType(7, 'C', BaseVarType.Integer, -1);
		public static readonly ScriptVarType UInt24 = new ScriptVarType(8, 'H', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Interface = new ScriptVarType(9, 'I', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Identikit = new ScriptVarType(10, 'K', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Music = new ScriptVarType(11, 'M', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type12 = new ScriptVarType(12, 'N', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type13 = new ScriptVarType(13, 'O', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type14 = new ScriptVarType(14, 'P', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type15 = new ScriptVarType(15, 'Q', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type16 = new ScriptVarType(16, 'R', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Skill = new ScriptVarType(17, 'S', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type18 = new ScriptVarType(18, 'T', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type19 = new ScriptVarType(19, 'V', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type20 = new ScriptVarType(20, '^', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type21 = new ScriptVarType(21, '`', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Coordinate = new ScriptVarType(22, 'c', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Graphic = new ScriptVarType(23, 'd', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type24 = new ScriptVarType(24, 'e', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Font = new ScriptVarType(25, 'f', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Enum = new ScriptVarType(26, 'g', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type27 = new ScriptVarType(27, 'h', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type28 = new ScriptVarType(28, 'j', BaseVarType.Integer, -1);
		public static readonly ScriptVarType ChatCategory = new ScriptVarType(29, 'k', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Location = new ScriptVarType(30, 'l', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Model = new ScriptVarType(31, 'm', BaseVarType.Integer, -1);
		public static readonly ScriptVarType NPC = new ScriptVarType(32, 'n', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Object = new ScriptVarType(33, 'o', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Player = new ScriptVarType(34, 'p', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Description = new ScriptVarType(35, 'r', BaseVarType.Long, -1L);
		public static readonly ScriptVarType String = new ScriptVarType(36, 's', BaseVarType.String, "");
		public static readonly ScriptVarType Spot = new ScriptVarType(37, 't', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type38 = new ScriptVarType(38, 'u', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Inventory = new ScriptVarType(39, 'v', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type40 = new ScriptVarType(40, 'x', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type41 = new ScriptVarType(41, 'y', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Character = new ScriptVarType(42, 'z', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type43 = new ScriptVarType(43, '|', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Render = new ScriptVarType(44, '\u20ac', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type45 = new ScriptVarType(45, '\u0192', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type46 = new ScriptVarType(46, '\u2021', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type47 = new ScriptVarType(47, '\u2030', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type48 = new ScriptVarType(48, '\u0160', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type49 = new ScriptVarType(49, '\u0152', BaseVarType.Long, -1L);
		public static readonly ScriptVarType Vector3 = new ScriptVarType(50, '\u017d', BaseVarType.Vector3, new Vector3());
		public static readonly ScriptVarType Type51 = new ScriptVarType(51, '\u0161', BaseVarType.Integer, -1);

		public static readonly ScriptVarType Type53 = new ScriptVarType(53, '\u00a1', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type54 = new ScriptVarType(54, '\u00a2', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type55 = new ScriptVarType(55, '\u00a3', BaseVarType.Integer, -1);
		public static readonly ScriptVarType ClanThread = new ScriptVarType(56, '\u00a7', BaseVarType.Long, -1L);
		public static readonly ScriptVarType Type57 = new ScriptVarType(57, '\u00ab', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type58 = new ScriptVarType(58, '\u00ae', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type59 = new ScriptVarType(59, '\u00b5', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type60 = new ScriptVarType(60, '\u00b6', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type61 = new ScriptVarType(61, '\u00c6', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type62 = new ScriptVarType(62, '\u00d7', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type63 = new ScriptVarType(63, '\u00de', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type64 = new ScriptVarType(64, '\u00e1', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type65 = new ScriptVarType(65, '\u00e6', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type66 = new ScriptVarType(66, '\u00e9', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type67 = new ScriptVarType(67, '\u00ed', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type68 = new ScriptVarType(68, '\u00ee', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type69 = new ScriptVarType(69, '\u00f3', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type70 = new ScriptVarType(70, '\u00fa', BaseVarType.Integer, -1);
		public static readonly ScriptVarType UserHash = new ScriptVarType(71, '\u00fb', BaseVarType.Long, -1L);
		public static readonly ScriptVarType Type72 = new ScriptVarType(72, '\u00ce', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Struct = new ScriptVarType(73, 'J', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type74 = new ScriptVarType(74, '\u00d0', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type75 = new ScriptVarType(75, '\u00a4', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type76 = new ScriptVarType(76, '\u00a5', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type77 = new ScriptVarType(77, '\u00e8', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type78 = new ScriptVarType(78, '\u00b9', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type79 = new ScriptVarType(79, '\u00b0', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type80 = new ScriptVarType(80, '\u00ec', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type81 = new ScriptVarType(81, '\u00eb', BaseVarType.Integer, -1);

		public static readonly ScriptVarType Type83 = new ScriptVarType(83, '\u00fe', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type84 = new ScriptVarType(84, '\u00fd', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type85 = new ScriptVarType(85, '\u00ff', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type86 = new ScriptVarType(86, '\u00f5', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type87 = new ScriptVarType(87, '\u00f4', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type88 = new ScriptVarType(88, '\u00f6', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type89 = new ScriptVarType(89, '\u00f2', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type90 = new ScriptVarType(90, '\u00dc', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type91 = new ScriptVarType(91, '\u00f9', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type92 = new ScriptVarType(92, '\u00ef', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type93 = new ScriptVarType(93, '\u00af', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type94 = new ScriptVarType(94, '\u00ea', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type95 = new ScriptVarType(95, '\u00f0', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type96 = new ScriptVarType(96, '\u00e5', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type97 = new ScriptVarType(97, 'a', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type98 = new ScriptVarType(98, 'F', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type99 = new ScriptVarType(99, 'L', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type100 = new ScriptVarType(100, '\u00a9', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type101 = new ScriptVarType(101, '\u00dd', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type102 = new ScriptVarType(102, '\u00ac', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type103 = new ScriptVarType(103, '\u00f8', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type104 = new ScriptVarType(104, '\u00e4', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type105 = new ScriptVarType(105, '\u00e3', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type106 = new ScriptVarType(106, '\u00e2', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type107 = new ScriptVarType(107, '\u00e0', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type108 = new ScriptVarType(108, '\u00c0', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type109 = new ScriptVarType(109, '\u00d2', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type110 = new ScriptVarType(110, '\u00cf', BaseVarType.Long, 0L);
		public static readonly ScriptVarType Type111 = new ScriptVarType(111, '\u00cc', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type112 = new ScriptVarType(112, '\u00c9', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type113 = new ScriptVarType(113, '\u00ca', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type114 = new ScriptVarType(114, '\u00f7', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type115 = new ScriptVarType(115, '\u00bc', BaseVarType.Long, -1L);
		public static readonly ScriptVarType Type116 = new ScriptVarType(116, '\u00bd', BaseVarType.Long, -1L);
		public static readonly ScriptVarType Type117 = new ScriptVarType(117, '\u2022', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type118 = new ScriptVarType(118, '\u00c2', BaseVarType.Long, -1L);
		public static readonly ScriptVarType Type119 = new ScriptVarType(119, '\u00c3', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type120 = new ScriptVarType(120, '\u00c5', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type121 = new ScriptVarType(121, '\u00cb', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type122 = new ScriptVarType(122, '\u00cd', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type123 = new ScriptVarType(123, '\u00d5', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type124 = new ScriptVarType(124, '\u00b2', BaseVarType.Integer, -1);

		public static readonly ScriptVarType Type200 = new ScriptVarType(200, 'X', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type201 = new ScriptVarType(201, 'W', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type202 = new ScriptVarType(202, 'b', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type203 = new ScriptVarType(203, 'B', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type204 = new ScriptVarType(204, '4', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type205 = new ScriptVarType(205, 'w', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type206 = new ScriptVarType(206, 'q', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type207 = new ScriptVarType(207, '0', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Type208 = new ScriptVarType(208, '6', BaseVarType.Integer, -1);

		public static readonly ScriptVarType Unknown1 = new ScriptVarType(null, '#', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown2 = new ScriptVarType(null, '(', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown3 = new ScriptVarType(null, '%', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown4 = new ScriptVarType(null, '&', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown5 = new ScriptVarType(null, ')', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown6 = new ScriptVarType(null, '3', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown7 = new ScriptVarType(null, '5', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown8 = new ScriptVarType(null, '7', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown9 = new ScriptVarType(null, '8', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown10 = new ScriptVarType(null, '9', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown11 = new ScriptVarType(null, 'D', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown12 = new ScriptVarType(null, 'G', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown13 = new ScriptVarType(null, 'U', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown14 = new ScriptVarType(null, '\u00c1', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown15 = new ScriptVarType(null, 'Z', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown16 = new ScriptVarType(null, '~', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown17 = new ScriptVarType(null, '\u00b1', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown18 = new ScriptVarType(null, '\u00bb', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown19 = new ScriptVarType(null, '\u00bf', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown20 = new ScriptVarType(null, '\u00c7', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown21 = new ScriptVarType(null, '\u00d8', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown22 = new ScriptVarType(null, '\u00d1', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown23 = new ScriptVarType(null, '\u00f1', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown24 = new ScriptVarType(null, '\u00d9', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown25 = new ScriptVarType(null, '\u00df', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown26 = new ScriptVarType(null, 'E', BaseVarType.Integer, -1);
		public static readonly ScriptVarType IntArray = new ScriptVarType(null, 'Y', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown27 = new ScriptVarType(null, '\u00c4', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown28 = new ScriptVarType(null, '\u00fc', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown29 = new ScriptVarType(null, '\u00da', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown30 = new ScriptVarType(null, '\u00db', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown31 = new ScriptVarType(null, '\u00d3', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown32 = new ScriptVarType(null, '\u00c8', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown33 = new ScriptVarType(null, '\u00d4', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown34 = new ScriptVarType(null, '\u00be', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown35 = new ScriptVarType(null, '\u00d6', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown36 = new ScriptVarType(null, '\u00b3', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown37 = new ScriptVarType(null, '\u00b7', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown38 = new ScriptVarType(null, '\0', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown39 = new ScriptVarType(null, '\0', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown40 = new ScriptVarType(null, '\0', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown41 = new ScriptVarType(null, '\u00ba', BaseVarType.Integer, -1);
		public static readonly ScriptVarType Unknown42 = new ScriptVarType(null, '!', BaseVarType.None, -1);
		public static readonly ScriptVarType Unknown43 = new ScriptVarType(null, '$', BaseVarType.None, -1);
		public static readonly ScriptVarType Unknown44 = new ScriptVarType(null, '?', BaseVarType.None, -1);
		public static readonly ScriptVarType Unknown45 = new ScriptVarType(null, '\u00e7', BaseVarType.None, -1);
		public static readonly ScriptVarType Unknown46 = new ScriptVarType(null, '*', BaseVarType.None, -1);

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
			return GetTypes().FirstOrDefault(type => type.IntId == value) ?? Unlisted;
		}

		public static ScriptVarType FromValue(char value)
		{
			return GetTypes().FirstOrDefault(type => type.CharId == value) ?? Unlisted;
		}

		public static IEnumerable<ScriptVarType> GetTypes()
		{
			var fieldInfo = typeof(ScriptVarType).GetFields();
			return fieldInfo
				.Where(field => field.IsStatic && field.FieldType == typeof(ScriptVarType))
				.Select(field => (ScriptVarType) field.GetValue(null));
		}

		public override string ToString()
		{
			var fieldInfo = typeof(ScriptVarType).GetFields();
			return fieldInfo
				.Where(field => field.IsStatic && field.FieldType == typeof(ScriptVarType) && field.GetValue(null) == this)
				.Select(field => field.Name)
				.First();
		}
	}
}