namespace Villermen.RuneScapeCacheTools.Cache.FileTypes
{
    /// <summary>
    /// Base type for all cache files.
    /// Child types must implement methods to convert to and from bytes.
    /// </summary>
    public abstract class CacheFile
    {
        public CacheFileInfo Info { get; set; }

        public void FromFile(CacheFile file)
        {
            this.Info = file.Info;

            this.Decode(file.ToBinaryFile().Data);
        }

        public abstract void Decode(byte[] data);

        public BinaryFile ToBinaryFile()
        {
            var file = this as BinaryFile;

            return file ?? new BinaryFile
            {
                Info = this.Info,
                Data = this.Encode()
            };
        }

        public abstract byte[] Encode();
    }
}