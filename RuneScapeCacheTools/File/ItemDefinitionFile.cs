using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Converters;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.File
{
    /// <summary>
    /// Contains the properties of an item.
    /// </summary>
    public class ItemDefinitionFile
    {
        public int? ModelId { get; set; }
        public string? Name { get; set; }
        public string? BuffEffect { get; set; }
        public ushort? ModelZoom { get; set; }
        public ushort? ModelRotation1 { get; set; }
        public ushort? ModelRotation2 { get; set; }
        public short? ModelOffset1 { get; set; }
        public short? ModelOffset2 { get; set; }
        public bool? Stackable { get; set; }
        public int? Value { get; set; }
        public byte? EquipSlotId { get; set; }
        public byte? EquipId { get; set; }
        public bool? Unknown15 { get; set; }
        public bool? MembersOnly { get; set; }
        public ushort? Unknown18 { get; set; }
        public int? MaleEquip1 { get; set; }
        public int? MaleEquip2 { get; set; }
        public int? FemaleEquip1 { get; set; }
        public int? FemaleEquip2 { get; set; }
        public byte? Unknown27 { get; set; }
        public string? GroundOption1 { get; set; }
        public string? GroundOption2 { get; set; }
        public string? GroundOption3 { get; set; }
        public string? GroundOption4 { get; set; }
        public string? GroundOption5 { get; set; }
        public string? InventoryOption1 { get; set; }
        public string? InventoryOption2 { get; set; }
        public string? InventoryOption3 { get; set; }
        public string? InventoryOption4 { get; set; }
        public string? InventoryOption5 { get; set; }
        public ushort[,]? ModelColors { get; set; }
        public ushort[,]? TextureColors { get; set; }
        public byte[]? Unknown42 { get; set; }
        public uint? Unknown43 { get; set; }
        public ushort? Unknown44 { get; set; }
        public ushort? Unknown45 { get; set; }
        public bool? Tradeable { get; set; }
        public int? GeBuyLimit { get; set; }
        public int? ColorEquip1 { get; set; }
        public int? ColorEquip2 { get; set; }
        public int? Unknown90 { get; set; }
        public int? Unknown91 { get; set; }
        public int? Unknown92 { get; set; }
        public int? Unknown93 { get; set; }
        public ushort? Unknown94 { get; set; }
        public ushort? Unknown95 { get; set; }
        public byte? Unknown96 { get; set; }
        public ushort? NoteId { get; set; }
        public ushort? NoteTemplateId { get; set; }
        public Tuple<ushort, ushort>? Stack1 { get; set; }
        public Tuple<ushort, ushort>? Stack2 { get; set; }
        public Tuple<ushort, ushort>? Stack3 { get; set; }
        public Tuple<ushort, ushort>? Stack4 { get; set; }
        public Tuple<ushort, ushort>? Stack5 { get; set; }
        public Tuple<ushort, ushort>? Stack6 { get; set; }
        public Tuple<ushort, ushort>? Stack7 { get; set; }
        public Tuple<ushort, ushort>? Stack8 { get; set; }
        public Tuple<ushort, ushort>? Stack9 { get; set; }
        public Tuple<ushort, ushort>? Stack10 { get; set; }
        public ushort? Unknown110 { get; set; }
        public ushort? Unknown111 { get; set; }
        public ushort? Unknown112 { get; set; }
        public byte? Unknown113 { get; set; }
        public byte? Unknown114 { get; set; }
        public byte? TeamId { get; set; }
        public ushort? LendId { get; set; }
        public ushort? LendTemplateId { get; set; }
        public int? Unknown125 { get; set; }
        public int? Unknown126 { get; set; }
        public int? Unknown127 { get; set; }
        public int? Unknown128 { get; set; }
        public int? Unknown129 { get; set; }
        public int? Unknown130 { get; set; }
        public ushort[]? Unknown132 { get; set; }
        public byte? Unknown134 { get; set; }
        public ushort? BindId { get; set; }
        public ushort? BindTemplateId { get; set; }
        public ushort? Unknown142 { get; set; }
        public ushort? Unknown143 { get; set; }
        public ushort? Unknown144 { get; set; }
        public ushort? Unknown145 { get; set; }
        public ushort? Unknown146 { get; set; }
        public ushort? Unknown150 { get; set; }
        public ushort? Unknown151 { get; set; }
        public ushort? Unknown152 { get; set; }
        public ushort? Unknown153 { get; set; }
        public ushort? Unknown154 { get; set; }
        public bool? Is25Gp { get; set; }
        public ushort? Unknown161 { get; set; }
        public ushort? Unknown162 { get; set; }
        public ushort? ShardAmount { get; set; }
        public string? ShardName { get; set; }
        public bool?  Unknown165 { get; set; }
        public bool? Unknown167 { get; set; }
        public bool? Unknown168 { get; set; }
        public Dictionary<PropertyKey, object>? Properties { get; set; }

        public static ItemDefinitionFile Decode(byte[] data)
        {
            var file = new ItemDefinitionFile();

            using var reader = new BinaryReader(new MemoryStream(data));

            Opcode opcode;
            do
            {
                opcode = (Opcode)reader.ReadByte();
                object? _ = opcode switch
                {
                    Opcode.End => null,
                    Opcode.ModelId => file.ModelId = reader.ReadAwkwardInt(),
                    Opcode.Name => file.Name = reader.ReadNullTerminatedString(),
                    Opcode.BuffEffect => file.BuffEffect = reader.ReadNullTerminatedString(),
                    Opcode.ModelZoom => file.ModelZoom = reader.ReadUInt16BigEndian(),
                    Opcode.ModelRotation1 => file.ModelRotation1 = reader.ReadUInt16BigEndian(),
                    Opcode.ModelRotation2 => file.ModelRotation2 = reader.ReadUInt16BigEndian(),
                    Opcode.ModelOffset1 => file.ModelOffset1 = reader.ReadInt16BigEndian(),
                    Opcode.ModelOffset2 => file.ModelOffset2 = reader.ReadInt16BigEndian(),
                    Opcode.Stackable => file.Stackable = true,
                    Opcode.Value => file.Value = reader.ReadInt32BigEndian(),
                    Opcode.EquipSlotId => file.EquipSlotId = reader.ReadByte(),
                    Opcode.EquipId => file.EquipId = reader.ReadByte(),
                    Opcode.Unknown15 => file.Unknown15 = true,
                    Opcode.MembersOnly => file.MembersOnly = true,
                    Opcode.Unknown18 => file.Unknown18 = reader.ReadUInt16BigEndian(),
                    Opcode.MaleEquip1 => file.MaleEquip1 = reader.ReadAwkwardInt(),
                    Opcode.MaleEquip2 => file.MaleEquip2 = reader.ReadAwkwardInt(),
                    Opcode.FemaleEquip1 => file.FemaleEquip1 = reader.ReadAwkwardInt(),
                    Opcode.FemaleEquip2 => file.FemaleEquip2 = reader.ReadAwkwardInt(),
                    Opcode.Unknown27 => file.Unknown27 = reader.ReadByte(),
                    Opcode.GroundOption1 => file.GroundOption1 = reader.ReadNullTerminatedString(),
                    Opcode.GroundOption2 => file.GroundOption2 = reader.ReadNullTerminatedString(),
                    Opcode.GroundOption3 => file.GroundOption3 = reader.ReadNullTerminatedString(),
                    Opcode.GroundOption4 => file.GroundOption4 = reader.ReadNullTerminatedString(),
                    Opcode.GroundOption5 => file.GroundOption5 = reader.ReadNullTerminatedString(),
                    Opcode.InventoryOption1 => file.InventoryOption1 = reader.ReadNullTerminatedString(),
                    Opcode.InventoryOption2 => file.InventoryOption2 = reader.ReadNullTerminatedString(),
                    Opcode.InventoryOption3 => file.InventoryOption3 = reader.ReadNullTerminatedString(),
                    Opcode.InventoryOption4 => file.InventoryOption4 = reader.ReadNullTerminatedString(),
                    Opcode.InventoryOption5 => file.InventoryOption5 = reader.ReadNullTerminatedString(),
                    Opcode.ModelColors => file.ModelColors = reader.ReadInterlacedUInt16BigEndianArrays(2, reader.ReadByte()),
                    Opcode.TextureColors => file.TextureColors = reader.ReadInterlacedUInt16BigEndianArrays(2, reader.ReadByte()),
                    Opcode.Unknown42 => file.Unknown42 = reader.ReadBytesExactly(reader.ReadByte()),
                    Opcode.Unknown43 => file.Unknown43 = reader.ReadUInt32BigEndian(),
                    Opcode.Unknown44 => file.Unknown44 = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown45 => file.Unknown45 = reader.ReadUInt16BigEndian(),
                    Opcode.Tradeable => file.Tradeable = true,
                    Opcode.GeBuyLimit => file.GeBuyLimit = reader.ReadInt32BigEndian(),
                    Opcode.ColorEquip1 => file.ColorEquip1 = reader.ReadAwkwardInt(),
                    Opcode.ColorEquip2 => file.ColorEquip2 = reader.ReadAwkwardInt(),
                    Opcode.Unknown90 => file.Unknown90 = reader.ReadAwkwardInt(),
                    Opcode.Unknown91 => file.Unknown91 = reader.ReadAwkwardInt(),
                    Opcode.Unknown92 => file.Unknown92 = reader.ReadAwkwardInt(),
                    Opcode.Unknown93 => file.Unknown93 = reader.ReadAwkwardInt(),
                    Opcode.Unknown94 => file.Unknown94 = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown95 => file.Unknown95 = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown96 => file.Unknown96 = reader.ReadByte(),
                    Opcode.NoteId => file.NoteId = reader.ReadUInt16BigEndian(),
                    Opcode.NoteTemplateId => file.NoteTemplateId = reader.ReadUInt16BigEndian(),
                    Opcode.Stack1 => file.Stack1 = new Tuple<ushort, ushort>(reader.ReadUInt16BigEndian(), reader.ReadUInt16BigEndian()),
                    Opcode.Stack2 => file.Stack2 = new Tuple<ushort, ushort>(reader.ReadUInt16BigEndian(), reader.ReadUInt16BigEndian()),
                    Opcode.Stack3 => file.Stack3 = new Tuple<ushort, ushort>(reader.ReadUInt16BigEndian(), reader.ReadUInt16BigEndian()),
                    Opcode.Stack4 => file.Stack4 = new Tuple<ushort, ushort>(reader.ReadUInt16BigEndian(), reader.ReadUInt16BigEndian()),
                    Opcode.Stack5 => file.Stack5 = new Tuple<ushort, ushort>(reader.ReadUInt16BigEndian(), reader.ReadUInt16BigEndian()),
                    Opcode.Stack6 => file.Stack6 = new Tuple<ushort, ushort>(reader.ReadUInt16BigEndian(), reader.ReadUInt16BigEndian()),
                    Opcode.Stack7 => file.Stack7 = new Tuple<ushort, ushort>(reader.ReadUInt16BigEndian(), reader.ReadUInt16BigEndian()),
                    Opcode.Stack8 => file.Stack8 = new Tuple<ushort, ushort>(reader.ReadUInt16BigEndian(), reader.ReadUInt16BigEndian()),
                    Opcode.Stack9 => file.Stack9 = new Tuple<ushort, ushort>(reader.ReadUInt16BigEndian(), reader.ReadUInt16BigEndian()),
                    Opcode.Stack10 => file.Stack10 = new Tuple<ushort, ushort>(reader.ReadUInt16BigEndian(), reader.ReadUInt16BigEndian()),
                    Opcode.Unknown110 => file.Unknown110 = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown111 => file.Unknown111 = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown112 => file.Unknown112 = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown113 => file.Unknown113 = reader.ReadByte(),
                    Opcode.Unknown114 => file.Unknown114 = reader.ReadByte(),
                    Opcode.TeamId => file.TeamId = reader.ReadByte(),
                    Opcode.LendId => file.LendId = reader.ReadUInt16BigEndian(),
                    Opcode.LendTemplateId => file.LendTemplateId = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown125 => file.Unknown125 = reader.ReadUInt24BigEndian(),
                    Opcode.Unknown126 => file.Unknown126 = reader.ReadUInt24BigEndian(),
                    Opcode.Unknown127 => file.Unknown127 = reader.ReadUInt24BigEndian(),
                    Opcode.Unknown128 => file.Unknown128 = reader.ReadUInt24BigEndian(),
                    Opcode.Unknown129 => file.Unknown129 = reader.ReadUInt24BigEndian(),
                    Opcode.Unknown130 => file.Unknown130 = reader.ReadUInt24BigEndian(),
                    Opcode.Unknown132 => file.Unknown132 = reader.ReadUInt16BigEndians(reader.ReadByte()),
                    Opcode.Unknown134 => file.Unknown134 = reader.ReadByte(),
                    Opcode.BindId => file.BindId = reader.ReadUInt16BigEndian(),
                    Opcode.BindTemplateId => file.BindTemplateId = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown142 => file.Unknown142 = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown143 => file.Unknown143 = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown144 => file.Unknown144 = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown145 => file.Unknown145 = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown146 => file.Unknown146 = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown150 => file.Unknown150 = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown151 => file.Unknown151 = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown152 => file.Unknown152 = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown153 => file.Unknown153 = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown154 => file.Unknown154 = reader.ReadUInt16BigEndian(),
                    Opcode.Is25Gp => file.Is25Gp = true,
                    Opcode.Unknown161 => file.Unknown161 = reader.ReadUInt16BigEndian(),
                    Opcode.Unknown162 => file.Unknown162 = reader.ReadUInt16BigEndian(),
                    Opcode.ShardAmount => file.ShardAmount = reader.ReadUInt16BigEndian(),
                    Opcode.ShardName => file.ShardName = reader.ReadNullTerminatedString(),
                    Opcode.Unknown165 => file.Unknown165 = true,
                    Opcode.Unknown167 => file.Unknown167 = true,
                    Opcode.Unknown168 => file.Unknown168 = true,
                    Opcode.Properties => file.Properties = ItemDefinitionFile.ReadProperties(reader),
                    _ => throw new DecodeException($"Unknown opcode {opcode}."),
                };
            }
            while (opcode != Opcode.End);

            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                throw new DecodeException($"Data remaining after decoding item definition. {reader.BaseStream.Length - reader.BaseStream.Position} bytes remain.");
            }

            return file;
        }

        /// <summary>
        /// Returns all properties of the item that are defined (not null).
        /// </summary>
        public Dictionary<string, object> GetDefinedProperties()
        {
            // TODO: This feels very hacky but so does listing every property (which would be harder to maintain too). Think about how this could work better reusable for encoding. Maybe do list it but in the extractor?
            return typeof(ItemDefinitionFile)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(typeProperty => new KeyValuePair<string, object>(typeProperty.Name, typeProperty.GetValue(this)))
                .Where(typeProperty => typeProperty.Value != null)
                .ToDictionary(typeProperty => typeProperty.Key, typeProperty => typeProperty.Value);
        }

        private static Dictionary<PropertyKey, object> ReadProperties(BinaryReader reader)
        {
            var properties = new Dictionary<PropertyKey, object>();

            var propertyCount = reader.ReadByte();
            for (var i = 0; i < propertyCount; i++)
            {
                var valueIsString = reader.ReadByte() != 0;
                var key = reader.ReadUInt24BigEndian();

                var value = valueIsString
                    ? (object)reader.ReadNullTerminatedString()
                    : (object)reader.ReadInt32BigEndian();

                // Note: Duplicate properties exist and are probably caused by improper tooling at Jagex HQ.
                properties[(PropertyKey)key] = value;
            }

            return properties;
        }

        private enum Opcode
        {
            End = 0,
            ModelId = 1,
            Name = 2,
            BuffEffect = 3,
            ModelZoom = 4,
            ModelRotation1 = 5,
            ModelRotation2 = 6,
            ModelOffset1 = 7,
            ModelOffset2 = 8,
            Stackable = 11,
            Value = 12,
            EquipSlotId = 13,
            EquipId = 14,
            Unknown15 = 15,
            MembersOnly = 16,
            Unknown18 = 18,
            MaleEquip1 = 23,
            MaleEquip2 = 24,
            FemaleEquip1 = 25,
            FemaleEquip2 = 26,
            Unknown27 = 27,
            GroundOption1 = 30,
            GroundOption2 = 31,
            GroundOption3 = 32,
            GroundOption4 = 33,
            GroundOption5 = 34,
            InventoryOption1 = 35,
            InventoryOption2 = 36,
            InventoryOption3 = 37,
            InventoryOption4 = 38,
            InventoryOption5 = 39,
            ModelColors = 40,
            TextureColors = 41,
            Unknown42 = 42,
            Unknown43 = 43,
            Unknown44 = 44,
            Unknown45 = 45,
            Tradeable = 65,
            GeBuyLimit = 69,
            ColorEquip1 = 78,
            ColorEquip2 = 79,
            Unknown90 = 90,
            Unknown91 = 91,
            Unknown92 = 92,
            Unknown93 = 93,
            Unknown94 = 94,
            Unknown95 = 95,
            Unknown96 = 96,
            NoteId = 97,
            NoteTemplateId = 98,
            Stack1 = 100,
            Stack2 = 101,
            Stack3 = 102,
            Stack4 = 103,
            Stack5 = 104,
            Stack6 = 105,
            Stack7 = 106,
            Stack8 = 107,
            Stack9 = 108,
            Stack10 = 109,
            Unknown110 = 110,
            Unknown111 = 111,
            Unknown112 = 112,
            Unknown113 = 113,
            Unknown114 = 114,
            TeamId = 115,
            LendId = 121,
            LendTemplateId = 122,
            Unknown125 = 125,
            Unknown126 = 126,
            Unknown127 = 127,
            Unknown128 = 128,
            Unknown129 = 129,
            Unknown130 = 130,
            Unknown132 = 132,
            Unknown134 = 134,
            BindId = 139,
            BindTemplateId = 140,
            Unknown142 = 142,
            Unknown143 = 143,
            Unknown144 = 144,
            Unknown145 = 145,
            Unknown146 = 146,
            Unknown150 = 150,
            Unknown151 = 151,
            Unknown152 = 152,
            Unknown153 = 153,
            Unknown154 = 154,
            Is25Gp = 157,
            Unknown161 = 161,
            Unknown162 = 162,
            ShardAmount = 163,
            ShardName = 164,
            Unknown165 = 165,
            Unknown167 = 167,
            Unknown168 = 168,
            Properties = 249
        }

        public enum PropertyKey
        {
            EquipOption1 = 528,
            EquipOption2 = 529,
            EquipOption3 = 530,
            EquipOption4 = 531,
            EquipSkillRequired = 749,
            EquipLevelRequired = 750,
            LifePointBonus = 1326,
            MeleeAffinity = 2866,
            RangedAffinity = 2867,
            MagicAffinity = 2868,
            ArmourBonus = 2870,
            PotionEffectValue = 3000,
            PortentOfDegradationHealAmount = 3698,
            Broken = 3793,
            UnknownMtxDescription = 4085,
            SpecialAttackCost = 4332,
            SpecialAttackName = 4333,
            SpecialAttackDescription = 4334,
            DestroyText = 5417,
            ZarosItem = 5440,
            UnknownFayreTokenRelated = 6405,
            SigilCooldownDefault = 6520,
            SigilCooldown = 6521,
            SigilMaxCharges = 6522
        }
    }
}
