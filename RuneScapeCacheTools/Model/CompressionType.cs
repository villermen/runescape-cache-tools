namespace Villermen.RuneScapeCacheTools.Model
{
    public enum CompressionType
    {
        None = 0x00,
        Bzip2 = 0x01,
        Gzip = 0x02,
        Lzma = 0x03,
        Zlib = 0x5A, // Is actually the start of "ZLB".
    }
}
