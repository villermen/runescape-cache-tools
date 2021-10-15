using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.File
{
    /// <summary>
    /// Contains the properties of an item.
    /// </summary>
    public class ItemDefinitionFile
    {
        public int ModelId { get; set; }
        public string Name { get; set; }
        public string BuffEffect { get; set; }
        public ushort ModelZoom { get; set; }
        public ushort ModelRotation1 { get; set; }
        public ushort ModelRotation2 { get; set; }
        public short ModelOffset1 { get; set; }
        public short ModelOffset2 { get; set; }
        public bool Stackable { get; set; }
        public int Value { get; set; }
        public byte EquipSlotId { get; set; }
        public byte EquipId { get; set; }
        public bool UnknownSwitch15 { get; set; }
        public bool MembersOnly { get; set; }
        public ushort UnknownShort1 { get; set; }
        public int MaleEquip1 { get; set; }
        public int MaleEquip2 { get; set; }
        public int FemaleEquip1 { get; set; }
        public int FemaleEquip2 { get; set; }
        public byte UnknownByte1 { get; set; }
        public string[] GroundOptions { get; set; } = new string[5];
        public string[] InventoryOptions { get; set; } = new string[5];
        public ushort[] OriginalModelColors { get; set; }
        public ushort[] ModifiedModelColors { get; set; }
        public ushort[] OriginalTextureColors { get; set; }
        public ushort[] ModifiedTextureColors { get; set; }
        public byte[] UnknownByteArray1 { get; set; }
        public uint UnknownInt1 { get; set; }
        public ushort UnknownShort2 { get; set; }
        public ushort UnknownShort3 { get; set; }
        public bool Unnoted { get; set; }
        public int GeBuyLimit { get; set; }
        public int ColorEquip1 { get; set; }
        public int ColorEquip2 { get; set; }
        public int UnknownAwkwardInt1 { get; set; }
        public int UnknownAwkwardInt2 { get; set; }
        public int UnknownAwkwardInt3 { get; set; }
        public int UnknownAwkwardInt4 { get; set; }
        public ushort UnknownShort4 { get; set; }
        public ushort UnknownShort5 { get; set; }
        public byte UnknownByte2 { get; set; }
        public ushort NoteId { get; set; }
        public ushort NoteTemplateId { get; set; }
        public Tuple<ushort, ushort>[] Stacks { get; set; } = new Tuple<ushort, ushort>[10];
        public ushort UnknownShort6 { get; set; }
        public ushort UnknownShort7 { get; set; }
        public ushort UnknownShort8 { get; set; }
        public byte UnknownByte3 { get; set; }
        public byte UnknownByte4 { get; set; }
        public byte TeamId { get; set; }
        public ushort LendId { get; set; }
        public ushort LendTemplateId { get; set; }
        public int UnknownTribyte1 { get; set; }
        public int UnknownTribyte2 { get; set; }
        public int UnknownTribyte3 { get; set; }
        public int UnknownTribyte4 { get; set; }
        public int UnknownTribyte5 { get; set; }
        public int UnknownTribyte6 { get; set; }
        public ushort[] UnknownShortArray1 { get; set; }
        public byte UnknownByte5 { get; set; }
        public ushort BindId { get; set; }
        public ushort BindTemplateId { get; set; }
        public ushort UnknownShort9 { get; set; }
        public ushort UnknownShort10 { get; set; }
        public ushort UnknownShort11 { get; set; }
        public ushort UnknownShort12 { get; set; }
        public ushort UnknownShort13 { get; set; }
        public ushort UnknownShort14 { get; set; }
        public ushort UnknownShort15 { get; set; }
        public ushort UnknownShort16 { get; set; }
        public ushort UnknownShort17 { get; set; }
        public ushort UnknownShort18 { get; set; }
        public bool Is25Gp { get; set; }
        public ushort UnknownShort19 { get; set; }
        public ushort UnknownShort20 { get; set; }
        public ushort ShardAmount { get; set; }
        public string ShardName { get; set; }
        public bool UnknownSwitch165 { get; set; }
        public bool UnknownSwitch167 { get; set; }
        public bool UnknownSwitch168 { get; set; }
        public Dictionary<PropertyKey, object> Properties { get; set; } = new Dictionary<PropertyKey, object>();

        public static ItemDefinitionFile Decode(byte[] data)
        {
            var file = new ItemDefinitionFile();

            using var reader = new BinaryReader(new MemoryStream(data));

            Opcode opcode;
            do
            {
                opcode = (Opcode)reader.ReadByte();

                switch (opcode)
                {
                    case Opcode.End:
                        break;

                    case Opcode.ModelId:
                        file.ModelId = reader.ReadAwkwardInt();
                        break;

                    case Opcode.Name:
                        file.Name = reader.ReadNullTerminatedString();
                        break;

                    case Opcode.BuffEffect:
                        file.BuffEffect = reader.ReadNullTerminatedString();
                        break;

                    case Opcode.ModelZoom:
                        file.ModelZoom = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.ModelRotation1:
                        file.ModelRotation1 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.ModelRotation2:
                        file.ModelRotation2 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.ModelOffset1:
                        file.ModelOffset1 = reader.ReadInt16BigEndian();
                        break;

                    case Opcode.ModelOffset2:
                        file.ModelOffset2 = reader.ReadInt16BigEndian();
                        break;

                    case Opcode.Stackable:
                        file.Stackable = true;
                        break;

                    case Opcode.Value:
                        file.Value = reader.ReadInt32BigEndian();
                        break;

                    case Opcode.EquipSlotId:
                        file.EquipSlotId = reader.ReadByte();
                        break;

                    case Opcode.EquipId:
                        file.EquipId = reader.ReadByte();
                        break;

                    case Opcode.UnknownSwitch15:
                        file.UnknownSwitch15 = true;
                        break;

                    case Opcode.MembersOnly:
                        file.MembersOnly = true;
                        break;

                    case Opcode.UnknownShort1:
                        file.UnknownShort1 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.MaleEquip1:
                        file.MaleEquip1 = reader.ReadAwkwardInt();
                        break;

                    case Opcode.MaleEquip2:
                        file.MaleEquip2 = reader.ReadAwkwardInt();
                        break;

                    case Opcode.FemaleEquip1:
                        file.FemaleEquip1 = reader.ReadAwkwardInt();
                        break;

                    case Opcode.FemaleEquip2:
                        file.FemaleEquip2 = reader.ReadAwkwardInt();
                        break;

                    case Opcode.UnknownByte1:
                        file.UnknownByte1 = reader.ReadByte();
                        break;

                    case Opcode.GroundOption1:
                        file.GroundOptions[0] = reader.ReadNullTerminatedString();
                        break;

                    case Opcode.GroundOption2:
                        file.GroundOptions[1] = reader.ReadNullTerminatedString();
                        break;

                    case Opcode.GroundOption3:
                        file.GroundOptions[2] = reader.ReadNullTerminatedString();
                        break;

                    case Opcode.GroundOption4:
                        file.GroundOptions[3] = reader.ReadNullTerminatedString();
                        break;

                    case Opcode.GroundOption5:
                        file.GroundOptions[4] = reader.ReadNullTerminatedString();
                        break;

                    case Opcode.InventoryOption1:
                        file.InventoryOptions[0] = reader.ReadNullTerminatedString();
                        break;

                    case Opcode.InventoryOption2:
                        file.InventoryOptions[1] = reader.ReadNullTerminatedString();
                        break;

                    case Opcode.InventoryOption3:
                        file.InventoryOptions[2] = reader.ReadNullTerminatedString();
                        break;

                    case Opcode.InventoryOption4:
                        file.InventoryOptions[3] = reader.ReadNullTerminatedString();
                        break;

                    case Opcode.InventoryOption5:
                        file.InventoryOptions[4] = reader.ReadNullTerminatedString();
                        break;

                    case Opcode.ModelColors:
                        var modelColorCount = reader.ReadByte();
                        file.OriginalModelColors = new ushort[modelColorCount];
                        file.ModifiedModelColors = new ushort[modelColorCount];
                        for (var i = 0; i < modelColorCount; i++)
                        {
                            file.OriginalModelColors[i] = reader.ReadUInt16BigEndian();
                            file.ModifiedModelColors[i] = reader.ReadUInt16BigEndian();
                        }
                        break;

                    case Opcode.TextureColors:
                        var textureColorCount = reader.ReadByte();
                        file.OriginalTextureColors = new ushort[textureColorCount];
                        file.ModifiedTextureColors = new ushort[textureColorCount];
                        for (var i = 0; i < textureColorCount; i++)
                        {
                            file.OriginalTextureColors[i] = reader.ReadUInt16BigEndian();
                            file.ModifiedTextureColors[i] = reader.ReadUInt16BigEndian();
                        }
                        break;

                    case Opcode.UnknownByteArray1:
                        var unknownByteArray1Length = reader.ReadByte();
                        file.UnknownByteArray1 = new byte[unknownByteArray1Length];
                        for (var i = 0; i < unknownByteArray1Length; i++)
                        {
                            file.UnknownByteArray1[i] = reader.ReadByte();
                        }
                        break;

                    case Opcode.UnknownInt1:
                        file.UnknownInt1 = reader.ReadUInt32BigEndian();
                        break;

                    case Opcode.UnknownShort2:
                        file.UnknownShort2 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownShort3:
                        file.UnknownShort3 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.Unnoted:
                        file.Unnoted = true;
                        break;

                    case Opcode.GeBuyLimit:
                        file.GeBuyLimit = reader.ReadInt32BigEndian();
                        break;

                    case Opcode.ColorEquip1:
                        file.ColorEquip1 = reader.ReadAwkwardInt();
                        break;

                    case Opcode.ColorEquip2:
                        file.ColorEquip2 = reader.ReadAwkwardInt();
                        break;

                    case Opcode.UnknownAwkwardInt1:
                        file.UnknownAwkwardInt1 = reader.ReadAwkwardInt();
                        break;

                    case Opcode.UnknownAwkwardInt2:
                        file.UnknownAwkwardInt2 = reader.ReadAwkwardInt();
                        break;

                    case Opcode.UnknownAwkwardInt3:
                        file.UnknownAwkwardInt3 = reader.ReadAwkwardInt();
                        break;

                    case Opcode.UnknownAwkwardInt4:
                        file.UnknownAwkwardInt4 = reader.ReadAwkwardInt();
                        break;

                    case Opcode.UnknownShort4:
                        file.UnknownShort4 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownShort5:
                        file.UnknownShort5 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownByte2:
                        file.UnknownByte2 = reader.ReadByte();
                        break;

                    case Opcode.NoteId:
                        file.NoteId = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.NoteTemplateId:
                        file.NoteTemplateId = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.Stack1:
                        file.Stacks[0] = new Tuple<ushort, ushort>(
                            reader.ReadUInt16BigEndian(),
                            reader.ReadUInt16BigEndian());
                        break;

                    case Opcode.Stack2:
                        file.Stacks[1] = new Tuple<ushort, ushort>(
                            reader.ReadUInt16BigEndian(),
                            reader.ReadUInt16BigEndian());
                        break;

                    case Opcode.Stack3:
                        file.Stacks[2] = new Tuple<ushort, ushort>(
                            reader.ReadUInt16BigEndian(),
                            reader.ReadUInt16BigEndian());
                        break;

                    case Opcode.Stack4:
                        file.Stacks[3] = new Tuple<ushort, ushort>(
                            reader.ReadUInt16BigEndian(),
                            reader.ReadUInt16BigEndian());
                        break;

                    case Opcode.Stack5:
                        file.Stacks[4] = new Tuple<ushort, ushort>(
                            reader.ReadUInt16BigEndian(),
                            reader.ReadUInt16BigEndian());
                        break;

                    case Opcode.Stack6:
                        file.Stacks[5] = new Tuple<ushort, ushort>(
                            reader.ReadUInt16BigEndian(),
                            reader.ReadUInt16BigEndian());
                        break;

                    case Opcode.Stack7:
                        file.Stacks[6] = new Tuple<ushort, ushort>(
                            reader.ReadUInt16BigEndian(),
                            reader.ReadUInt16BigEndian());
                        break;

                    case Opcode.Stack8:
                        file.Stacks[7] = new Tuple<ushort, ushort>(
                            reader.ReadUInt16BigEndian(),
                            reader.ReadUInt16BigEndian());
                        break;

                    case Opcode.Stack9:
                        file.Stacks[8] = new Tuple<ushort, ushort>(
                            reader.ReadUInt16BigEndian(),
                            reader.ReadUInt16BigEndian());
                        break;

                    case Opcode.Stack10:
                        file.Stacks[9] = new Tuple<ushort, ushort>(
                            reader.ReadUInt16BigEndian(),
                            reader.ReadUInt16BigEndian());
                        break;

                    case Opcode.UnknownShort6:
                        file.UnknownShort6 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownShort7:
                        file.UnknownShort7 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownShort8:
                        file.UnknownShort8 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownByte3:
                        file.UnknownByte3 = reader.ReadByte();
                        break;

                    case Opcode.UnknownByte4:
                        file.UnknownByte4 = reader.ReadByte();
                        break;

                    case Opcode.TeamId:
                        file.TeamId = reader.ReadByte();
                        break;

                    case Opcode.LendId:
                        file.LendId = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.LendTemplateId:
                        file.LendTemplateId = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownTribyte1:
                        file.UnknownTribyte1 = reader.ReadUInt24BigEndian();
                        break;

                    case Opcode.UnknownTribyte2:
                        file.UnknownTribyte2 = reader.ReadUInt24BigEndian();
                        break;

                    case Opcode.UnknownTribyte3:
                        file.UnknownTribyte3 = reader.ReadUInt24BigEndian();
                        break;

                    case Opcode.UnknownTribyte4:
                        file.UnknownTribyte4 = reader.ReadUInt24BigEndian();
                        break;

                    case Opcode.UnknownTribyte5:
                        file.UnknownTribyte5 = reader.ReadUInt24BigEndian();
                        break;

                    case Opcode.UnknownTribyte6:
                        file.UnknownTribyte6 = reader.ReadUInt24BigEndian();
                        break;

                    case Opcode.UnknownShortArray1:
                        var unknownShortArray1Length = reader.ReadByte();
                        file.UnknownShortArray1 = new ushort[unknownShortArray1Length];
                        for (var i = 0; i < unknownShortArray1Length; i++)
                        {
                            file.UnknownShortArray1[i] = reader.ReadUInt16BigEndian();
                        }
                        break;

                    case Opcode.UnknownByte5:
                        file.UnknownByte5 = reader.ReadByte();
                        break;

                    case Opcode.BindId:
                        file.BindId = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.BindTemplateId:
                        file.BindTemplateId = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownShort9:
                        file.UnknownShort9 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownShort10:
                        file.UnknownShort10 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownShort11:
                        file.UnknownShort11 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownShort12:
                        file.UnknownShort12 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownShort13:
                        file.UnknownShort13 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownShort14:
                        file.UnknownShort14 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownShort15:
                        file.UnknownShort15 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownShort16:
                        file.UnknownShort16 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownShort17:
                        file.UnknownShort17 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownShort18:
                        file.UnknownShort18 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.Is25Gp:
                        file.Is25Gp = true;
                        break;

                    case Opcode.UnknownShort19:
                        file.UnknownShort19 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.UnknownShort20:
                        file.UnknownShort20 = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.ShardAmount:
                        file.ShardAmount = reader.ReadUInt16BigEndian();
                        break;

                    case Opcode.ShardName:
                        file.ShardName = reader.ReadNullTerminatedString();
                        break;

                    case Opcode.UnknownSwitch165:
                        file.UnknownSwitch165 = true;
                        break;

                    case Opcode.UnknownSwitch167:
                        file.UnknownSwitch167 = true;
                        break;

                    case Opcode.UnknownSwitch168:
                        file.UnknownSwitch168 = true;
                        break;

                    case Opcode.Properties:
                        var propertyCount = reader.ReadByte();

                        for (var i = 0; i < propertyCount; i++)
                        {
                            var valueIsString = reader.ReadByte() != 0;
                            var key = reader.ReadUInt24BigEndian();

                            var value = valueIsString
                                ? (object)reader.ReadNullTerminatedString()
                                : reader.ReadInt32BigEndian();

                            if (!file.Properties.ContainsKey((PropertyKey)key))
                            {
                                file.Properties.Add((PropertyKey)key, value);
                            }
                            else
                            {
                                // Duplicate properties are probably caused by improper tooling at Jagex HQ
                                file.Properties[(PropertyKey)key] = value;
                            }
                        }
                        break;

                    default:
                        throw new DecodeException($"Unknown opcode {opcode}.");
                }
            }
            while (opcode != Opcode.End);

            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                throw new DecodeException($"Data remaining after decoding item definition. {reader.BaseStream.Length - reader.BaseStream.Position} bytes remain.");
            }

            return file;
        }

        public Dictionary<string, string> GetFields()
        {
            var result = new Dictionary<string, string>();

            // Add all public properties' names and values (except "Properties") to the list
            foreach (var typeProperty in typeof(ItemDefinitionFile).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (typeProperty.Name == "Properties")
                {
                    continue;
                }

                result.Add(typeProperty.Name, Formatter.GetValueRepresentation(typeProperty.GetValue(this)));
            }

            // Every property will get its own key
            foreach (var property in this.Properties)
            {
                result.Add(
                    Enum.IsDefined(typeof(PropertyKey), property.Key)
                        ? $"Property{property.Key}"
                        : $"PropertyUnknown{property.Key}",
                    Formatter.GetValueRepresentation(property.Value)
                );
            }

            return result;
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
            UnknownSwitch15 = 15,
            MembersOnly = 16,
            UnknownShort1 = 18,
            MaleEquip1 = 23,
            MaleEquip2 = 24,
            FemaleEquip1 = 25,
            FemaleEquip2 = 26,
            UnknownByte1 = 27,
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
            UnknownByteArray1 = 42,
            UnknownInt1 = 43,
            UnknownShort2 = 44,
            UnknownShort3 = 45,
            Unnoted = 65,
            GeBuyLimit = 69,
            ColorEquip1 = 78,
            ColorEquip2 = 79,
            UnknownAwkwardInt1 = 90,
            UnknownAwkwardInt2 = 91,
            UnknownAwkwardInt3 = 92,
            UnknownAwkwardInt4 = 93,
            UnknownShort4 = 94,
            UnknownShort5 = 95,
            UnknownByte2 = 96,
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
            UnknownShort6 = 110,
            UnknownShort7 = 111,
            UnknownShort8 = 112,
            UnknownByte3 = 113,
            UnknownByte4 = 114,
            TeamId = 115,
            LendId = 121,
            LendTemplateId = 122,
            UnknownTribyte1 = 125,
            UnknownTribyte2 = 126,
            UnknownTribyte3 = 127,
            UnknownTribyte4 = 128,
            UnknownTribyte5 = 129,
            UnknownTribyte6 = 130,
            UnknownShortArray1 = 132,
            UnknownByte5 = 134,
            BindId = 139,
            BindTemplateId = 140,
            UnknownShort9 = 142,
            UnknownShort10 = 143,
            UnknownShort11 = 144,
            UnknownShort12 = 145,
            UnknownShort13 = 146,
            UnknownShort14 = 150,
            UnknownShort15 = 151,
            UnknownShort16 = 152,
            UnknownShort17 = 153,
            UnknownShort18 = 154,
            Is25Gp = 157,
            UnknownShort19 = 161,
            UnknownShort20 = 162,
            ShardAmount = 163,
            ShardName = 164,
            UnknownSwitch165 = 165,
            UnknownSwitch167 = 167,
            UnknownSwitch168 = 168,
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
