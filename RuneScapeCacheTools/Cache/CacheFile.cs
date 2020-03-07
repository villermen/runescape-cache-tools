namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    /// A file in the cache containing (decoded) binary data.
    /// </summary>
    public class CacheFile
    {
        public byte[] Data { get; set; }

        public CacheFile(byte[] data)
        {
            this.Data = data;
        }
    }
}
