namespace Villermen.RuneScapeCacheTools.Cache
{
    public abstract class CacheFile
    {
        public CacheFileInfo Info { get; set; }

        public void FromDataFile(DataCacheFile dataFile)
        {
            this.Info = dataFile.Info;

            this.Decode(dataFile.Data);
        }

        protected abstract void Decode(byte[] data);

        public DataCacheFile ToDataFile()
        {
            return new DataCacheFile
            {
                Info = this.Info,
                Data = this.Encode()
            };
        }

        protected abstract byte[] Encode();
    }
}