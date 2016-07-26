using System;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
	/// <summary>
	///   The <see cref="RuneTek5Cache" /> class provides a unified, high-level API for modifying the cache of a Jagex game.
	/// </summary>
	/// <author>Graham</author>
	/// <author>`Discardedx2</author>
	/// <author>Villermen</author>
	public class RuneTek5Cache : Cache
	{
		public RuneTek5Cache()
		{
			CacheDirectory = DefaultCacheDirectory;
		}

		/// <summary>
		///   Creates an interface on the cache stored in the given directory.
		/// </summary>
		/// <param name="cacheDirectory"></param>
		public RuneTek5Cache(string cacheDirectory)
		{
			CacheDirectory = cacheDirectory;
		}

		private string _cacheDirectory;
		public override string CacheDirectory
		{
			get { return _cacheDirectory; }
			set
			{
				_cacheDirectory = value;
				FileStore = new FileStore(_cacheDirectory);
			}
		}

		/// <summary>
		///   The <see cref="RuneTek5.FileStore" /> that backs this cache.
		/// </summary>
		public FileStore FileStore { get; set; }

		public override int IndexCount => FileStore.IndexCount;

		public override string DefaultCacheDirectory
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

		public override int GetArchiveFileCount(int indexId, int archiveId)
		{
			var archive = GetArchive(indexId, archiveId);
			return archive.Entries.Length;
		}

		public override byte[] GetFileData(int indexId, int fileId)
		{
			var container = GetFile(indexId, fileId);
			return container.Data;
		}

		public override byte[] GetArchiveFileData(int indexId, int archiveId, int fileId)
		{
			var archive = GetArchive(indexId, archiveId);
			return archive.Entries[fileId];
		}

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

		public Container GetFile(int indexId, int fileId)
		{
			if (indexId == FileStore.MetadataIndexId)
			{
				throw new CacheException("Reference tables can only be read with the low level FileStore API!");
			}

			// Delegate the call to the file store and then decode the container
			 return new Container(FileStore.GetFileData(indexId, fileId));
		}

		/// <summary>
		///   Reads an <see cref="Archive" />.
		/// </summary>
		/// <param name="indexId"></param>
		/// <param name="archiveId"></param>
		/// <returns></returns>
		public Archive GetArchive(int indexId, int archiveId)
		{
			// Grab the container and the reference table
			var container = GetFile(indexId, archiveId);
			var tableContainer = new Container(FileStore.GetMetadata(indexId));

			var table = new ReferenceTable(tableContainer.Data);

			// Check if the file/entry are valid
			var entry = table.Entries[archiveId];

			return new Archive(container.Data, entry.Capacity);
		}
	}
}