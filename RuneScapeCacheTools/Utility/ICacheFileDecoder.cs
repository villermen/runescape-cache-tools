using System.Collections.Generic;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Utility
{
    // TODO: Summaries.
    public interface ICacheFileDecoder
    {
        CacheFile DecodeFile(byte[] encodedData, CacheFileInfo? info);

        SortedDictionary<int, byte[]> DecodeEntries(byte[] data, int[] entryIds);

        byte[] EncodeFile(CacheFile file);

        byte[] EncodeEntries(SortedDictionary<int, byte[]> entries, CacheFileInfo info);
    }
}
