namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    ///     Represents a child entry within an <see cref="ReferenceTableFile" /> in the <see cref="ReferenceTable" />.
    /// </summary>
    public class ReferenceTableFileEntry
    {
        public ReferenceTableFileEntry(int id)
        {
            Id = id;
        }

        /// <summary>
        ///     This entry's identifier.
        /// </summary>
        public int Identifier { get; set; } = -1;

        /// <summary>
        ///     The entry's id.
        /// </summary>
        public int Id { get; set; }
    }
}