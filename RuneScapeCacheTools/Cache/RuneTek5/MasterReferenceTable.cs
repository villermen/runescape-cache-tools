using System;
using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    ///     A master reference table holds information on the other reference tables.
    ///     This is stored in a separate class, as the
    /// </summary>
    public class MasterReferenceTable : CacheFile
    {
        public IDictionary<Index, MasterReferenceTableEntry> ReferenceTableFiles { get; } = new Dictionary<Index, MasterReferenceTableEntry>();

        public byte[] RsaEncryptedWhirlpoolDigest { get; set; }

        protected override void Decode(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));

            var tableCount = reader.ReadByte();

            for (var tableId = 0; tableId < tableCount; tableId++)
            {
                var index = (Index)tableId;

                var table = new MasterReferenceTableEntry(index)
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

        protected override byte[] Encode()
        {
            throw new NotImplementedException("Encoding of master reference table is not implemented. AFAIK it's a downloader only thing.");
        }
    }
}