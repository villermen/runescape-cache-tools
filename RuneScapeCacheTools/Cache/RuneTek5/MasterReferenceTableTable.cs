namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    public class MasterReferenceTableTable
    {
        public MasterReferenceTableTable(int indexId)
        {
            IndexId = indexId;
        }

        public int IndexId { get; set; }

        public int CRC { get; set; }

        public byte[] WhirlpoolDigest { get; set; }

        public int Version { get; set; }

        public int FileCount { get; set; }

        public int Length { get; set; }
    }
}