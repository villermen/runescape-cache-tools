using System;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Audio.Vorbis;
using Xunit;

namespace RuneScapeCacheToolsTests
{
    public class VorbisTests : IDisposable
    {
        private VorbisReader Reader { get; }
        private VorbisWriter Writer { get; }

        public VorbisTests()
        {
            Reader = new VorbisReader(File.OpenRead("C:\\local\\temp\\Soundscape.ogg"));
            Writer = new VorbisWriter(File.OpenWrite("C:\\local\\temp\\Out.ogg"));
        }

        [Fact(Skip = "No files provided yet.")]
        public void TestReadComments()
        {
            Reader.ReadPacket();
            var commentPacket = Reader.ReadPacket();

            Assert.IsType<VorbisCommentHeader>(commentPacket);

            var commentHeader = (VorbisCommentHeader)commentPacket;

            Assert.True(commentHeader.UserComments.Contains(new Tuple<string, string>("EXTRACTED_BY", "Villers RuneScape Cache Tools")));
        } 

        public void Dispose()
        {
            Reader?.Dispose();
            Writer?.Dispose();
        }
    }
}