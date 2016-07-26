using System;
using System.Collections.Generic;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;

namespace Villermen.RuneScapeCacheTools.Cache
{
	/// <summary>
	///   RuneTek5 (RS3 in NXT & HTML) cache format.
	/// </summary>
	public class RuneTek5Cache : Cache
	{
		private RuneTek5.RuneTek5Cache _backingCache;

		private string _cacheDirectory;

		public RuneTek5Cache()
		{
		}

		public RuneTek5Cache(IExtensionGuesser extensionGuesser) : base(extensionGuesser)
		{
		}

		public override string DefaultCacheDirectory
			=> Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jagexcache/runescape/LIVE/";

		public override string CacheDirectory
		{
			get { return _cacheDirectory; }
			set
			{
				_cacheDirectory = value;
				_backingCache = new RuneTek5.RuneTek5Cache(new FileStore(_cacheDirectory));
			}
		}

		public override int IndexCount => _backingCache.IndexCount;

		public override int GetFileCount(int indexId)
		{
			return _backingCache.GetFileCount(indexId);
		}

		public override int GetArchiveFileCount(int indexId, int archiveId)
		{
			var archive = _backingCache.GetArchive(indexId, archiveId);
			return archive.Entries.Length;
		}

		public override byte[] GetFileData(int indexId, int fileId)
		{
			return _backingCache.GetFile(indexId, fileId).Data;
		}

		public override byte[] GetArchiveFileData(int indexId, int archiveId, int fileId)
		{
			var archive = _backingCache.GetArchive(indexId, archiveId);
			return archive.Entries[fileId];
		}
	}
}