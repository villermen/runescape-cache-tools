using System;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    [Flags]
    public enum ReferenceTableOptions
    {
        /// <summary>
        /// A flag which indicates this <see cref="ReferenceTable" /> contains Djb2 hashed identifiers.
        /// </summary>
        Identifiers = 1,

        /// <summary>
        /// A flag which indicates this <see cref="ReferenceTable" />} contains whirlpool digests for its entries.
        /// </summary>
        WhirlpoolDigests = 2,

        /// <summary>
        /// A flag which indicates this <see cref="ReferenceTable" /> contains sizes for its entries.
        /// </summary>
        Sizes = 4,

        /// <summary>
        /// A flag which indicates this <see cref="ReferenceTable" /> contains some kind of hash which is currently
        /// unused by the RuneScape client.
        /// </summary>
        MysteryHashes = 8
    }
}
