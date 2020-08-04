namespace Villermen.RuneScapeCacheTools.Model
{
    /// <summary>
    /// Represents metadata of an entry within a <see cref="CacheFileInfo" />.
    /// </summary>
    public class CacheFileEntryInfo
    {
        /// <summary>
        /// This entry's identifier.
        /// </summary>
        public int? Identifier { get; set; }

        /// <summary>
        /// Returns a copy of this object with the same values.
        /// </summary>
        public CacheFileEntryInfo Clone()
        {
            return new CacheFileEntryInfo
            {
                Identifier = this.Identifier
            };
        }
    }
}
