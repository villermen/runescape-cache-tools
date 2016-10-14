using System;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Audio.Ogg;
using Villermen.RuneScapeCacheTools.Audio.Vorbis;
using Xunit;
using Xunit.Abstractions;

namespace RuneScapeCacheToolsTests
{
    public class OggTests : IDisposable
    {
        private ITestOutputHelper Output { get; }

        private OggReader Reader1 { get; }
        private OggReader Reader2 { get; }
        private OggWriter Writer { get; }

        public OggTests(ITestOutputHelper output)
        {
            Output = output;

            Reader1 = new OggReader(File.OpenRead("testdata/sample1.ogg"));
            Reader2 = new OggReader(File.OpenRead("testdata/sample2.ogg"));

            Writer = new OggWriter(File.OpenWrite("out.ogg"));
        }

        [Fact]
        public void TestReadComments()
        {
            Reader1.ReadVorbisPacket();
            var commentPacket = Reader1.ReadVorbisPacket();

            Output.WriteLine($"Type of packet: {commentPacket.GetType().FullName}");

            Assert.IsType<VorbisCommentHeader>(commentPacket);

            var commentHeader = (VorbisCommentHeader)commentPacket;

            Output.WriteLine("Comments in header:");
            foreach (var userComment in commentHeader.UserComments)
            {
                Output.WriteLine($" - {userComment.Item1}: {userComment.Item2}");
            }

            Assert.True(commentHeader.UserComments.Contains(new Tuple<string, string>("DATE", "2012")));
        }

        [Fact]
        public void TestTransferOggPackets()
        {
            OggPacket oggPacket;
            while ((oggPacket = Reader1.ReadOggPacket()) != null)
            {
                Writer.WriteOggPacket(oggPacket);
            }

            // TODO: Doesn't work if creating lacing values ourselves. Why?
        }

        public void Dispose()
        {
            Reader1?.Dispose();
            Reader2?.Dispose();
            Writer?.Dispose();
        }
    }
}