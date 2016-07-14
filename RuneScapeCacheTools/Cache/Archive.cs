using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    /// An <see cref="Archive"/> is a file within the cache that can have multiple member files inside it.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Villermen</author>
    public class Archive
    {
        /// <summary>
        /// Decodes the specified <see cref="BinaryReader"/> into an <see cref="Archive"/>.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="size">The size of the archive in ???.</param>
        /// <returns></returns>
        public static Archive Decode(BinaryReader reader, int size)
        {
            /* allocate a new archive object */
            Archive archive = new Archive(size);

            /* read the number of chunks at the end of the archive */
            buffer.position(buffer.limit() - 1);
            int chunks = buffer.get() & 0xFF;
            /* read the sizes of the child entries and individual chunks */
            int[][] chunkSizes = new int[chunks][size];
            int[] sizes = new int[size];
            buffer.position(buffer.limit() - 1 - chunks * size * 4);
            for (int chunk = 0; chunk < chunks; chunk++)
            {
                int chunkSize = 0;
                for (int id = 0; id < size; id++)
                {
                    /* read the delta-encoded chunk length */
                    int delta = buffer.getInt();
                    chunkSize += delta;

                    chunkSizes[chunk][id] = chunkSize; /* store the size of this chunk */
                    sizes[id] += chunkSize;            /* and add it to the size of the whole file */
                }
            }

            /* allocate the buffers for the child entries */
            for (int id = 0; id < size; id++)
            {
                archive.entries[id] = ByteBuffer.allocate(sizes[id]);
            }

            /* read the data into the buffers */
            buffer.position(0);
            for (int chunk = 0; chunk < chunks; chunk++)
            {
                for (int id = 0; id < size; id++)
                {
                    /* get the length of this chunk */
                    int chunkSize = chunkSizes[chunk][id];

                    /* copy this chunk into a temporary buffer */
                    byte[] temp = new byte[chunkSize];
                    buffer.get(temp);

                    /* copy the temporary buffer into the file buffer */
                    archive.entries[id].put(temp);
                }
            }

            /* flip all of the buffers */
            for (int id = 0; id < size; id++)
            {
                archive.entries[id].flip();
            }

            /* return the archive */
            return archive;
        }

        /**
         * The array of entries in this archive.
         */
        private final ByteBuffer[] entries;

	/**
	 * Creates a new archive.
	 * @param size The number of entries in the archive.
	 */
	public Archive(int size)
        {
            this.entries = new ByteBuffer[size];
        }

        /**
         * Encodes this {@link Archive} into a {@link ByteBuffer}.
         * <p />
         * Please note that this is a fairly simple implementation that does not
         * attempt to use more than one chunk.
         * @return An encoded {@link ByteBuffer}.
         * @throws IOException if an I/O error occurs.
         */
        public ByteBuffer encode() throws IOException
        { // TODO: an implementation that can use more than one chunk
            ByteArrayOutputStream bout = new ByteArrayOutputStream();
        DataOutputStream os = new DataOutputStream(bout);
		try {
			/* add the data for each entry */
			for (int id = 0; id<entries.length; id++) {
				/* copy to temp buffer */
				byte[] temp = new byte[entries[id].limit()];
        entries[id].position(0);
				try {
					entries[id].get(temp);
    } finally {
					entries[id].position(0);
}

/* copy to output stream */
os.write(temp);
			}

			/* write the chunk lengths */
			int prev = 0;
			for (int id = 0; id<entries.length; id++) {
				/* 
				 * since each file is stored in the only chunk, just write the
				 * delta-encoded file size
				 */
				int chunkSize = entries[id].limit();
os.writeInt(chunkSize - prev);
				prev = chunkSize;
			}

			/* we only used one chunk due to a limitation of the implementation */
			bout.write(1);

			/* wrap the bytes from the stream in a buffer */
			byte[] bytes = bout.toByteArray();
			return ByteBuffer.wrap(bytes);
		} finally {
			os.close();
		}
	}

	/**
	 * Gets the entry with the specified id.
	 * @param id The id.
	 * @return The entry.
	 */
	public ByteBuffer getEntry(int id)
{
    return entries[id];
}

/**
 * Inserts/replaces the entry with the specified id.
 * @param id The id.
 * @param buffer The entry.
 */
public void putEntry(int id, ByteBuffer buffer)
{
    entries[id] = buffer;
}

/**
 * Gets the size of this archive.
 * @return The size of this archive.
 */
public int size()
{
    return entries.length;
}

}

}
