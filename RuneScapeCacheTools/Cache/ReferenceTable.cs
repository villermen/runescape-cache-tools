using System.Collections.Generic;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools;

namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    /// A ReferenceTable holds details for all the files with a singletype, such as checksums, versions and archive members.
    /// There are also optional fields for identifier hashes and whirlpool digests.
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Sean</author>
    /// <author>Villermen</author>
    /// </summary>
    public class ReferenceTable
    {
        /// <summary>
        /// Represents a child entry within an <see cref="Entry"/> in the <see cref="ReferenceTable"/>.
        /// </summary>
        public class ChildEntry
        {
            /// <summary>
            /// This entry's identifier.
            /// </summary>
            public int Identifier { get; private set; } = -1;

            /// <summary>
            /// The cache index of this entry.
            /// </summary>
            public int Index { get; private set; }

            public ChildEntry(int index)
            {
                Index = index;
            }
        }

        /// <summary>
        /// Represents a single entry within a <see cref="ReferenceTable"/>.
        /// </summary>
        public class Entry
        {
            /// <summary>
            /// Some unknown hash added on build 816.
            /// It is hard to pinpoint what exactly this is because it is not used in the client.
            /// </summary>
            public int MysteryHash { get; set; }

            /// <summary>
            /// The compressed size of this entry.
            /// </summary>
            public int CompressedSize { get; set; }

            /// <summary>
            /// The uncompressed size of this entry.
            /// </summary>
            public int UncompressedSize { get; set; }

            /// <summary>
            /// The identifier of this entry.
            /// </summary>
            public int Identifier { get; set; } = -1;

            /// <summary>
            /// The CRC32 checksum of this entry.
            /// </summary>
            public int CRC { get; set; }

            /// <summary>
            /// The whirlpool digest of this entry.
            /// </summary>
            public byte[] Whirlpool { get; set; } = new byte[64];

            /// <summary>
            /// The version of this entry stored as the time the file was last edited in seconds since the unix epoch.
            /// </summary>
            public int Version { get; set; }

            /// <summary>
            /// The cache index of this entry
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            /// The children in this entry.
            /// </summary>
            public IDictionary<int, ChildEntry> Entries { get; } = new SortedDictionary<int, ChildEntry>();

            public Entry(int index)
            {
                Index = index;
            }
        }

        // TODO: convert to flags enum
        /// <summary>
        /// A flag which indicates this <see cref="ReferenceTable"/> contains Djb2 hashed identifiers.
        /// </summary>
        public const int FlagIdentifiers = 0x01;

        /// <summary>
        /// A flag which indicates this <see cref="ReferenceTable"/>} contains whirlpool digests for its entries.
        /// </summary>
        public const int FlagWhirlpool = 0x02;

        /// <summary>
        /// A flag which indicates this <see cref="ReferenceTable"/> contains sizes for its entries.
        /// </summary>
        public const int FlagSizes = 0x04;

        /// <summary>
        /// A flag which indicates this <see cref="ReferenceTable"/> contains some kind of hash which is currently unused by the RuneScape client.
        /// </summary>
        public const int FlagUnkownHash = 0x08;

        /// <summary>
        /// Decodes the slave checksum table contained in the given <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ReferenceTable Decode(BinaryReader reader)
        {
            /* create a new table */
            ReferenceTable table = new ReferenceTable();

            /* read header */
            table.format = reader.get() & 0xFF;
            if (table.format < 5 || table.format > 7)
            {
                throw new RuntimeException("Incorrect JS5 protocol number: " + table.format);
            }
            if (table.format >= 6)
            {
                table.version = reader.getInt();
            }
            table.flags = reader.get() & 0xFF;

            /* read the ids */
            int[] ids = new int[table.format >= 7 ? ByteBufferUtils.getSmartInt(reader) : reader.getShort() & 0xFFFF];
            int accumulator = 0, size = -1;
            for (int i = 0; i < ids.length; i++)
            {
                int delta = table.format >= 7 ? ByteBufferUtils.getSmartInt(reader)
                        : reader.getShort() & 0xFFFF;
                ids[i] = accumulator += delta;
                if (ids[i] > size)
                {
                    size = ids[i];
                }
            }
            size++;
            //table.indices = ids;

            /* and allocate specific entries within that array */
            int index = 0;
            for (int id : ids)
            {
                table.entries.put(id, new Entry(index++));
            }

            /* read the identifiers if present */
            if ((table.flags & FlagIdentifiers) != 0)
            {
                for (int id : ids)
                {
                    table.entries.get(id).identifier = reader.getInt();
                }
            }

            /* read the CRC32 checksums */
            for (int id : ids)
            {
                table.entries.get(id).crc = reader.getInt();
            }

            /* read some type of hash*/
            if ((table.flags & FlagUnkownHash) != 0)
            {
                for (int id : ids)
                {
                    table.entries.get(id).mysteryHash = reader.getInt();
                }
            }

            /* read the whirlpool digests if present */
            if ((table.flags & FlagWhirlpool) != 0)
            {
                for (int id : ids)
                {
                    reader.get(table.entries.get(id).whirlpool);
                }
            }

            /* read the compressed and uncompressed sizes */
            if ((table.flags & FlagSizes) != 0)
            {
                for (int id : ids)
                {
                    table.entries.get(id).compressedSize = reader.getInt();
                    table.entries.get(id).uncompressedSize = reader.getInt();
                }
            }

            /* read the version numbers */
            for (int id : ids)
            {
                int version = reader.getInt();
                //System.out.println(version);
                table.entries.get(id).version = version;
            }

            /* read the child sizes */
            int[][] members = new int[size][];
            for (int id : ids)
            {
                members[id] = new int[table.format >= 7 ? ByteBufferUtils.getSmartInt(reader) : reader.getShort() & 0xFFFF];
            }

            /* read the child ids */
            for (int id : ids)
            {
                /* reset the accumulator and size */
                accumulator = 0;
                size = -1;

                /* loop through the array of ids */
                for (int i = 0; i < members[id].length; i++)
                {
                    int delta = table.format >= 7 ? ByteBufferUtils.getSmartInt(reader) : reader.getShort() & 0xFFFF;
                    members[id][i] = accumulator += delta;
                    if (members[id][i] > size)
                    {
                        size = members[id][i];
                    }
                }
                size++;

                /* and allocate specific entries within the array */
                index = 0;
                for (int child : members[id])
                {
                    table.entries.get(id).entries.put(child, new ChildEntry(index++));
                }
            }

            /* read the child identifiers if present */
            if ((table.flags & FlagIdentifiers) != 0)
            {
                for (int id : ids)
                {
                    for (int child : members[id])
                    {
                        table.entries.get(id).entries.get(child).identifier = reader.getInt();
                    }
                }
            }

            /* return the table we constructed */
            return table;
        }

        /**
            * Puts a smart integer into the stream.
            * @param os The stream.
            * @param value The value.
            * @throws IOException The exception thrown if an i/o error occurs.
            * 
            * Credits to Graham for this method.
            */
        public static void putSmartInt(DataOutputStream os, int value) throws IOException
        {
	if ((value & 0xFFFF) < 32768)
		os.writeShort(value);
	else
		os.writeInt(0x80000000 | value);
        }

        /**
            * The format of this table.
            */
        private int format;

        /**
            * The version of this table.
            */
        private int version;

        /**
            * The flags of this table.
            */
        private int flags;

        /**
            * The entries in this table.
            */
        private SortedMap<Integer, Entry> entries = new TreeMap<Integer, Entry>();

        /**
            * Gets the maximum number of entries in this table.
            * @return The maximum number of entries.
            */
        public int capacity()
        {
            if (entries.isEmpty())
                return 0;

            return entries.lastKey() + 1;
        }

        /**
            * Encodes this {@link ReferenceTable} into a {@link ByteBuffer}.
            * @return The {@link ByteBuffer}.
            * @throws IOException if an I/O error occurs.
            */
        public ByteBuffer encode() throws IOException
        {
            /* 
                * we can't (easily) predict the size ahead of time, so we write to a
                * stream and then to the reader
                */
            ByteArrayOutputStream bout = new ByteArrayOutputStream();
        DataOutputStream os = new DataOutputStream(bout);
	try {
		/* write the header */
		os.write(format);
		if (format >= 6) {
			os.writeInt(version);
		}
    os.write(flags);

        /* calculate and write the number of non-null entries */
        putSmartFormat(entries.size(), os);

		/* write the ids */
		int last = 0;
		for (int id = 0; id<capacity(); id++) {
			if (entries.containsKey(id)) {
				int delta = id - last;

                putSmartFormat(delta, os);
    last = id;
			}
}

		/* write the identifiers if required */
		if ((flags & FLAG_IDENTIFIERS) != 0) {
			for (ReferenceTable.Entry entry : entries.values()) {
				os.writeInt(entry.identifier);
			}
		}

		/* write the CRC checksums */
		for (ReferenceTable.Entry entry : entries.values()) {
			os.writeInt(entry.crc);
		}

		if(Suite.build >= 816) {
			/* unknown 816+ flag */
			if ((flags & FLAG_UNKOWN_HASH) != 0) {
				for (ReferenceTable.Entry entry : entries.values()) {
					os.writeInt(entry.mysteryHash);
				}
			}
		}

		/* write the whirlpool digests if required */
		if ((flags & FLAG_WHIRLPOOL) != 0) {
			for (ReferenceTable.Entry entry : entries.values()) {
				os.write(entry.whirlpool);
			}
		}
		if(Suite.build >= 816) {
			/* unknown 816+ flag */
			if ((flags & FLAG_SIZES) != 0) {
				for (ReferenceTable.Entry entry : entries.values()) {
					os.writeInt(entry.compressedSize);
					os.writeInt(entry.uncompressedSize);
				}
			}
		}
		/* write the versions */
		for (ReferenceTable.Entry entry : entries.values()) {
			os.writeInt(entry.version);
		}

		/* calculate and write the number of non-null child entries */
		for (ReferenceTable.Entry entry : entries.values()) {

            putSmartFormat(entry.entries.size(), os);
		}

		/* write the child ids */
		for (ReferenceTable.Entry entry : entries.values()) {
			last = 0;
			for (int id = 0; id<entry.capacity(); id++) {
				if (entry.entries.containsKey(id)) {
					int delta = id - last;

                    putSmartFormat(delta, os);
last = id;
				}
			}
		}

		/* write the child identifiers if required  */
		if ((flags & FLAG_IDENTIFIERS) != 0) {
			for (ReferenceTable.Entry entry : entries.values()) {
				for (ReferenceTable.ChildEntry child : entry.entries.values()) {
					os.writeInt(child.identifier);
				}
			}
		}

		/* convert the stream to a byte array and then wrap a reader */
		byte[] bytes = bout.toByteArray();
		return ByteBuffer.wrap(bytes);
	} finally {
		os.close();
	}
}

	/**
	 * Gets the entry with the specified id, or {@code null} if it does not
	 * exist.
	 * @param id The id.
	 * @return The entry.
	 */
	public Entry getEntry(int id)
    {
        return entries.get(id);
    }

    /**
     * Gets the child entry with the specified id, or {@code null} if it does
     * not exist.
     * @param id The parent id.
     * @param child The child id.
     * @return The entry.
     */
    public ChildEntry getEntry(int id, int child)
    {
        Entry entry = entries.get(id);
        if (entry == null)
            return null;

        return entry.getEntry(child);
    }

    /**
     * Gets the flags of this table.
     * @return The flags.
     */
    public int getFlags()
    {
        return flags;
    }

    /**
     * Gets the format of this table.
     * @return The format.
     */
    public int getFormat()
    {
        return format;
    }

    /**
     * Gets the version of this table.
     * @return The version of this table.
     */
    public int getVersion()
    {
        return version;
    }

    /**
     * Replaces or inserts the entry with the specified id.
     * @param id The id.
     * @param entry The entry.
     */
    public void putEntry(int id, Entry entry)
    {
        entries.put(id, entry);
    }

    /**
     * Puts the data into a certain type by the format id.
     * @param val The value to put into the reader.
     * @param reader The reader.
     * @throws IOException The exception thrown if an i/o error occurs.
     */
    public void putSmartFormat(int val, DataOutputStream os) throws IOException
    {
		    if(format >= 7)
			    ReferenceTable.putSmartInt(os, val);
		    else 
			    os.writeShort((short) val);
    }

    /**
     * Removes the entry with the specified id.
     * @param id The id.
     */
    public void removeEntry(int id)
    {
        entries.remove(id);
    }

    /**
     * Sets the flags of this table.
     * @param flags The flags.
     */
    public void setFlags(int flags)
    {
        this.flags = flags;
    }

    /**
     * Sets the format of this table.
     * @param format The format.
     */
    public void setFormat(int format)
    {
        this.format = format;
    }

    /**
     * Sets the version of this table.
     * @param version The version.
     */
    public void setVersion(int version)
    {
        this.version = version;
    }

    /**
     * Gets the number of actual entries.
     * @return The number of actual entries.
     */
    public int size()
    {
        return entries.size();
    }
}