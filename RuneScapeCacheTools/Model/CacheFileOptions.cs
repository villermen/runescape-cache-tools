using System;
using Villermen.RuneScapeCacheTools.File;

namespace Villermen.RuneScapeCacheTools.Model
{
    [Flags]
    public enum CacheFileOptions
    {
        /// <summary>
        ///     A flag which indicates this <see cref="ReferenceTableFile" /> contains Djb2 hashed identifiers.
        /// </summary>
        Identifiers = 1,

        /// <summary>
        ///     A flag which indicates this <see cref="ReferenceTableFile" />} contains whirlpool digests for its entries.
        /// </summary>
        WhirlpoolDigests = 2,

        /// <summary>
        ///     A flag which indicates this <see cref="ReferenceTableFile" /> contains sizes for its entries.
        /// </summary>
        Sizes = 4,

        /// <summary>
        ///     A flag which indicates this <see cref="ReferenceTableFile" /> contains some kind of hash which is currently unused by
        ///     the RuneScape client.
        /// </summary>
        MysteryHashes = 8
    }
}
