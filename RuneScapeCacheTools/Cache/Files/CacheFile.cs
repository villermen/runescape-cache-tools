namespace Villermen.RuneScapeCacheTools.Cache.Files
{
    /// <summary>
    /// Base type for all cache files.
    /// Child types must implement methods to convert to and from bytes.
    /// </summary>
    public abstract class CacheFile
    {
        public CacheFileInfo Info { get; set; }

        public void FromDataFile(BinaryFile dataFile)
        {
            this.Info = dataFile.Info;

            this.Decode(dataFile.Data);
        }

        protected abstract void Decode(byte[] data);

        public BinaryFile ToDataFile()
        {
            return new BinaryFile
            {
                Info = this.Info,
                Data = this.Encode()
            };
        }

        protected abstract byte[] Encode();
    }
}