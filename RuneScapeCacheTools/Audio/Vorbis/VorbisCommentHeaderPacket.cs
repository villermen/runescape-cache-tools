using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    internal class VorbisCommentHeaderPacket : VorbisHeaderPacket
    {
        public const byte PacketType = 0x03;

        public VorbisCommentHeaderPacket()
        {
            UserComments = new ReadOnlyCollection<Tuple<string, string>>(_userComments);
        }

        public static VorbisCommentHeaderPacket Decode(byte[] packetData)
        {
            var packetStream = new MemoryStream(packetData);
            var packetReader = new BinaryReader(packetStream);

            VerifyHeaderSignature(packetStream, PacketType);

            var vendorLength = packetReader.ReadUInt32();
            var vendorString = Encoding.UTF8.GetString(packetReader.ReadBytes((int)vendorLength));
            var userCommentListLength = packetReader.ReadUInt32();

            for (var userCommentIndex = 0; userCommentIndex < userCommentListLength; userCommentIndex++)
            {
                var userCommentLength = packetReader.ReadUInt32();
                var userComment = Encoding.UTF8.GetString(packetReader.ReadBytes((int)userCommentLength));
            }

            var framingBit = packetReader.ReadByte() & 0x01;

            if (packet.FramingFlag != 1)
            {
                throw new VorbisException("Framing flag should be 1 but is 0.");
            }
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