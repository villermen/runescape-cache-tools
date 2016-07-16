using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    /// An <see cref="Archive"/> is a file within the cache that can have multiple member files inside it.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Villermen</author>
    internal class Archive
    {
        /// <summary>
        /// The data of the entries in the archive.
        /// </summary>
        public byte[][] Entries { get; private set; }

        /// <summary>
        /// Decodes the specified <see cref="BinaryReader"/> into an <see cref="Archive"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="amountOfEntries">The amount of files in the archive.</param>
        /// <returns></returns>
        public static Archive Decode(Stream stream, int amountOfEntries)
        {
            var reader = new BinaryReader(stream);
            var archive = new Archive(amountOfEntries);

            var amountOfChunks = reader.ReadByte();

            // Read the sizes of the child entries and individual chunks
            var chunkSizes = new int[amountOfChunks, amountOfEntries];
            var entrySizes = new int[amountOfEntries];

            for (var chunkId = 0; chunkId < amountOfChunks; chunkId++)
            {
                var chunkSize = 0;
                for (var entryId = 0; entryId < amountOfEntries; entryId++)
                {
                    // Read the delta encoded chunk length
                    var delta = reader.ReadInt32BigEndian();
                    chunkSize += delta;

                    // Store the size of this chunk
                    chunkSizes[chunkId, entryId] = chunkSize;

                    // Add it to the size of the whole file
                    entrySizes[entryId] += chunkSize;
                }
            }

            //// Allocate the buffers for the child entries
            //for (var entryId = 0; entryId < amountOfEntries; entryId++)
            //{
            //    archive.Entries[entryId] = new byte[chunkSizes[entryId]];
            //}

            // Read the data into the buffers
            //buffer.position(0);
            for (var chunkId = 0; chunkId < amountOfChunks; chunkId++)
            {
                for (var entryId = 0; entryId < amountOfEntries; entryId++)
                {
                    // Read the bytes of the entry into the archive entries
                    var entrySize = chunkSizes[chunkId, entryId];
                    var entryData = reader.ReadBytes(entrySize);

                    if (entryData.Length != entrySize)
                    {
                        throw new CacheException("End of file reached while reading the archive.");
                    }
                    
                    archive.Entries[entryId] = entryData;
                }
            }

            ///* flip all of the buffers */
            //for (int id = 0; id < size; id++)
            //{
            //    archive.entries[id].flip();
            //}

            return archive;
        }

        /// <summary>
        /// Creates a new archive.
        /// </summary>
        /// <param name="size">The number of entries in the archive</param>
        public Archive(int size)
        {
            Entries = new byte[size][];
        }

        //        /**
        //         * Encodes this {@link Archive} into a {@link ByteBuffer}.
        //         * <p />
        //         * Please note that this is a fairly simple implementation that does not
        //         * attempt to use more than one chunk.
        //         * @return An encoded {@link ByteBuffer}.
        //         * @throws IOException if an I/O error occurs.
        //         */
        //        public ByteBuffer encode() throws IOException
        //        { // TODO: an implementation that can use more than one chunk
        //            ByteArrayOutputStream bout = new ByteArrayOutputStream();
        //        DataOutputStream os = new DataOutputStream(bout);
        //		try {
        //			/* add the data for each entry */
        //			for (int id = 0; id<entries.length; id++) {
        //				/* copy to temp buffer */
        //				byte[] temp = new byte[entries[id].limit()];
        //        entries[id].position(0);
        //				try {
        //					entries[id].get(temp);
        //    } finally {
        //					entries[id].position(0);
        //}

        ///* copy to output stream */
        //os.write(temp);
        //			}

        //			/* write the chunk lengths */
        //			int prev = 0;
        //			for (int id = 0; id<entries.length; id++) {
        //				/* 
        //				 * since each file is stored in the only chunk, just write the
        //				 * delta-encoded file size
        //				 */
        //				int chunkSize = entries[id].limit();
        //os.writeInt(chunkSize - prev);
        //				prev = chunkSize;
        //			}

        //			/* we only used one chunk due to a limitation of the implementation */
        //			bout.write(1);

        //			/* wrap the bytes from the stream in a buffer */
        //			byte[] bytes = bout.toByteArray();
        //			return ByteBuffer.wrap(bytes);
        //		} finally {
        //			os.close();
        //		}
        //	}

        //	/**
        //	 * Gets the entry with the specified id.
        //	 * @param id The id.
        //	 * @return The entry.
        //	 */
        //	public ByteBuffer getEntry(int id)
        //{
        //    return entries[id];
        //}

        ///**
        // * Inserts/replaces the entry with the specified id.
        // * @param id The id.
        // * @param buffer The entry.
        // */
        //public void putEntry(int id, ByteBuffer buffer)
        //{
        //    entries[id] = buffer;
        //}

        ///**
        // * Gets the size of this archive.
        // * @return The size of this archive.
        // */
        //public int size()
        //{
        //    return entries.length;
        //}
    }
}
