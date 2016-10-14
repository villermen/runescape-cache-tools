using System;
using System.Collections.Generic;
using System.IO;

namespace Villermen.RuneScapeCacheTools.Audio.Vorbis
{
    /// <summary>
    ///     Due to bit order, this algorithm behaves slightly different from the default CRC32 implementation.
    /// </summary>
    internal class VorbisCrc : Stream
    {
        private const uint polynomial = 0x04c11db7;
        private static readonly uint[] table = new uint[256];

        static VorbisCrc()
        {
            for (uint i = 0; i < 256; i++)
            {
                var s = i << 24;
                for (var j = 0; j < 8; ++j)
                {
                    s = (s << 1) ^ (s >= 1U << 31 ? VorbisCrc.polynomial : 0);
                }
                VorbisCrc.table[i] = s;
            }
        }

        public uint Value { get; private set; }

        public void Update(byte value)
        {
            Value = (Value << 8) ^ VorbisCrc.table[value ^ (Value >> 24)];
        }

        public void Update(IEnumerable<byte> data)
        {
            foreach (var value in data)
            {
                Update(value);
            }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            for (var i = offset; i < offset + count; i++)
            {
                Update(buffer[i]);
            }
        }

        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite { get; } = true;
        public override long Length { get; }
        public override long Position { get; set; }
    }
}