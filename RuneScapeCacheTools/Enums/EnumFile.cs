using System.Collections;
using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Enums
{
    public class EnumFile : IEnumerable<KeyValuePair<int, object>>
    {
        public EnumFile(byte[] data)
        {
            var dataReader = new BinaryReader(new MemoryStream(data));
            var ended = false;

            while (dataReader.BaseStream.Position < dataReader.BaseStream.Length && !ended)
            {
                var opcode = (EnumOpcode) dataReader.ReadByte();

                switch (opcode)
                {
                    case EnumOpcode.CharKeyType:
                        KeyType = ScriptVarType.FromValue(dataReader.ReadAwkwardChar());
                        break;

                    case EnumOpcode.CharValueType:
                        ValueType = ScriptVarType.FromValue(dataReader.ReadAwkwardChar());
                        break;

                    case EnumOpcode.DefaultString:
                        DefaultString = dataReader.ReadNullTerminatedString();
                        break;

                    case EnumOpcode.DefaultInteger:
                        DefaultInteger = dataReader.ReadInt32BigEndian();
                        break;

                    case EnumOpcode.StringDataDictionary:
                    case EnumOpcode.IntegerDataDictionary:
                        var count = dataReader.ReadUInt16BigEndian();
                        Values = new Dictionary<int, object>(count);

                        for (var i = 0; i < count; i++)
                        {
                            var key = dataReader.ReadInt32BigEndian();
                            object value;

                            if (opcode == EnumOpcode.StringDataDictionary)
                            {
                                value = dataReader.ReadNullTerminatedString();
                            }
                            else
                            {
                                value = dataReader.ReadInt32BigEndian();
                            }

                            Values[key] = value;
                        }
                        break;

                    case EnumOpcode.StringDataArray:
                    case EnumOpcode.IntegerDataArray:
                        var max = dataReader.ReadUInt16BigEndian();
                        count = dataReader.ReadUInt16BigEndian();
                        Values = new Dictionary<int, object>(count);

                        for (var i = 0; i < count; i++)
                        {
                            var key = dataReader.ReadUInt16BigEndian();
                            if (opcode == EnumOpcode.StringDataArray)
                            {
                                Values[key] = dataReader.ReadNullTerminatedString();
                            }
                            else
                            {
                                Values[key] = dataReader.ReadInt32BigEndian();
                            }
                        }
                        break;

                    case EnumOpcode.ByteKeyType:
                        KeyType = ScriptVarType.FromValue(dataReader.ReadSmartShort());
                        break;

                    case EnumOpcode.ByteValueType:
                        ValueType = ScriptVarType.FromValue(dataReader.ReadSmartShort());
                        break;

                    case EnumOpcode.End:
                        ended = true;
                        break;

                    default:
                        throw new CacheException($"Invalid enum opcode \"{opcode}\".");
                }
            }

            if (KeyType == null)
            {
                throw new EnumParseException("Enum data does not contain a key type.");
            }

            if (ValueType == null)
            {
                throw new EnumParseException("Enum data does not contain a value type.");
            }

            if (Values == null)
            {
                throw new EnumParseException("Enum does not contain any values.");
            }
        }

        public object this[int key] => Values[key];

        public ScriptVarType KeyType { get; }
        public ScriptVarType ValueType { get; }
        public Dictionary<int, object> Values { get; }
        public string DefaultString { get; } = "null";
        public int DefaultInteger { get; }

        public IEnumerator<KeyValuePair<int, object>> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsKey(int key)
        {
            return Values.ContainsKey(key);
        }
    }
}