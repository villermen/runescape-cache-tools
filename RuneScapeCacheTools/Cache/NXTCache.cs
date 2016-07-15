using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace Villermen.RuneScapeCacheTools.Cache
{
	public class NXTCache : Cache
	{
		private readonly Dictionary<int, SQLiteConnection> indexConnections = new Dictionary<int, SQLiteConnection>();

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

		public override byte[] GetFileData(int indexId, int fileId)
		{
			var connection = GetIndexConnection(indexId);

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

	    public override ReferenceTable GetReferenceTable(int indexId)
	    {
            var connection = GetIndexConnection(indexId);

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

	    public override IEnumerable<int> GetFileIds(int indexId)
		{
			var connection = GetIndexConnection(indexId);
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

		protected string GetIndexFile(int indexId)
		{
			return $"{CacheDirectory}js5-{indexId}.jcache";
		}

		protected SQLiteConnection GetIndexConnection(int indexId)
		{
			// Return an established connection
			if (indexConnections.ContainsKey(indexId))
			{
				return indexConnections[indexId];
			}

			// Store and return a new connection
			var connection = new SQLiteConnection($"Data Source={GetIndexFile(indexId)};Version=3;");
			connection.Open();

			indexConnections.Add(indexId, connection);

			return connection;
		}
	}
}