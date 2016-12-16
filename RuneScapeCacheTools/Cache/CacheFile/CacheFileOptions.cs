namespace Villermen.RuneScapeCacheTools.Cache.CacheFile
{
    using System;
    using Villermen.RuneScapeCacheTools.Cache.RuneTek5;

    [Flags]
    public enum CacheFileOptions
    {
        /// <summary>
        ///     A flag which indicates this <see cref="ReferenceTable" /> contains Djb2 hashed identifiers.
        /// </summary>
        Identifiers = 0x01,

        /// <summary>
        ///     A flag which indicates this <see cref="ReferenceTable" />} contains whirlpool digests for its entries.
        /// </summary>
        WhirlpoolDigests = 0x02,

        /// <summary>
        ///     A flag which indicates this <see cref="ReferenceTable" /> contains sizes for its entries.
        /// </summary>
        Sizes = 0x04,

        /// <summary>
        ///     A flag which indicates this <see cref="ReferenceTable" /> contains some kind of hash which is currently unused by
        ///     the RuneScape client.
        /// </summary>
        MysteryHashes = 0x08
    }
}