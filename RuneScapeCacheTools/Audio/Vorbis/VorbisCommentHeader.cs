using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    public class VorbisCommentHeader : VorbisHeader
    {
        public const byte PacketType = 0x03;

        public VorbisCommentHeader()
        {
            UserComments = new ReadOnlyCollection<Tuple<string, string>>(_userComments);
        }

        public static VorbisCommentHeader Decode(byte[] packetData)
        {
            var packetStream = new MemoryStream(packetData);
            var packetReader = new BinaryReader(packetStream);

            var packet = new VorbisCommentHeader();

            packet.DecodeHeader(packetStream, VorbisCommentHeader.PacketType);

            var vendorLength = packetReader.ReadUInt32();
            packet.VendorString = Encoding.UTF8.GetString(packetReader.ReadBytes((int)vendorLength));

            var userCommentListLength = packetReader.ReadUInt32();
            for (var userCommentIndex = 0; userCommentIndex < userCommentListLength; userCommentIndex++)
            {
                var userCommentLength = packetReader.ReadUInt32();
                var userComment = Encoding.UTF8.GetString(packetReader.ReadBytes((int)userCommentLength));

                var userCommentSeparatorPosition = userComment.IndexOf((char)0x3D);
                if (userCommentSeparatorPosition == -1)
                {
                    throw new VorbisException("No user comment separator (=) found in user comment.");
                }

                packet.AddUserComment(userComment.Substring(0, userCommentSeparatorPosition), userComment.Substring(userCommentSeparatorPosition + 1));
            }

            var framingBit = packetReader.ReadByte() & 0x01;

            if (framingBit != 1)
            {
                throw new VorbisException("Framing bit should be 1 but is 0.");
            }

            return packet;
        }

        public string VendorString { get; private set; }

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

        public override void Encode(Stream stream)
        {
            EncodeHeader(stream);

            var packetWriter = new BinaryWriter(stream);
            packetWriter.Write((uint)VendorString.Length);
            packetWriter.Write(Encoding.UTF8.GetBytes(VendorString));
            packetWriter.Write((uint)UserComments.Count);
            foreach (var userComment in UserComments)
            {
                packetWriter.Write(Encoding.ASCII.GetBytes(userComment.Item1));
                packetWriter.Write(0x3D);
                packetWriter.Write(Encoding.UTF8.GetBytes(userComment.Item2));
            }
            packetWriter.Write((byte)1);
        }
    }
}