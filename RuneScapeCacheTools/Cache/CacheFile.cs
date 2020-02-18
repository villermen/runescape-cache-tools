namespace Villermen.RuneScapeCacheTools.File
{
    public class CacheFile
    {
        public byte[] Data { get; set; }

        public CacheFile(byte[] data)
        {
            this.Data = data;
        }
    }
}
