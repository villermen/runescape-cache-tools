using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Villermen.RuneScapeCacheTools.Exceptions;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.FileTypes
{
    /// <summary>
    /// Contains the properties of an item.
    /// </summary>
    public class ItemDefinitionFile : CacheFile
    {
        public int ModelId { get; set; }
        public string Name { get; set; } = "null";
        public ushort ModelZoom { get; set; }
        public ushort ModelRotation1 { get; set; }
        public ushort ModelRotation2 { get; set; }
        public short ModelOffset1 { get; set; }
        public short ModelOffset2 { get; set; }
        public bool Stackable { get; set; }
        public int Value { get; set; }
        public byte EquipSlotId { get; set; }
        public byte EquipId { get; set; }
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
        public bool Is25gp { get; set; }
        public ushort UnknownShort19 { get; set; }
        public ushort UnknownShort20 { get; set; }
        public ushort ShardAmount { get; set; }
        public string ShardName { get; set; }
        public bool UnknownSwitch2 { get; set; }
        public Dictionary<PropertyKey, object> Properties { get; set; } = new Dictionary<PropertyKey, object>();

        public override void Decode(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                Opcode opcode;
                do
                {
                    opcode = (Opcode)reader.ReadByte();

                    switch (opcode)
                    {
                        case Opcode.End:
                            break;

                        case Opcode.ModelId:
                            this.ModelId = reader.ReadAwkwardInt();
                            break;

                        case Opcode.Name:
                            this.Name = reader.ReadNullTerminatedString();
                            break;

                        case Opcode.ModelZoom:
                            this.ModelZoom = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.ModelRotation1:
                            this.ModelRotation1 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.ModelRotation2:
                            this.ModelRotation2 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.ModelOffset1:
                            this.ModelOffset1 = reader.ReadInt16BigEndian();
                            break;

                        case Opcode.ModelOffset2:
                            this.ModelOffset2 = reader.ReadInt16BigEndian();
                            break;

                        case Opcode.Stackable:
                            this.Stackable = true;
                            break;

                        case Opcode.Value:
                            this.Value = reader.ReadInt32BigEndian();
                            break;

                        case Opcode.EquipSlotId:
                            this.EquipSlotId = reader.ReadByte();
                            break;

                        case Opcode.EquipId:
                            this.EquipId = reader.ReadByte();
                            break;

                        case Opcode.MembersOnly:
                            this.MembersOnly = true;
                            break;

                        case Opcode.UnknownShort1:
                            this.UnknownShort1 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.MaleEquip1:
                            this.MaleEquip1 = reader.ReadAwkwardInt();
                            break;

                        case Opcode.MaleEquip2:
                            this.MaleEquip2 = reader.ReadAwkwardInt();
                            break;

                        case Opcode.FemaleEquip1:
                            this.FemaleEquip1 = reader.ReadAwkwardInt();
                            break;

                        case Opcode.FemaleEquip2:
                            this.FemaleEquip2 = reader.ReadAwkwardInt();
                            break;

                        case Opcode.UnknownByte1:
                            this.UnknownByte1 = reader.ReadByte();
                            break;

                        case Opcode.GroundOption1:
                            this.GroundOptions[0] = reader.ReadNullTerminatedString();
                            break;

                        case Opcode.GroundOption2:
                            this.GroundOptions[1] = reader.ReadNullTerminatedString();
                            break;

                        case Opcode.GroundOption3:
                            this.GroundOptions[2] = reader.ReadNullTerminatedString();
                            break;

                        case Opcode.GroundOption4:
                            this.GroundOptions[3] = reader.ReadNullTerminatedString();
                            break;

                        case Opcode.GroundOption5:
                            this.GroundOptions[4] = reader.ReadNullTerminatedString();
                            break;

                        case Opcode.InventoryOption1:
                            this.InventoryOptions[0] = reader.ReadNullTerminatedString();
                            break;

                        case Opcode.InventoryOption2:
                            this.InventoryOptions[1] = reader.ReadNullTerminatedString();
                            break;

                        case Opcode.InventoryOption3:
                            this.InventoryOptions[2] = reader.ReadNullTerminatedString();
                            break;

                        case Opcode.InventoryOption4:
                            this.InventoryOptions[3] = reader.ReadNullTerminatedString();
                            break;

                        case Opcode.InventoryOption5:
                            this.InventoryOptions[4] = reader.ReadNullTerminatedString();
                            break;

                        case Opcode.ModelColors:
                            var modelColorCount = reader.ReadByte();
                            this.OriginalModelColors = new ushort[modelColorCount];
                            this.ModifiedModelColors = new ushort[modelColorCount];
                            for (var i = 0; i < modelColorCount; i++)
                            {
                                this.OriginalModelColors[i] = reader.ReadUInt16BigEndian();
                                this.ModifiedModelColors[i] = reader.ReadUInt16BigEndian();
                            }
                            break;

                        case Opcode.TextureColors:
                            var textureColorCount = reader.ReadByte();
                            this.OriginalTextureColors = new ushort[textureColorCount];
                            this.ModifiedTextureColors = new ushort[textureColorCount];
                            for (var i = 0; i < textureColorCount; i++)
                            {
                                this.OriginalTextureColors[i] = reader.ReadUInt16BigEndian();
                                this.ModifiedTextureColors[i] = reader.ReadUInt16BigEndian();
                            }
                            break;

                        case Opcode.UnknownByteArray1:
                            var unknownByteArray1Length = reader.ReadByte();
                            this.UnknownByteArray1 = new byte[unknownByteArray1Length];
                            for (var i = 0; i < unknownByteArray1Length; i++)
                            {
                                this.UnknownByteArray1[i] = reader.ReadByte();
                            }
                            break;

                        case Opcode.UnknownInt1:
                            this.UnknownInt1 = reader.ReadUInt32BigEndian();
                            break;

                        case Opcode.UnknownShort2:
                            this.UnknownShort2 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownShort3:
                            this.UnknownShort3 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.Unnoted:
                            this.Unnoted = true;
                            break;

                        case Opcode.ColorEquip1:
                            this.ColorEquip1 = reader.ReadAwkwardInt();
                            break;

                        case Opcode.ColorEquip2:
                            this.ColorEquip2 = reader.ReadAwkwardInt();
                            break;

                        case Opcode.UnknownAwkwardInt1:
                            this.UnknownAwkwardInt1 = reader.ReadAwkwardInt();
                            break;

                        case Opcode.UnknownAwkwardInt2:
                            this.UnknownAwkwardInt2 = reader.ReadAwkwardInt();
                            break;

                        case Opcode.UnknownAwkwardInt3:
                            this.UnknownAwkwardInt3 = reader.ReadAwkwardInt();
                            break;

                        case Opcode.UnknownAwkwardInt4:
                            this.UnknownAwkwardInt4 = reader.ReadAwkwardInt();
                            break;

                        case Opcode.UnknownShort4:
                            this.UnknownShort4 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownShort5:
                            this.UnknownShort5 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownByte2:
                            this.UnknownByte2 = reader.ReadByte();
                            break;

                        case Opcode.NoteId:
                            this.NoteId = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.NoteTemplateId:
                            this.NoteTemplateId = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.Stack1:
                            this.Stacks[0] = new Tuple<ushort, ushort>(
                                reader.ReadUInt16BigEndian(),
                                reader.ReadUInt16BigEndian());
                            break;

                        case Opcode.Stack2:
                            this.Stacks[1] = new Tuple<ushort, ushort>(
                                reader.ReadUInt16BigEndian(),
                                reader.ReadUInt16BigEndian());
                            break;

                        case Opcode.Stack3:
                            this.Stacks[2] = new Tuple<ushort, ushort>(
                                reader.ReadUInt16BigEndian(),
                                reader.ReadUInt16BigEndian());
                            break;

                        case Opcode.Stack4:
                            this.Stacks[3] = new Tuple<ushort, ushort>(
                                reader.ReadUInt16BigEndian(),
                                reader.ReadUInt16BigEndian());
                            break;

                        case Opcode.Stack5:
                            this.Stacks[4] = new Tuple<ushort, ushort>(
                                reader.ReadUInt16BigEndian(),
                                reader.ReadUInt16BigEndian());
                            break;

                        case Opcode.Stack6:
                            this.Stacks[5] = new Tuple<ushort, ushort>(
                                reader.ReadUInt16BigEndian(),
                                reader.ReadUInt16BigEndian());
                            break;

                        case Opcode.Stack7:
                            this.Stacks[6] = new Tuple<ushort, ushort>(
                                reader.ReadUInt16BigEndian(),
                                reader.ReadUInt16BigEndian());
                            break;

                        case Opcode.Stack8:
                            this.Stacks[7] = new Tuple<ushort, ushort>(
                                reader.ReadUInt16BigEndian(),
                                reader.ReadUInt16BigEndian());
                            break;

                        case Opcode.Stack9:
                            this.Stacks[8] = new Tuple<ushort, ushort>(
                                reader.ReadUInt16BigEndian(),
                                reader.ReadUInt16BigEndian());
                            break;

                        case Opcode.Stack10:
                            this.Stacks[9] = new Tuple<ushort, ushort>(
                                reader.ReadUInt16BigEndian(),
                                reader.ReadUInt16BigEndian());
                            break;

                        case Opcode.UnknownShort6:
                            this.UnknownShort6 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownShort7:
                            this.UnknownShort7 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownShort8:
                            this.UnknownShort8 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownByte3:
                            this.UnknownByte3 = reader.ReadByte();
                            break;

                        case Opcode.UnknownByte4:
                            this.UnknownByte4 = reader.ReadByte();
                            break;

                        case Opcode.TeamId:
                            this.TeamId = reader.ReadByte();
                            break;

                        case Opcode.LendId:
                            this.LendId = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.LendTemplateId:
                            this.LendTemplateId = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownTribyte1:
                            this.UnknownTribyte1 = reader.ReadUInt24BigEndian();
                            break;

                        case Opcode.UnknownTribyte2:
                            this.UnknownTribyte2 = reader.ReadUInt24BigEndian();
                            break;

                        case Opcode.UnknownTribyte3:
                            this.UnknownTribyte3 = reader.ReadUInt24BigEndian();
                            break;

                        case Opcode.UnknownTribyte4:
                            this.UnknownTribyte4 = reader.ReadUInt24BigEndian();
                            break;

                        case Opcode.UnknownTribyte5:
                            this.UnknownTribyte5 = reader.ReadUInt24BigEndian();
                            break;

                        case Opcode.UnknownTribyte6:
                            this.UnknownTribyte6 = reader.ReadUInt24BigEndian();
                            break;

                        case Opcode.UnknownShortArray1:
                            var unknownShortArray1Length = reader.ReadByte();
                            this.UnknownShortArray1 = new ushort[unknownShortArray1Length];
                            for (var i = 0; i < unknownShortArray1Length; i++)
                            {
                                this.UnknownShortArray1[i] = reader.ReadUInt16BigEndian();
                            }
                            break;

                        case Opcode.UnknownByte5:
                            this.UnknownByte5 = reader.ReadByte();
                            break;

                        case Opcode.BindId:
                            this.BindId = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.BindTemplateId:
                            this.BindTemplateId = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownShort9:
                            this.UnknownShort9 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownShort10:
                            this.UnknownShort10 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownShort11:
                            this.UnknownShort11 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownShort12:
                            this.UnknownShort12 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownShort13:
                            this.UnknownShort13 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownShort14:
                            this.UnknownShort14 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownShort15:
                            this.UnknownShort15 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownShort16:
                            this.UnknownShort16 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownShort17:
                            this.UnknownShort17 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownShort18:
                            this.UnknownShort18 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.Is25gp:
                            this.Is25gp = true;
                            break;

                        case Opcode.UnknownShort19:
                            this.UnknownShort19 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.UnknownShort20:
                            this.UnknownShort20 = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.ShardAmount:
                            this.ShardAmount = reader.ReadUInt16BigEndian();
                            break;

                        case Opcode.ShardName:
                            this.ShardName = reader.ReadNullTerminatedString();
                            break;

                        case Opcode.UnknownSwitch2:
                            this.UnknownSwitch2 = true;
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

                                if (!this.Properties.ContainsKey((PropertyKey)key))
                                {
                                    this.Properties.Add((PropertyKey)key, value);
                                }
                                else
                                {
                                    // Duplicate properties are probably caused by improper tooling at Jagex HQ
                                    this.Properties[(PropertyKey)key] = value;
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
                    throw new DecodeException("Data remaining after parsing item definition.");
                }
            }
        }

        public override byte[] Encode()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetFields()
        {
            var result = new Dictionary<string, object>
            {
                {"ModelId", this.ModelId},
                {"Name", this.Name},
                {"ModelZoom", this.ModelZoom},
                {"ModelRotation1", this.ModelRotation1},
                {"ModelRotation2", this.ModelRotation2},
                {"ModelOffset1", this.ModelOffset1},
                {"ModelOffset2", this.ModelOffset2},
                {"Stackable", this.Stackable},
                {"Value", this.Value},
                {"EquipSlotId", this.EquipSlotId},
                {"EquipId", this.EquipId},
                {"MembersOnly", this.MembersOnly},
                {"UnknownShort1", this.UnknownShort1},
                {"MaleEquip1", this.MaleEquip1},
                {"MaleEquip2", this.MaleEquip2},
                {"FemaleEquip1", this.FemaleEquip1},
                {"FemaleEquip2", this.FemaleEquip2},
                {"UnknownByte1", this.UnknownByte1},
                {"GroundOption1", this.GroundOptions[0]},
                {"GroundOption2", this.GroundOptions[1]},
                {"GroundOption3", this.GroundOptions[2]},
                {"GroundOption4", this.GroundOptions[3]},
                {"GroundOption5", this.GroundOptions[4]},
                {"InventoryOption1", this.InventoryOptions[0]},
                {"InventoryOption2", this.InventoryOptions[1]},
                {"InventoryOption3", this.InventoryOptions[2]},
                {"InventoryOption4", this.InventoryOptions[3]},
                {"InventoryOption5", this.InventoryOptions[4]},
                {"OriginalModelColors", this.OriginalModelColors},
                {"ModifiedModelColors", this.ModifiedModelColors},
                {"OriginalTextureColors", this.OriginalTextureColors},
                {"ModifiedTextureColors", this.ModifiedTextureColors},
                {"UnknownByteArray1", this.UnknownByteArray1},
                {"UnknownInt1", this.UnknownInt1},
                {"UnknownShort2", this.UnknownShort2},
                {"UnknownShort3", this.UnknownShort3},
                {"Unnoted", this.Unnoted},
                {"ColorEquip1", this.ColorEquip1},
                {"ColorEquip2", this.ColorEquip2},
                {"UnknownAwkwardInt1", this.UnknownAwkwardInt1},
                {"UnknownAwkwardInt2", this.UnknownAwkwardInt2},
                {"UnknownAwkwardInt3", this.UnknownAwkwardInt3},
                {"UnknownAwkwardInt4", this.UnknownAwkwardInt4},
                {"UnknownShort4", this.UnknownShort4},
                {"UnknownShort5", this.UnknownShort5},
                {"UnknownByte2", this.UnknownByte2},
                {"NoteId", this.NoteId},
                {"NoteTemplateId", this.NoteTemplateId},
                {"Stack1", this.Stacks[0]},
                {"Stack2", this.Stacks[1]},
                {"Stack3", this.Stacks[2]},
                {"Stack4", this.Stacks[3]},
                {"Stack5", this.Stacks[4]},
                {"Stack6", this.Stacks[5]},
                {"Stack7", this.Stacks[6]},
                {"Stack8", this.Stacks[7]},
                {"Stack9", this.Stacks[8]},
                {"Stack10", this.Stacks[9]},
                {"UnknownShort6", this.UnknownShort6},
                {"UnknownShort7", this.UnknownShort7},
                {"UnknownShort8", this.UnknownShort8},
                {"UnknownByte3", this.UnknownByte3},
                {"UnknownByte4", this.UnknownByte4},
                {"TeamId", this.TeamId},
                {"LendId", this.LendId},
                {"LendTemplateId", this.LendTemplateId},
                {"UnknownTribyte1", this.UnknownTribyte1},
                {"UnknownTribyte2", this.UnknownTribyte2},
                {"UnknownTribyte3", this.UnknownTribyte3},
                {"UnknownTribyte4", this.UnknownTribyte4},
                {"UnknownTribyte5", this.UnknownTribyte5},
                {"UnknownTribyte6", this.UnknownTribyte6},
                {"UnknownShortArray1", this.UnknownShortArray1},
                {"UnknownByte5", this.UnknownByte5},
                {"BindId", this.BindId},
                {"BindTemplateId", this.BindTemplateId},
                {"UnknownShort9", this.UnknownShort9},
                {"UnknownShort10", this.UnknownShort10},
                {"UnknownShort11", this.UnknownShort11},
                {"UnknownShort12", this.UnknownShort12},
                {"UnknownShort13", this.UnknownShort13},
                {"UnknownShort14", this.UnknownShort14},
                {"UnknownShort15", this.UnknownShort15},
                {"UnknownShort16", this.UnknownShort16},
                {"UnknownShort17", this.UnknownShort17},
                {"UnknownShort18", this.UnknownShort18},
                {"Is25gp", this.Is25gp},
                {"UnknownShort19", this.UnknownShort19},
                {"UnknownShort20", this.UnknownShort20},
                {"ShardAmount", this.ShardAmount},
                {"ShardName", this.ShardName},
                {"UnknownSwitch2", this.UnknownSwitch2}
            };

            foreach (var property in this.Properties)
            {
                result.Add(
                    Enum.IsDefined(typeof(PropertyKey), property.Key)
                        ? $"Property{property.Key}"
                        : $"PropertyUnknown{property.Key}",
                    property.Value
                );
            }

            return result;
        }

        private enum Opcode
        {
            End = 0,
            ModelId = 1,
            Name = 2,
            ModelZoom = 4,
            ModelRotation1 = 5,
            ModelRotation2 = 6,
            ModelOffset1 = 7,
            ModelOffset2 = 8,
            Stackable = 11,
            Value = 12,
            EquipSlotId = 13,
            EquipId = 14,
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
            Is25gp = 157,
            UnknownShort19 = 161,
            UnknownShort20 = 162,
            ShardAmount = 163,
            ShardName = 164,
            UnknownSwitch2 = 165,
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
