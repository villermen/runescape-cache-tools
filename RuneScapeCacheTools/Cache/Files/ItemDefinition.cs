using System;
using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Exceptions;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.Files
{
    /// <summary>
    /// Contains the properties of an item.
    /// </summary>
    public class ItemDefinition : CacheFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string[] InventoryActions { get; } = new string[5];
        public string[] GroundActions { get; } = new string[5];
        public IList<Tuple<int, object>> Properties { get; set; } = new List<Tuple<int, object>>();
        public bool MembersOnly { get; set; }

        public bool UnknownSwitch { get; set; }
        public bool UnknownSwitch2 { get; set; }

        public byte UnknownByte { get; set; }

        public short UnknownShort { get; set; }
        public short UnknownShort2 { get; set; }
        public short UnknownShort3 { get; set; }
        public short UnknownShort4 { get; set; }
        public short UnknownShort5 { get; set; }
        public short UnknownShort6 { get; set; }
        public short UnknownShort7 { get; set; }
        public short UnknownShort8 { get; set; }
        public short UnknownShort9 { get; set; }
        public short UnknownShort10 { get; set; }
        public short UnknownShort11{ get; set; }
        public short UnknownShort12 { get; set; }

        public int UnknownInt { get; set; }
        public int UnknownInt2 { get; set; }
        public int UnknownInt3 { get; set; }
        public int UnknownInt4 { get; set; }

        public int UnknownInt6 { get; set; }

        public IList<int> UnknownIntMap { get; set; } = new List<int>();

        protected override void Decode(byte[] data)
        {
            using (var dataReader = new BinaryReader(new MemoryStream(data)))
            {
                Opcode opcode;
                do
                {
                    opcode = (Opcode) dataReader.ReadByte();

                    switch (opcode)
                    {
                        case Opcode.End:
                            break;

                        case Opcode.Id:
                            this.Id = dataReader.ReadAwkwardInt();
                            break;

                        case Opcode.Name:
                            this.Name = dataReader.ReadNullTerminatedString();
                            break;

                        case Opcode.GroundAction1:
                            this.GroundActions[0] = dataReader.ReadNullTerminatedString();
                            break;

                        case Opcode.GroundAction2:
                            this.GroundActions[1] = dataReader.ReadNullTerminatedString();
                            break;

                        case Opcode.GroundAction3:
                            this.GroundActions[2] = dataReader.ReadNullTerminatedString();
                            break;

                        case Opcode.GroundAction4:
                            this.GroundActions[3] = dataReader.ReadNullTerminatedString();
                            break;

                        case Opcode.GroundAction5:
                            this.GroundActions[4] = dataReader.ReadNullTerminatedString();
                            break;

                        case Opcode.InventoryAction1:
                            this.InventoryActions[0] = dataReader.ReadNullTerminatedString();
                            break;

                        case Opcode.InventoryAction2:
                            this.InventoryActions[1] = dataReader.ReadNullTerminatedString();
                            break;

                        case Opcode.InventoryAction3:
                            this.InventoryActions[2] = dataReader.ReadNullTerminatedString();
                            break;

                        case Opcode.InventoryAction4:
                            this.InventoryActions[3] = dataReader.ReadNullTerminatedString();
                            break;

                        case Opcode.InventoryAction5:
                            this.InventoryActions[4] = dataReader.ReadNullTerminatedString();
                            break;

                        case Opcode.Properties:
                            var amountOfValues = dataReader.ReadByte();

                            for (var i = 0; i < amountOfValues; i++)
                            {
                                var valueIsString = dataReader.ReadByte() != 0;
                                var key = dataReader.ReadUInt24BigEndian();
                                var value = valueIsString
                                    ? (object)dataReader.ReadNullTerminatedString()
                                    : dataReader.ReadUInt32BigEndian();

                                this.Properties.Add(new Tuple<int, object>(key, value));
                            }
                            break;

                        case Opcode.UnknownInt:
                            this.UnknownInt = dataReader.ReadInt32BigEndian();
                            break;

                        case Opcode.UnknownInt2:
                            this.UnknownInt2 = dataReader.ReadInt32BigEndian();
                            break;

                        case Opcode.UnknownInt3:
                            this.UnknownInt3 = dataReader.ReadInt32BigEndian();
                            break;

                        case Opcode.UnknownInt4:
                            this.UnknownInt4 = dataReader.ReadInt32BigEndian();
                            break;

                        case Opcode.UnknownInt6:
                            this.UnknownInt6 = dataReader.ReadInt32BigEndian();
                            break;

                        case Opcode.UnknownShort:
                            this.UnknownShort = dataReader.ReadInt16BigEndian();
                            break;

                        case Opcode.UnknownShort2:
                            this.UnknownShort2 = dataReader.ReadInt16BigEndian();
                            break;

                        case Opcode.UnknownShort3:
                            this.UnknownShort3 = dataReader.ReadInt16BigEndian();
                            break;

                        case Opcode.UnknownShort4:
                            this.UnknownShort4 = dataReader.ReadInt16BigEndian();
                            break;

                        case Opcode.UnknownShort5:
                            this.UnknownShort5 = dataReader.ReadInt16BigEndian();
                            break;

                        case Opcode.UnknownShort6:
                            this.UnknownShort6 = dataReader.ReadInt16BigEndian();
                            break;

                        case Opcode.UnknownShort7:
                            this.UnknownShort7 = dataReader.ReadInt16BigEndian();
                            break;

                        case Opcode.UnknownShort8:
                            this.UnknownShort8 = dataReader.ReadInt16BigEndian();
                            break;

                        case Opcode.UnknownShort9:
                            this.UnknownShort9 = dataReader.ReadInt16BigEndian();
                            break;

                        case Opcode.UnknownShort10:
                            this.UnknownShort10 = dataReader.ReadInt16BigEndian();
                            break;

                        case Opcode.UnknownShort11:
                            this.UnknownShort11 = dataReader.ReadInt16BigEndian();
                            break;

                        case Opcode.UnknownShort12:
                            this.UnknownShort12 = dataReader.ReadInt16BigEndian();
                            break;

                        case Opcode.MembersOnly:
                            this.MembersOnly = true;
                            break;

                        case Opcode.UnknownSwitch:
                            this.UnknownSwitch = true;
                            break;

                        case Opcode.UnknownSwitch2:
                            this.UnknownSwitch2 = true;
                            break;

                        case Opcode.UnknownIntMap:
                            var length = dataReader.ReadByte();
                            for (var i = 0; i < length; i++)
                            {
                                this.UnknownIntMap.Add(dataReader.ReadInt32BigEndian());
                            }
                            break;

                        case Opcode.UnknownByte:
                            this.UnknownByte = dataReader.ReadByte();
                            break;

                        default:
                            throw new DecodeException($"Unknown opcode 0x{(byte)opcode:X2}.");
                    }
                }
                while (opcode != Opcode.End);

                if (dataReader.BaseStream.Position < dataReader.BaseStream.Length)
                {
                    throw new DecodeException("Data remaining after parsing item definition.");
                }
            }
        }

        protected override byte[] Encode()
        {
            throw new NotImplementedException();
        }

        private enum Opcode
        {
            End = 0x00,

            Id = 0x01,
            Name = 0x02,
            UnknownShort2 = 0x04,
            UnknownInt6 = 0x05,
            UnknownShort4 = 0x06,
            UnknownShort7 = 0x07,
            UnknownShort = 0x08,
            UnknownInt = 0x0C,
            UnknownByte = 0x0D,
            MembersOnly = 0x10,
            UnknownSwitch2 = 0x11,
            UnknownInt2 = 0x17,
            UnknownInt4 = 0x19,
            GroundAction1 = 0x1E,
            GroundAction2 = 0x1F,
            GroundAction3 = 0x20,
            GroundAction4 = 0x21,
            GroundAction5 = 0x22,
            InventoryAction1 = 0x23,
            InventoryAction2 = 0x24,
            InventoryAction3 = 0x25,
            InventoryAction4 = 0x26,
            InventoryAction5 = 0x27,
            UnknownIntMap = 0x28,
            UnknownSwitch = 0x41,
            UnknownShort8 = 0x5E,
            UnknownShort3 = 0x5F,
            UnknownShort5 = 0x61,
            UnknownShort6 = 0x62,
            UnknownInt3 = 0x65,
            UnknownShort9 = 0x90,
            UnknownShort10 = 0x91,
            UnknownShort11 = 0x96,
            UnknownShort12 = 0x97,
            Properties = 0xF9
        }
    }
}
