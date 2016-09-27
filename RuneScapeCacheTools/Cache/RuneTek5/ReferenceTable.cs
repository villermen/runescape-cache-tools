using System;
using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    ///     A <see cref="ReferenceTable" /> holds metadata for all registered files in an index, such as checksums, versions
    ///     and archive members.
    ///     Note that the data of registered files does not have to be present in the index for them to be listed.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Sean</author>
    /// <author>Villermen</author>
    public class ReferenceTable
    {
        [Flags]
        public enum DataFlags
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

        /// <summary>
        ///     Decodes the reference table contained in the given RuneTek5CacheFile.
        /// </summary>
        /// <param name="runeTek5CacheFile"></param>
        /// <returns></returns>
        public ReferenceTable(RuneTek5CacheFile runeTek5CacheFile)
        {
            var reader = new BinaryReader(new MemoryStream(runeTek5CacheFile.Data));

            Format = reader.ReadByte();

            // Read header
            if (Format < 5 || Format > 7)
            {
                throw new CacheException($"Incorrect reference table protocol number: {Format}.");
            }

            if (Format >= 6)
            {
                Version = reader.ReadInt32BigEndian();
            }

            Flags = (DataFlags) reader.ReadByte();

            // Read the ids
            var ids = new int[Format >= 7 ? reader.ReadSmartInt() : reader.ReadUInt16BigEndian()];
            int accumulator = 0, size = -1;
            for (var i = 0; i < ids.Length; i++)
            {
                var delta = Format >= 7 ? reader.ReadSmartInt() : reader.ReadUInt16BigEndian();
                ids[i] = accumulator += delta;

                if (ids[i] > size)
                {
                    size = ids[i];
                }
            }

            size++;

            // Indices = ids;

            // Allocate specific entries within the ids array
            var index = 0;
            foreach (var id in ids)
            {
                Entries.Add(id, new Entry(index++));
            }

            // Read the identifiers if present
            if ((Flags & DataFlags.Identifiers) != 0)
            {
                foreach (var id in ids)
                {
                    Entries[id].Identifier = reader.ReadInt32BigEndian();
                }
            }

            // Read the CRC32 checksums
            foreach (var id in ids)
            {
                Entries[id].CRC = reader.ReadInt32BigEndian();
            }

            // Read some type of hash
            if ((Flags & DataFlags.MysteryHashes) != 0)
            {
                foreach (var id in ids)
                {
                    Entries[id].MysteryHash = reader.ReadInt32BigEndian();
                }
            }

            // Read the whirlpool digests if present
            if ((Flags & DataFlags.WhirlpoolDigests) != 0)
            {
                foreach (var id in ids)
                {
                    Entries[id].Whirlpool = reader.ReadBytes(64);
                }
            }

            // Read the compressed and uncompressed sizes
            if ((Flags & DataFlags.Sizes) != 0)
            {
                foreach (var id in ids)
                {
                    Entries[id].CompressedSize = reader.ReadInt32BigEndian();
                    Entries[id].UncompressedSize = reader.ReadInt32BigEndian();
                }
            }

            // Read the version numbers
            foreach (var id in ids)
            {
                var version = reader.ReadInt32BigEndian();
                Entries[id].Version = version;
            }

            // Read the child sizes
            var members = new int[size][];
            foreach (var id in ids)
            {
                members[id] = new int[Format >= 7 ? reader.ReadSmartInt() : reader.ReadUInt16BigEndian()];
            }

            // Read the child ids
            foreach (var id in ids)
            {
                // Reset the accumulator and size
                accumulator = 0;
                size = -1;

                // Loop through the array of ids
                for (var i = 0; i < members[id].Length; i++)
                {
                    var delta = Format >= 7 ? reader.ReadSmartInt() : reader.ReadUInt16BigEndian();
                    members[id][i] = accumulator += delta;
                    if (members[id][i] > size)
                    {
                        size = members[id][i];
                    }
                }

                // size++;

                // Allocate specific entries within the ids array
                index = 0;
                foreach (var child in members[id])
                {
                    Entries[id].ChildEntries.Add(child, new ChildEntry(index++));
                }
            }

            /* read the child identifiers if present */
            if ((Flags & DataFlags.Identifiers) != 0)
            {
                foreach (var id in ids)
                {
                    foreach (var child in members[id])
                    {
                        Entries[id].ChildEntries[child].Identifier = reader.ReadInt32BigEndian();
                    }
                }
            }
        }

        /// <summary>
        ///     The format of this table.
        /// </summary>
        public int Format { get; set; }

        /// <summary>
        ///     The version of this table.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        ///     The flags of this table.
        /// </summary>
        public DataFlags Flags { get; set; }

        /// <summary>
        ///     The entries in this table.
        /// </summary>
        public IDictionary<int, Entry> Entries { get; } = new SortedDictionary<int, Entry>();

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

//            /* 
//                * we can't (easily) predict the size ahead of time, so we write to a
//                * stream and then to the reader
//                */
//            ByteArrayOutputStream bout = new ByteArrayOutputStream();
//        DataOutputStream os = new DataOutputStream(bout);
//	try {

//		/* write the header */

//        {
//        public ByteBuffer encode() throws IOException

        /**
            * Encodes this {@link ReferenceTable} into a {@link ByteBuffer}.
            * @return The {@link ByteBuffer}.
            * @throws IOException if an I/O error occurs.
            */
//		os.write(format);
//		if (format >= 6) {
//			os.writeInt(version);
//		}
//    os.write(flags);

//        /* calculate and write the number of non-null entries */
//        putSmartFormat(entries.size(), os);

//		/* write the ids */
//		int last = 0;
//		for (int id = 0; id<capacity(); id++) {
//			if (entries.containsKey(id)) {
//				int delta = id - last;

//                putSmartFormat(delta, os);
//    last = id;
//			}
//}

//		/* write the identifiers if required */
//		if ((flags & FLAG_IDENTIFIERS) != 0) {
//			for (ReferenceTable.Entry entry : entries.values()) {
//				os.writeInt(entry.identifier);
//			}
//		}

//		/* write the CRC checksums */
//		for (ReferenceTable.Entry entry : entries.values()) {
//			os.writeInt(entry.crc);
//		}

//		if(Suite.build >= 816) {
//			/* unknown 816+ flag */
//			if ((flags & FLAG_UNKOWN_HASH) != 0) {
//				for (ReferenceTable.Entry entry : entries.values()) {
//					os.writeInt(entry.mysteryHash);
//				}
//			}
//		}

//		/* write the whirlpool digests if required */
//		if ((flags & FLAG_WHIRLPOOL) != 0) {
//			for (ReferenceTable.Entry entry : entries.values()) {
//				os.write(entry.whirlpool);
//			}
//		}
//		if(Suite.build >= 816) {
//			/* unknown 816+ flag */
//			if ((flags & FLAG_SIZES) != 0) {
//				for (ReferenceTable.Entry entry : entries.values()) {
//					os.writeInt(entry.compressedSize);
//					os.writeInt(entry.uncompressedSize);
//				}
//			}
//		}
//		/* write the versions */
//		for (ReferenceTable.Entry entry : entries.values()) {
//			os.writeInt(entry.version);
//		}

//		/* calculate and write the number of non-null child entries */
//		for (ReferenceTable.Entry entry : entries.values()) {

//            putSmartFormat(entry.entries.size(), os);
//		}

//		/* write the child ids */
//		for (ReferenceTable.Entry entry : entries.values()) {
//			last = 0;
//			for (int id = 0; id<entry.capacity(); id++) {
//				if (entry.entries.containsKey(id)) {
//					int delta = id - last;

//                    putSmartFormat(delta, os);
//last = id;
//				}
//			}
//		}

//		/* write the child identifiers if required  */
//		if ((flags & FLAG_IDENTIFIERS) != 0) {
//			for (ReferenceTable.Entry entry : entries.values()) {
//				for (ReferenceTable.ChildEntry child : entry.entries.values()) {
//					os.writeInt(child.identifier);
//				}
//			}
//		}

//		/* convert the stream to a byte array and then wrap a reader */
//		byte[] bytes = bout.toByteArray();
//		return ByteBuffer.wrap(bytes);
//	} finally {
//		os.close();
//	}

        ///**
        // * Puts the data into a certain type by the format id.
        // * @param val The value to put into the reader.
        // * @param reader The reader.
        // * @throws IOException The exception thrown if an i/o error occurs.
        // */
        //public void putSmartFormat(int val, DataOutputStream os) throws IOException
        //{
        //  if(format >= 7)
        //   ReferenceTable.putSmartInt(os, val);
        //  else 
        //   os.writeShort((short) val);
        //}
    }
}