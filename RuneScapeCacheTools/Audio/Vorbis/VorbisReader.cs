using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    public class VorbisReader : IDisposable
    {
        public VorbisReader(string filePath)
        {
            _filePath = filePath;

            var i = ReadPage();
            var c = ReadPage();
            var s = ReadPage();
        }

        private readonly string _filePath;

        private Stream _dataStream;

        private Stream DataStream => _dataStream ?? (_dataStream = File.OpenRead(_filePath));

        private int NextPageSequenceNumber { get; set; }

        private VorbisPacket ReadPacket()
        {
            // TODO: Read pages till a full packet is obtained
            var packetData = new byte[0];

            var packetType = packetData[0];

            switch (packetType)
            {
                // Identification header
                case VorbisIdentificationHeader.PacketType:
                    return VorbisIdentificationHeader.Decode(packetData);
                    break;

                // Comment header
                case VorbisCommentHeader.PacketType:
                    return VorbisCommentHeader.Decode(packetData);
                    break;

                // Setup header
                case VorbisSetupHeader.PacketType:
                    return VorbisSetupHeader.Decode(packetData);
                    break;

                // Audio packet
                default:
                    return VorbisAudioPacket.Decode(packetData);
                    break;
            }
        }

        private VorbisPage ReadPage()
        {
            return VorbisPage.Decode(DataStream);
        }

        public void Dispose()
        {
            DataStream?.Dispose();
        }
    }
}