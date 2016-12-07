using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    ///     A master reference table holds information on the other reference tables.
    ///     This is stored in a separate class, as the
    /// </summary>
    public class MasterReferenceTable
    {
        public MasterReferenceTable(CacheFile cacheFile)
        {
            var reader = new BinaryReader(new MemoryStream(cacheFile.Data));

            var tableCount = reader.ReadByte();

            for (var tableId = 0; tableId < tableCount; tableId++)
            {
                var index = (Index)tableId;

                var table = new MasterReferenceTableTable(index)
                {
                    CRC = reader.ReadInt32BigEndian(),
                    Version = reader.ReadInt32BigEndian(),
                    FileCount = reader.ReadInt32BigEndian(),
                    Length = reader.ReadInt32BigEndian(),
                    WhirlpoolDigest = reader.ReadBytes(64)
                };

                ReferenceTableFiles.Add(index, table);
            }

            RSAEncryptedWhirlpoolDigest = reader.ReadBytes(512);
        }

        public IDictionary<Index, MasterReferenceTableTable> ReferenceTableFiles { get; } = new Dictionary<Index, MasterReferenceTableTable>();

        public byte[] RSAEncryptedWhirlpoolDigest { get; set; }
    }
}