using System.Collections.Generic;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    public partial class ReferenceTable
    {
        /// <summary>
        ///     Represents a single entry within a <see cref="ReferenceTable" />.
        /// </summary>
        public class Entry
        {
            public Entry(int index)
            {
                Index = index;
            }

            /// <summary>
            ///     Some unknown hash added on build 816.
            ///     It is hard to pinpoint what exactly this is because it is not used in the client.
            /// </summary>
            public int MysteryHash { get; set; }

            /// <summary>
            ///     The compressed size of this entry.
            /// </summary>
            public int CompressedSize { get; set; }

            /// <summary>
            ///     The uncompressed size of this entry.
            /// </summary>
            public int UncompressedSize { get; set; }

            /// <summary>
            ///     The identifier of this entry.
            /// </summary>
            public int Identifier { get; set; } = -1;

            /// <summary>
            ///     The CRC32 checksum of this entry.
            /// </summary>
            public int CRC { get; set; }

            /// <summary>
            ///     The whirlpool digest of this entry.
            /// </summary>
            public byte[] Whirlpool { get; set; }

            /// <summary>
            ///     The version of this entry stored as the time the file was last edited in seconds since the unix epoch.
            /// </summary>
            public int Version { get; set; }

            /// <summary>
            ///     The cache index of this entry
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            ///     The children in this entry.
            /// </summary>
            public IDictionary<int, ChildEntry> ChildEntries { get; } = new SortedDictionary<int, ChildEntry>();
        }
    }
}