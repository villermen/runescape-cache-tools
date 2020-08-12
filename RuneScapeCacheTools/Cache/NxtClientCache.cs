using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.Cache
{
    public class NxtClientCache : ReferenceTableCache
    {
        public static string DefaultCacheDirectory => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "/Jagex/RuneScape/";

        /// <summary>
        /// The directory where the cache is located.
        /// </summary>
        public string CacheDirectory { get; }

        public bool ReadOnly { get; }

        private readonly object _ioLock = new object();

        private readonly Dictionary<CacheIndex, SQLiteConnection> _connections = new Dictionary<CacheIndex, SQLiteConnection>();

        public NxtClientCache(string? cacheDirectory = null, bool readOnly = true) : base(new RuneTek7CacheFileDecoder())
        {
            this.CacheDirectory = PathExtensions.FixDirectory(cacheDirectory ?? NxtClientCache.DefaultCacheDirectory);
            this.ReadOnly = readOnly;

            // Create the cache directory if writing is allowed.
            if (!this.ReadOnly)
            {
                Directory.CreateDirectory(this.CacheDirectory);
            }

            this.OpenConnections();
        }

        public override IEnumerable<CacheIndex> GetAvailableIndexes()
        {
            return this._connections.Keys;
        }

        public override byte[] GetFileData(CacheIndex index, int fileId)
        {
            // Reference tables are stored as file 1 in a separate table in the database of the index.
            var dbTable = "cache";
            if (index == CacheIndex.ReferenceTables)
            {
                index = (CacheIndex)fileId;
                dbTable = "cache_index";
                fileId = 1;
            }

            lock (this._ioLock)
            {
                if (!this._connections.Keys.Contains(index))
                {
                    throw new CacheFileNotFoundException($"Index {(int)index} is not available.");
                }

                var connection = this._connections[index];

                using var command = connection.CreateCommand();
                command.CommandText = $"SELECT DATA FROM {dbTable} WHERE KEY = @fileId";
                command.Parameters.AddWithValue("fileId", fileId);

                var data = (byte[]?)command.ExecuteScalar();
                if (data == null)
                {
                    throw new CacheFileNotFoundException($"File {(int)index}/{fileId} does not exist in the cache.");
                }

                // TODO: We can verify the checksum/version here, or maybe allow overriding GetFile() to put it in the info.

                return data;
            }
        }

        protected override void PutFileData(CacheIndex index, int fileId, byte[] data)
        {
            if (this.ReadOnly)
            {
                throw new CacheException("Can't write data in readonly mode.");
            }

            var dbTable = "cache";
            if (index == CacheIndex.ReferenceTables)
            {
                index = (CacheIndex)fileId;
                dbTable = "cache_index";
                fileId = 1;
            }

            lock (this._ioLock)
            {
                if (!this._connections.Keys.Contains(index))
                {
                    this.OpenConnection(index, true);
                }

                var connection = this._connections[index];

                using var command = connection.CreateCommand();
                command.CommandText = $"INSERT OR REPLACE INTO {dbTable} (KEY, DATA, VERSION, CRC) VALUES (@key, @data, @version, @crc)";
                command.Parameters.AddWithValue("key", fileId);
                command.Parameters.AddWithValue("data", data);
                command.Parameters.AddWithValue("version", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                command.Parameters.AddWithValue("crc", -1);
                command.ExecuteNonQuery();

                // TODO: Use actual version and CRC.
            }
        }

        private void OpenConnections()
        {
            for (var indexId = 0; indexId <= 255; indexId++)
            {
                this.OpenConnection((CacheIndex)indexId, false);
            }
        }

        private void OpenConnection(CacheIndex index, bool force)
        {
            var indexPath = Path.Combine(this.CacheDirectory, $"js5-{(int)index}.jcache");

            var createTables = false;
            if (!System.IO.File.Exists(indexPath))
            {
                if (!force)
                {
                    return;
                }

                createTables = true;
            }

            var connectionStringBuilder = new SQLiteConnectionStringBuilder
            {
                DataSource = indexPath,
                ReadOnly = this.ReadOnly,
                FailIfMissing = this.ReadOnly,
            };
            var connection = new SQLiteConnection(connectionStringBuilder.ToString());
            connection.Open();

            if (createTables)
            {
                using var cacheCommand = connection.CreateCommand();
                cacheCommand.CommandText = "CREATE TABLE cache (KEY INTEGER PRIMARY KEY, DATA BLOB, VERSION INTEGER, CRC INTEGER)";
                cacheCommand.ExecuteNonQuery();
                using var cacheIndexCommand = connection.CreateCommand();
                cacheIndexCommand.CommandText = "CREATE TABLE cache_index (KEY INTEGER PRIMARY KEY, DATA BLOB, VERSION INTEGER, CRC INTEGER)";
                cacheIndexCommand.ExecuteNonQuery();
            }

            this._connections[index] = connection;
        }

        private void CloseConnections()
        {
            foreach (var connection in this._connections.Values)
            {
                connection.Close();
            }
            this._connections.Clear();
        }

        public override void Dispose()
        {
            this.CloseConnections();
        }
    }
}
