using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Extension;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.File
{
    /// <summary>
    ///     A master reference table holds information on the other reference tables.
    ///     This is stored in a separate class, as the
    /// </summary>
    public class MasterReferenceTableFile : CacheFile
    {
        public Dictionary<Index, ReferenceTableInfo> ReferenceTables { get; } = new Dictionary<Index, ReferenceTableInfo>();

        public byte[] RsaEncryptedWhirlpoolDigest { get; set; }

        public Dictionary<Index, ReferenceTableInfo> GetAvailableReferenceTables() => this.ReferenceTables
            .Where(infoPair => infoPair.Value.FileCount > 0)
            .ToDictionary(infoPair => infoPair.Key, infoPair => infoPair.Value);

        public override void Decode(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));

            var tableCount = reader.ReadByte();

            for (var tableId = 0; tableId < tableCount; tableId++)
            {
                var index = (Index)tableId;

                var table = new ReferenceTableInfo(index)
                {
                    CRC = reader.ReadInt32BigEndian(),
                    Version = reader.ReadInt32BigEndian(),
                    FileCount = reader.ReadInt32BigEndian(),
                    Length = reader.ReadInt32BigEndian(),
                    WhirlpoolDigest = reader.ReadBytes(64)
                };

                this.ReferenceTables.Add(index, table);
            }

            this.RsaEncryptedWhirlpoolDigest = reader.ReadBytes(512);

            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                throw new DecodeException($"Not all bytes read while decoding master reference table. {reader.BaseStream.Length - reader.BaseStream.Position} bytes remain.");
            }
        }

        public override byte[] Encode()
        {
            throw new NotImplementedException("Encoding of master reference table is not implemented. AFAIK it's a downloader only thing.");
        }

        public class ReferenceTableInfo
        {
            public ReferenceTableInfo(Index index)
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
