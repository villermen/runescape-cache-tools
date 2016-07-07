using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data.SQLite;

namespace Villermen.RuneScapeCacheTools
{
	public class NXTCache : Cache
	{
		private Dictionary<int, SQLiteConnection> archiveConnections = new Dictionary<int, SQLiteConnection>();

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
			SQLiteConnection connection = GetArchiveConnection(archiveId);

			SQLiteCommand command = new SQLiteCommand(
				$"SELECT DATA FROM cache WHERE KEY = $fileId"
				, connection);
			command.Parameters.AddWithValue("fileId", fileId);
			SQLiteDataReader reader = command.ExecuteReader();

			reader.Read();

			if (reader["DATA"].GetType().Equals(typeof(byte[])))
			{
				return (byte[])reader["DATA"];
			}

			return null;
		}

		public override IEnumerable<int> getFileIds(int archiveId)
		{
			SQLiteConnection connection = GetArchiveConnection(archiveId);
			SQLiteCommand command = connection.CreateCommand();
			command.CommandText = "SELECT KEY FROM cache";
			SQLiteDataReader reader = command.ExecuteReader();

			List<int> fileIds = new List<int>();
			while(reader.Read())
			{
				fileIds.Add((int)(long) reader["KEY"]);
			}

			return fileIds;
		}

		protected string GetArchiveFile(int archiveId)
		{
			return $"{CacheDirectory}js5-{archiveId}.jcache";
		}

		protected SQLiteConnection GetArchiveConnection(int archiveId)
		{
			// Return an established connection
			if (archiveConnections.ContainsKey(archiveId))
			{
				return archiveConnections[archiveId];
			}

			// Store and return a new connection
			SQLiteConnection connection = new SQLiteConnection($"Data Source={GetArchiveFile(archiveId)};Version=3;");
			connection.Open();

			archiveConnections.Add(archiveId, connection);

			return connection;
		}
	}
}
