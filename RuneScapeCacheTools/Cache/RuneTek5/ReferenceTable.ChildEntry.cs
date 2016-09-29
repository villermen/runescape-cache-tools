namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    public partial class ReferenceTable
    {
        /// <summary>
        ///     Represents a child entry within an <see cref="Entry" /> in the <see cref="ReferenceTable" />.
        /// </summary>
        public class ChildEntry
        {
            public ChildEntry(int index)
            {
                Index = index;
            }

            /// <summary>
            ///     This entry's identifier.
            /// </summary>
            public int Identifier { get; set; } = -1;

            /// <summary>
            ///     The cache index of this entry.
            /// </summary>
            public int Index { get; set; }
        }
    }
}