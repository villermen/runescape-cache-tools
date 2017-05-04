namespace Villermen.RuneScapeCacheTools.Cache.FileTypes
{
    /// <summary>
    /// Base type for all cache files.
    /// Child types must implement methods to convert to and from bytes.
    /// </summary>
    public abstract class CacheFile
    {
        public CacheFileInfo Info { get; set; }

        public void FromBinaryFile(BinaryFile binaryFile)
        {
            this.Info = binaryFile.Info;

            this.Decode(binaryFile.Data);
        }

        protected abstract void Decode(byte[] data);

        public BinaryFile ToBinaryFile()
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