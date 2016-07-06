using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Data.SQLite;

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

		protected override byte[] GetFileData(int archiveId, int fileId)
		{
			// TODO: Re-use
			SQLiteConnection connection = new SQLiteConnection($"Data Source={GetArchiveFile(archiveId)};Version=3;");
			connection.Open();

			SQLiteDataReader reader = new SQLiteCommand(
				$"SELECT DATA FROM cache WHERE KEY = '{fileId}'"
				, connection).ExecuteReader();

			reader.Read();
			var d = reader["DATA"];

			throw new NotImplementedException();
		}

		public override IEnumerable<int> getFileIds()
		{
			throw new NotImplementedException();
		}

		protected string GetArchiveFile(int archiveId)
		{
			return $"{CacheDirectory}js5-{archiveId}.jcache";
		}
	}
}
