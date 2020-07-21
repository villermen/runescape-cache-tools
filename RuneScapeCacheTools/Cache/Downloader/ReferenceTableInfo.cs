namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    /// <summary>
    /// Contains information on a downloader-available reference table.
    /// </summary>
    public class ReferenceTableInfo
    {
        public int Crc { get; set; }

        public int FileCount { get; set; }

        public int Length { get; set; }

        public int Version { get; set; }

        public byte[] WhirlpoolDigest { get; set; }
    }
}
