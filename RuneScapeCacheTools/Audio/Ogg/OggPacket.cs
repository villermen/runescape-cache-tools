using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Villermen.RuneScapeCacheTools.Audio.Ogg
{
    /// <summary>
    ///     A collection of pages representing a single Ogg packet.
    /// </summary>
    public class OggPacket : Stream
    {
        public OggPacket()
        {
            Pages = new List<OggPage>();
        }

        public OggPacket(IEnumerable<OggPage> pages)
        {
            Pages = pages.ToList();
        }

        public IList<OggPage> Pages { get; }

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
            var readCount = count;
            var pageStartPosition = 0;

            foreach (var page in Pages)
            {
                if (readCount == count)
                {
                    break;
                }

                var endPosition = pageStartPosition + page.Data.Length;

                if (Position < endPosition)
                {
                    var readStartPosition = Position - pageStartPosition;
                    var bytesToRead = (int)Math.Min(endPosition - readStartPosition, count - readCount);

                    Array.Copy(page.Data, readStartPosition, buffer, readCount, bytesToRead);

                    readCount += bytesToRead;
                    Position += bytesToRead;
                }

                pageStartPosition = endPosition;
            }

            return readCount;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead { get; } = true;
        public override bool CanSeek { get; }
        public override bool CanWrite { get; }
        public override long Length => Pages.Aggregate(0, (current, page) => current + page.Data.Length);
        public override long Position { get; set; }
    }
}