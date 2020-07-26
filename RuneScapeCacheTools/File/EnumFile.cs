using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.File
{
    /// <summary>
    /// Represents a list of values or references to values located elsewhere in the cache.
    /// </summary>
    public class EnumFile : IEnumerable<KeyValuePair<int, object>>
    {
        public int DefaultInteger { get; set; }
        public string DefaultString { get; set; } = "null";

        public ScriptVarType KeyType { get; set; }
        public Dictionary<int, object> Values { get; set; }
        public ScriptVarType ValueType { get; set; }

        public object this[int key] => this.Values[key];

        public bool ContainsKey(int key)
        {
            return this.Values.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<int, object>> GetEnumerator()
        {
            return this.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public static EnumFile Decode(byte[] data)
        {
            var file = new EnumFile();

            var dataReader = new BinaryReader(new MemoryStream(data));
            var ended = false;

            while ((dataReader.BaseStream.Position < dataReader.BaseStream.Length) && !ended)
            {
                var opcode = (Opcode)dataReader.ReadByte();

                switch (opcode)
                {
                    case Opcode.CharKeyType:
                        file.KeyType = ScriptVarType.FromValue(dataReader.ReadAwkwardChar());
                        break;

                    case Opcode.CharValueType:
                        file.ValueType = ScriptVarType.FromValue(dataReader.ReadAwkwardChar());
                        break;

                    case Opcode.DefaultString:
                        file.DefaultString = dataReader.ReadNullTerminatedString();
                        break;

                    case Opcode.DefaultInteger:
                        file.DefaultInteger = dataReader.ReadInt32BigEndian();
                        break;

                    case Opcode.StringDataDictionary:
                    case Opcode.IntegerDataDictionary:
                        var count = dataReader.ReadUInt16BigEndian();
                        file.Values = new Dictionary<int, object>(count);

                        for (var i = 0; i < count; i++)
                        {
                            var key = dataReader.ReadInt32BigEndian();
                            object value;

                            if (opcode == Opcode.StringDataDictionary)
                            {
                                value = dataReader.ReadNullTerminatedString();
                            }
                            else
                            {
                                value = dataReader.ReadInt32BigEndian();
                            }

                            file.Values[key] = value;
                        }
                        break;

                    case Opcode.StringDataArray:
                    case Opcode.IntegerDataArray:
                        var max = dataReader.ReadUInt16BigEndian();
                        count = dataReader.ReadUInt16BigEndian();
                        file.Values = new Dictionary<int, object>(count);

                        for (var i = 0; i < count; i++)
                        {
                            var key = dataReader.ReadUInt16BigEndian();
                            if (opcode == Opcode.StringDataArray)
                            {
                                file.Values[key] = dataReader.ReadNullTerminatedString();
                            }
                            else
                            {
                                file.Values[key] = dataReader.ReadInt32BigEndian();
                            }
                        }
                        break;

                    case Opcode.ByteKeyType:
                        file.KeyType = ScriptVarType.FromValue(dataReader.ReadAwkwardShort());
                        break;

                    case Opcode.ByteValueType:
                        file.ValueType = ScriptVarType.FromValue(dataReader.ReadAwkwardShort());
                        break;

                    case Opcode.End:
                        ended = true;
                        break;

                    default:
                        throw new DecodeException($"Invalid enum opcode \"{opcode}\".");
                }
            }

            if (file.KeyType == null)
            {
                throw new DecodeException("Enum data does not contain a key type.");
            }

            if (file.ValueType == null)
            {
                throw new DecodeException("Enum data does not contain a value type.");
            }

            if (file.Values == null)
            {
                throw new DecodeException("Enum does not contain any values.");
            }

            if (dataReader.BaseStream.Position < dataReader.BaseStream.Length)
            {
                throw new DecodeException($"Input data not fully consumed while decoding enum file. {dataReader.BaseStream.Length - dataReader.BaseStream.Position} bytes remain.");
            }

            return file;
        }

        public enum Opcode
        {
            End = 0,
            CharKeyType = 1,
            CharValueType = 2,
            DefaultString = 3,
            DefaultInteger = 4,
            StringDataDictionary = 5,
            IntegerDataDictionary = 6,
            StringDataArray = 7,
            IntegerDataArray = 8,
            ByteKeyType = 101,
            ByteValueType = 102
        }
    }
}
