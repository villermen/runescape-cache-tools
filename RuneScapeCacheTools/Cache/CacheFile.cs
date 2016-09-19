using System.Linq;

namespace Villermen.RuneScapeCacheTools.Cache
{
    public class CacheFile
    {
        public CacheFile(int indexId, int fileId, byte[][] data, int version)
        {
            Data = data;
        }

        public bool IsArchive => Data.Length > 1;

        public int IndexId { get; }

        public int FileId { get; }

        public byte[][] Data { get; }
    }
}