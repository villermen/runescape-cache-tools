using System;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Audio.Vorbis;
using Xunit;
using Xunit.Abstractions;

namespace RuneScapeCacheToolsTests
{
    public class VorbisTests : IDisposable
    {
        private ITestOutputHelper Output { get; }

        private VorbisReader Reader1 { get; }
        private VorbisReader Reader2 { get; }
        private VorbisWriter Writer { get; }

        public VorbisTests(ITestOutputHelper output)
        {
            Output = output;

            Reader1 = new VorbisReader(File.OpenRead("testdata/sample1.ogg"));
            Reader2 = new VorbisReader(File.OpenRead("testdata/sample2.ogg"));

            Writer = new VorbisWriter(File.OpenWrite("out.ogg"));
        }

        [Fact]
        public void TestReadComments()
        {
            Reader1.ReadPacket();
            var commentPacket = Reader1.ReadPacket();

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
        public void TestCombineSamples()
        {
            VorbisPacket packet;
            while ((packet = Reader1.ReadPacket()) != null)
            {
                Writer.WritePacket(packet);
            }
        }

        public void Dispose()
        {
            Reader1?.Dispose();
            Reader2?.Dispose();
            Writer?.Dispose();
        }
    }
}