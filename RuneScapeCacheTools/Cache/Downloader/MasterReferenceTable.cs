using System.Collections.Generic;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    /// <summary>
    /// A master reference table holds information on the other reference tables. As far as I know this concept only
    /// exists for the downloader.
    /// </summary>
    public class MasterReferenceTable
    {
        public static MasterReferenceTable Decode(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));

            var referenceTableInfos = new Dictionary<CacheIndex, ReferenceTableInfo>();

            var tableCount = reader.ReadByte();
            for (var tableId = 0; tableId < tableCount; tableId++)
            {
                var index = (CacheIndex)tableId;

                var table = new ReferenceTableInfo
                {
                    Crc = reader.ReadInt32BigEndian(),
                    Version = reader.ReadInt32BigEndian(),
                    FileCount = reader.ReadInt32BigEndian(),
                    Length = reader.ReadInt32BigEndian(),
                    WhirlpoolDigest = reader.ReadBytes(64)
                };

                referenceTableInfos.Add(index, table);
            }

            var rsaEncryptedWhirlpoolDigest = reader.ReadBytes(512);

            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                throw new DecodeException($"Not all bytes read while decoding master reference table. {reader.BaseStream.Length - reader.BaseStream.Position} bytes remain.");
            }

            return new MasterReferenceTable
            {
                ReferenceTableInfos = referenceTableInfos,
                RsaEncryptedWhirlpoolDigest = rsaEncryptedWhirlpoolDigest,
            };
        }

        public Dictionary<CacheIndex, ReferenceTableInfo> ReferenceTableInfos { get; private set; } = new Dictionary<CacheIndex, ReferenceTableInfo>();

        /// <summary>
        /// I don't know what this is used for either but we don't need to verify or generate it ourselves anyway.
        /// </summary>
        public byte[] RsaEncryptedWhirlpoolDigest { get; set; }

        /// <summary>
        /// Returns indexes that have reference tables available (have at least one file stored in them).
        /// </summary>
        public IEnumerable<CacheIndex> AvailableReferenceTables => this.ReferenceTableInfos
            .Where(infoPair => infoPair.Value.FileCount > 0)
            .Select(infoPair => infoPair.Key);
    }
}
