namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    ///     Represents metadata of an entry within a <see cref="CacheFileInfo" />.
    /// </summary>
    public class CacheFileEntryInfo
    {
        /// <summary>
        ///     The entry's id.
        /// </summary>
        public int EntryId { get; set; } = -1;

        /// <summary>
        ///     This entry's identifier.
        /// </summary>
        public int Identifier { get; set; } = -1;
    }
}