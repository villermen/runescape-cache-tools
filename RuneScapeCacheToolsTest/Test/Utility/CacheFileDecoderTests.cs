using System;
using System.Collections.Generic;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Test.Fixture;
using Villermen.RuneScapeCacheTools.Utility;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Test.Utility
{
    [Collection(TestCacheCollection.Name)]
    public class CacheFileDecoderTests : BaseTests
    {
        private TestCacheFixture Fixture { get; }

        private readonly Dictionary<Type, ICacheFileDecoder> _fileDecoders = new Dictionary<Type, ICacheFileDecoder>();

        public CacheFileDecoderTests(TestCacheFixture fixture)
        {
            this.Fixture = fixture;

            this._fileDecoders.Add(typeof(RuneTek5CacheFileDecoder), new RuneTek5CacheFileDecoder());
            this._fileDecoders.Add(typeof(RuneTek7CacheFileDecoder), new RuneTek7CacheFileDecoder());
        }

        [Theory]
        [InlineData(typeof(RuneTek5CacheFileDecoder), CompressionType.None)]
        [InlineData(typeof(RuneTek5CacheFileDecoder), CompressionType.Bzip2)]
        [InlineData(typeof(RuneTek5CacheFileDecoder), CompressionType.Gzip)]
        // TODO: [InlineData(typeof(RuneTek5CacheFileDecoder), CompressionType.Lzma)]
        [InlineData(typeof(RuneTek7CacheFileDecoder), CompressionType.Zlib)]
        [InlineData(typeof(RuneTek7CacheFileDecoder), CompressionType.Bzip2)]
        public void TestEncodeDecode(Type decoderType, CompressionType compressionType)
        {
            var fileDecoder = this._fileDecoders[decoderType];

            var data = new byte[] { 0x41, 0x20, 0x71, 0x20, 0x70, 0x0A, 0x2E, 0x20, 0x20, 0x20, 0x20, 0x77 };

            var file = new CacheFile(data);
            var encodedData = fileDecoder.EncodeFile(file, new CacheFileInfo
            {
                CompressionType = compressionType
            });
            var decodedFile = fileDecoder.DecodeFile(encodedData, new CacheFileInfo());

            Assert.Equal(data, decodedFile.Data);
        }

        [Theory]
        [InlineData(typeof(RuneTek5CacheFileDecoder))]
        [InlineData(typeof(RuneTek7CacheFileDecoder))]
        public void TestEncodeDecodeEntries(Type decoderType)
        {
            var fileDecoder = this._fileDecoders[decoderType];

            var entry5 = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var entry10 = new byte[] { 0x05, 0x06, 0x07 };

            var file = new CacheFile();
            file.Entries[5] = entry5;
            file.Entries[10] = entry10;

            var info = new CacheFileInfo();
            var fileData = fileDecoder.EncodeFile(file, info);

            // Info is updated by encoding.
            Assert.Equal(new [] { 5, 10 }, info.Entries.Keys);

            var decodedFile = fileDecoder.DecodeFile(fileData, info);

            Assert.Equal(entry5, decodedFile.Entries[5]);
            Assert.Equal(entry10, decodedFile.Entries[10]);
        }

        [Fact]
        public void TestEncodeDecodeFromCache()
        {
            const int entryCount = 256;

            var file = this.Fixture.JavaClientCache.GetFile(CacheIndex.ItemDefinitions, 155);
            Assert.Equal(entryCount, file.Info.Entries.Count);
            Assert.Equal(entryCount, file.Entries.Count);

            // Encode and decode and check if the entries stay the same. We don't want to compare compressed data.
            var info = file.Info.Clone();
            var encodedFileData = this.Fixture.JavaClientCache.FileDecoder.EncodeFile(file, info);
            var decodedFile = this.Fixture.JavaClientCache.FileDecoder.DecodeFile(encodedFileData, info);

            Assert.Equal(file.Entries.Keys, decodedFile.Entries.Keys);
            foreach (var entryPair in file.Entries)
            {
                Assert.Equal(entryPair.Value, decodedFile.Entries[entryPair.Key]);
            }
        }
    }
}
