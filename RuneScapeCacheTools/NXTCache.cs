using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache;

namespace Villermen.RuneScapeCacheTools
{
	public class NXTCache : CacheMehRename
	{
		private readonly Dictionary<int, SQLiteConnection> archiveConnections = new Dictionary<int, SQLiteConnection>();

		public override string DefaultCacheDirectory
			=> Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "/Jagex/RuneScape/";

		public override IEnumerable<int> GetArchiveIds()
		{
			return Directory.EnumerateFiles(CacheDirectory, "js5-???.jcache")
				.Select(archiveFilePath =>
				{
					var archiveFileName = Path.GetFileNameWithoutExtension(archiveFilePath);
					var archiveIdString = archiveFileName.Substring(archiveFileName.LastIndexOf('-') + 1);
					return int.Parse(archiveIdString);
				})
				.OrderBy(id => id);
		}

		protected override byte[] GetFileData(int archiveId, int fileId)
		{
			var connection = GetArchiveConnection(archiveId);

			var command = new SQLiteCommand(
				$"SELECT DATA FROM cache WHERE KEY = $fileId"
				, connection);
			command.Parameters.AddWithValue("fileId", fileId);
			var reader = command.ExecuteReader();

			reader.Read();

			if (reader["DATA"].GetType() == typeof(byte[]))
			{
				return (byte[]) reader["DATA"];
			}

			return null;
		}

	    public override ReferenceTable GetReferenceTable(int archiveId)
	    {
            var connection = GetArchiveConnection(archiveId);

            var command = new SQLiteCommand(
                $"SELECT DATA FROM cache_index"
                , connection);
            var reader = command.ExecuteReader();

            reader.Read();

            if (reader["DATA"].GetType() == typeof(byte[]))
            {
                var binaryReader = new BinaryReader(new MemoryStream((byte[]) reader["DATA"]));
                return ReferenceTable.Decode(binaryReader);
            }

            return null;
        }

	    public override IEnumerable<int> GetFileIds(int archiveId)
		{
			var connection = GetArchiveConnection(archiveId);
			var command = connection.CreateCommand();
			command.CommandText = "SELECT KEY FROM cache";
			var reader = command.ExecuteReader();

			var fileIds = new List<int>();
			while (reader.Read())
			{
				fileIds.Add((int) (long) reader["KEY"]);
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
			var connection = new SQLiteConnection($"Data Source={GetArchiveFile(archiveId)};Version=3;");
			connection.Open();

			archiveConnections.Add(archiveId, connection);

			return connection;
		}
	}
}