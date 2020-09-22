using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Utility
{
    public interface ICacheFileDecoder
    {
        /// <summary>
        /// Decodes the data into a <see cref="CacheFile" />. If info is supplied it will be used to verify the decoded
        /// file's properties. The updated info will be set on the file itself.
        /// </summary>
        CacheFile DecodeFile(byte[] encodedData, CacheFileInfo? info);

        /// <summary>
        /// Encodes the given file into binary data. If info is supplied it will be used in the encoding process. Info
        /// will be updated with changed checksums etc.
        /// </summary>
        byte[] EncodeFile(CacheFile file, CacheFileInfo? info);
    }
}
