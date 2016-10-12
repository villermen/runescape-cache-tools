using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    internal class VorbisCommentHeader : VorbisHeader
    {
        public const byte PacketType = 0x03;

        public VorbisCommentHeader()
        {
            UserComments = new ReadOnlyCollection<Tuple<string, string>>(_userComments);
        }

        public static VorbisCommentHeader Decode(byte[] packetData)
        {
            var dataReader = new BinaryReader(new MemoryStream(packetData));

            var packetType = dataReader.ReadByte();
            if (packetType != PacketType)
            {
                throw new VorbisException($"Vorbis comment header packet type incorrect ({packetType} instead of {PacketType}).");
            }

            var vorbis = dataReader.ReadBytes(6);
            if (!vorbis.SequenceEqual(VorbisHeader.Vorbis))
            {
                
            }

            var vendorLength = dataReader.ReadUInt32();
            var vendorString = Encoding.UTF8.GetString(dataReader.ReadBytes((int)vendorLength));
            var userCommentListLength = dataReader.ReadUInt32();

            for (var userCommentIndex = 0; userCommentIndex < userCommentListLength; userCommentIndex++)
            {
                var userCommentLength = dataReader.ReadUInt32();
                var userComment = Encoding.UTF8.GetString(dataReader.ReadBytes((int)userCommentLength));
            }

            var framingBit = dataReader.ReadByte() << 7;

            throw new NotImplementedException();
        }

        private readonly List<Tuple<string, string>> _userComments = new List<Tuple<string, string>>();

        public IReadOnlyCollection<Tuple<string, string>> UserComments;

        public void AddUserComment(string key, string value)
        {
            foreach (var ch in key)
            {
                if (ch < 0x20 || ch > 0x7D || ch == 0x3D)
                {
                    throw new VorbisException($"Invalid character \"{ch}\" in comment key.");
                }
            }

            _userComments.Add(new Tuple<string, string>(key, value));
        }
    }
}