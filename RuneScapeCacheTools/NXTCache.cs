using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Villermen.RuneScapeCacheTools
{
	public class NXTCache : Cache
	{
		public override string DefaultCacheDirectory
		{
			get
			{
				return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
					"/Jagex/RuneScape/";
			}
		}

		public override async Task ExtractAllAsync()
		{
			throw new NotImplementedException();
		}

		public override async Task ExtractArchiveAsync(int archiveId)
		{
			throw new NotImplementedException();
		}

		public override async Task ExtractFileAsync(int archiveId, int fileId)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<int> getArchiveIds()
		{
			return Directory.EnumerateFiles(CacheDirectory, "js5-???.jcache")
				.Select((archiveFilePath) =>
				{
					string archiveFileName = Path.GetFileNameWithoutExtension(archiveFilePath);
					string archiveIdString = archiveFileName.Substring(archiveFileName.LastIndexOf('-') + 1);
					return int.Parse(archiveIdString);
				})
				.OrderBy((id) => id);
		}
	}
}
