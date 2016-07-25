using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace Villermen.RuneScapeCacheTools.Cache
{
	/// <summary>
	///   RuneTek7 (RS3 in NXT & HTML) cache format.
	/// </summary>
	[Obsolete("Finishing RuneTek5 first before continuing with this.")]
	public class RuneTek7Cache : Cache
	{
		private readonly Dictionary<int, SQLiteConnection> _indexConnections = new Dictionary<int, SQLiteConnection>();

		public override string DefaultCacheDirectory
			=> Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "/Jagex/RuneScape/";

		public override IEnumerable<int> GetIndexIds()
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

		public override byte[] GetArchiveFileData(int indexId, int archiveId, int fileId)
		{
			throw new NotImplementedException();
		}

		//   public override ReferenceTable GetReferenceTable(int indexId)
		//{
		//       var connection = GetIndexConnection(indexId);

		//       var command = new SQLiteCommand(
		//           $"SELECT DATA FROM cache_index"
		//           , connection);
		//       var reader = command.ExecuteReader();

		//       reader.Read();

		//       if (reader["DATA"].GetType() == typeof(byte[]))
		//       {
		//           return ReferenceTable.Decode(new MemoryStream((byte[])reader["DATA"]));
		//       }

		//       return null;
		//   }

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

		public override int GetArchiveFileCount(int indexId, int archiveId)
		{
			throw new NotImplementedException();
		}

		protected string GetIndexFile(int indexId)
		{
			return $"{CacheDirectory}js5-{indexId}.jcache";
		}

		protected SQLiteConnection GetIndexConnection(int indexId)
		{
			// Return an established connection
			if (_indexConnections.ContainsKey(indexId))
			{
				return _indexConnections[indexId];
			}

			// Store and return a new connection
			var connection = new SQLiteConnection($"Data Source={GetIndexFile(indexId)};Version=3;");
			connection.Open();

			_indexConnections.Add(indexId, connection);

			return connection;
		}
	}
}