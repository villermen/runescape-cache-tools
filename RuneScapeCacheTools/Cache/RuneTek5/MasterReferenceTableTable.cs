namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    public class MasterReferenceTableTable
    {
        public MasterReferenceTableTable(Index index)
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