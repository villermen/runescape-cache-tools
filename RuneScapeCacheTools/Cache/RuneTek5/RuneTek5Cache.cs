using System;
using System.IO;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
	/// <summary>
	///   The <see cref="RuneTek5Cache" /> class provides a unified, high-level API for modifying the cache of a Jagex game.
	/// </summary>
	/// <author>Graham</author>
	/// <author>`Discardedx2</author>
	/// <author>Villermen</author>
	public class RuneTek5Cache : CacheBase
	{
	    /// <summary>
	    ///   Creates an interface on the cache stored in the given directory.
	    /// </summary>
	    /// <param name="cacheDirectory"></param>
	    public RuneTek5Cache(string cacheDirectory = null) : 
            base(cacheDirectory ?? DefaultCacheDirectory)
	    {
            FileStore = new FileStore(CacheDirectory);
	    }

		/// <summary>
		///   The <see cref="RuneTek5.FileStore" /> that backs this cache.
		/// </summary>
		public FileStore FileStore { get; }

		public override int IndexCount => FileStore.IndexCount;

		public static string DefaultCacheDirectory
			=> Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jagexcache/runescape/LIVE/";

		/// <summary>
		///   Computes the <see cref="ChecksumTable" /> for this cache.
		///   The checksum table forms part of the so-called "update keys".
		/// </summary>
		/// <returns></returns>
		public ChecksumTable CreateChecksumTable()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Gets the number of files in the specified index.
		/// </summary>
		/// <param name="indexId"></param>
		/// <returns></returns>
		public override int GetFileCount(int indexId)
		{
			return FileStore.GetFileCount(indexId);
		}

		//public override int GetArchiveFileCount(int indexId, int archiveId)
		//{
		//	var archive = GetArchive(indexId, archiveId);
		//	return archive.Entries.Length;
		//}

		public override CacheFile GetFile(int indexId, int fileId)
		{
            // Create the reference table for the requested index
			var metaContainer = new Container(FileStore.GetFileData(FileStore.MetadataIndexId, indexId));
            var referenceTable = new ReferenceTable(metaContainer.Data);

            // The file must at least be defined in the reference table (doesn't mean it is actually complete)
		    if (!referenceTable.Entries.ContainsKey(fileId))
		    {
		        throw new CacheException($"Given cache file {fileId} in index {indexId} does not exist.");
		    }

		    var referenceTableEntry = referenceTable.Entries[fileId];

            Container container;

            try
		    {
                container = new Container(FileStore.GetFileData(indexId, fileId));
		    }
		    catch (SectorException exception)
		    {
		        throw new CacheException($"Cache file {fileId} in index {indexId} is incomplete or corrupted.", exception);
		    }

            // Archives (files with multiple entries) are handled differently
		    byte[][] data;

		    var amountOfEntries = referenceTableEntry.ChildEntries.Count;

		    if (amountOfEntries == 1)
		    {
		        data = new byte[][] {container.Data};
		    }
		    else
		    {
                var archive = new Archive(container.Data, amountOfEntries);
		        data = archive.Entries;
		    }

		    return new CacheFile(indexId, fileId, data, referenceTableEntry.Version);
		}

		//public override byte[] GetArchiveFileData(int indexId, int archiveId, int fileId)
		//{
		//	var archive = GetArchive(indexId, archiveId);
		//	return archive.Entries[fileId];
		//}

		/// <summary>
		///   Gets a file id from the cache by name.
		/// </summary>
		/// <returns></returns>
		public int GetFileIdByName(int indexId, string fileName)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Writes a file to the cache and updates the <see cref="ReferenceTable" /> that it is associated with to match.
		/// </summary>
		/// <param name="indexId"></param>
		/// <param name="fileId"></param>
		/// <param name="container"></param>
		public void WriteFile(int indexId, int fileId, Container container)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Writes a file contained in an archive to the cache.
		/// </summary>
		/// <param name="indexId"></param>
		/// <param name="archiveId"></param>
		/// <param name="archive"></param>
		public void WriteArchive(int indexId, int archiveId, Archive archive)
		{
			throw new NotImplementedException();
		}

		public int FindFileName(ReferenceTable referenceTable, string fileName)
		{
			throw new NotImplementedException();
		}

		public int FindFileName(ReferenceTable referenceTable, string fileName, int fileId)
		{
			throw new NotImplementedException();
		}

		public int FindFileName(ReferenceTable referenceTable, string fileName, int archiveId, int fileId)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Reads an <see cref="Archive" />.
		/// </summary>
		/// <param name="indexId"></param>
		/// <param name="archiveId"></param>
		/// <returns></returns>
		//public Archive GetArchive(int indexId, int archiveId)
		//{
		//	// Grab the container and the reference table
		//	var container = GetContainer(indexId, archiveId);
		//	var tableContainer = new Container(FileStore.GetMetadata(indexId));

		//	var table = new ReferenceTable(tableContainer.Data);

		//	// Check if the file/entry are valid
		//	var entry = table.Entries[archiveId];

		//	return new Archive(container.Data, entry.ChildEntries.Count);
		//}

	    protected override void Dispose(bool disposing)
	    {
	        if (disposing)
	        {
	            FileStore.Dispose();
	        }
	    }
	}
}