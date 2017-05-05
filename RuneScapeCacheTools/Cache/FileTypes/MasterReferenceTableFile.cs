using System;
using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.FileTypes
{
    /// <summary>
    ///     A master reference table holds information on the other reference tables.
    ///     This is stored in a separate class, as the
    /// </summary>
    public class MasterReferenceTableFile : CacheFile
    {
        public IDictionary<Index, Entry> ReferenceTableFiles { get; } = new Dictionary<Index, Entry>();

        public byte[] RsaEncryptedWhirlpoolDigest { get; set; }

        public override void Decode(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));

            var tableCount = reader.ReadByte();

            for (var tableId = 0; tableId < tableCount; tableId++)
            {
                var index = (Index)tableId;

                var table = new Entry(index)
                {
                    CRC = reader.ReadInt32BigEndian(),
                    Version = reader.ReadInt32BigEndian(),
                    FileCount = reader.ReadInt32BigEndian(),
                    Length = reader.ReadInt32BigEndian(),
                    WhirlpoolDigest = reader.ReadBytes(64)
                };

                this.ReferenceTableFiles.Add(index, table);
            }

            this.RsaEncryptedWhirlpoolDigest = reader.ReadBytes(512);
        }

        public override byte[] Encode()
        {
            throw new NotImplementedException("Encoding of master reference table is not implemented. AFAIK it's a downloader only thing.");
        }

        public class Entry
        {
            public Entry(Index index)
            {
                this.Index = index;
            }

            public int CRC { get; set; }

            public int FileCount { get; set; }

            public Index Index { get; set; }

            public int Length { get; set; }

            public int Version { get; set; }

            public byte[] WhirlpoolDigest { get; set; }
        }
    }
}