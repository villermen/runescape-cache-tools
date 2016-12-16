namespace RuneScapeCacheToolsTests
{
    using System.IO;
    using System.Runtime.InteropServices;
    using Villermen.RuneScapeCacheTools.Cache;
    using Villermen.RuneScapeCacheTools.Extensions;
    using Xunit;

    public class BinaryStreamExtensionsTests
    {
        [Fact]
        public void TestRead()
        {
            var d = 0xe20e5d01 & 0x7fffffff;

            var stream = new MemoryStream();

            // Write some known data
            stream.Write(new byte[] { 0xE2, 0x0E, 0x5D, 0x01, 0xB7, 0x71 }, 0 , 6);

            var reader = new BinaryReader(stream);

            stream.Position = 0L;
            Assert.Equal(-7666, reader.ReadInt16BigEndian());

            stream.Position = 0L;
            Assert.Equal(57870, reader.ReadUInt16BigEndian());

            stream.Position = 0L;
            Assert.Equal(14814813, reader.ReadUInt24BigEndian());

            stream.Position = 0L;
            Assert.Equal(3792592129, reader.ReadUInt32BigEndian());

            stream.Position = 0L;
            Assert.Equal(-502375167, reader.ReadInt32BigEndian());

            stream.Position = 0L;
            Assert.Equal(248551317813105, reader.ReadUInt48BigEndian());

            stream.Position = 0L;
            Assert.Equal(1645108481, reader.ReadAwkwardInt());
            Assert.Equal(4, stream.Position);

            stream.Position = 1L;
            Assert.Equal(3677, reader.ReadAwkwardInt());
            Assert.Equal(3, stream.Position);
        }

        [Fact]
        public void TestWrite()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            var reader = new BinaryReader(stream);

            stream.Position = 0L;
            writer.WriteInt16BigEndian(-23498);
            stream.Position = 0L;
            Assert.Equal(-23498, reader.ReadInt16BigEndian());

            stream.Position = 0L;
            writer.WriteUInt16BigEndian(23498);
            stream.Position = 0L;
            Assert.Equal(23498, reader.ReadInt16BigEndian());

            stream.Position = 0L;
            writer.WriteUInt24BigEndian(23498);
            stream.Position = 0L;
            Assert.Equal(23498, reader.ReadUInt24BigEndian());

            stream.Position = 0L;
            writer.WriteInt32BigEndian(-23498);
            stream.Position = 0L;
            Assert.Equal(-23498, reader.ReadInt32BigEndian());

            stream.Position = 0L;
            writer.WriteUInt32BigEndian(23498);
            stream.Position = 0L;
            Assert.Equal((uint)23498, reader.ReadUInt32BigEndian());

            stream.Position = 0L;
            writer.WriteAwkwardInt(23498);
            stream.Position = 0L;
            Assert.Equal(23498, reader.ReadAwkwardInt());

            stream.Position = 0L;
            writer.WriteAwkwardInt(-1);
            stream.Position = 0L;
            Assert.Equal(-1, reader.ReadAwkwardInt());

            stream.Position = 0L;
            writer.WriteAwkwardInt(998234832);
            stream.Position = 0L;
            Assert.Equal(998234832, reader.ReadAwkwardInt());
        }
    }
}