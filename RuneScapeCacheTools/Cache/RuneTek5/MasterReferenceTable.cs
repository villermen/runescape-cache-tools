using System.Collections.Generic;
using System.IO;
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

        public byte[] RSAEncryptedWhirlpoolDigest { get; set; }

        public static explicit operator MasterReferenceTable(DataCacheFile dataFile)
        {
            var masterFile = new MasterReferenceTable
            {
                Info = dataFile.Info
            };

            var reader = new BinaryReader(new MemoryStream(dataFile.Data));

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

                masterFile.ReferenceTableFiles.Add(index, table);
            }

            masterFile.RSAEncryptedWhirlpoolDigest = reader.ReadBytes(512);

            return masterFile;
        }
    }
}