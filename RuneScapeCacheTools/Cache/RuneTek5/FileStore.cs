using System;
using System.Collections.Generic;
using System.IO;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
	/// <summary>
	///   A file store holds multiple files inside a "virtual" file system made up of several index files and a single data
	///   file.
	/// </summary>
	/// <author>Graham</author>
	/// <author>`Discardedx2</author>
	/// <author>Villermen</author>
	public class FileStore
	{
		/// <summary>
		///   Index that contains metadata about the other indexes.
		/// </summary>
		public const int MetadataIndexId = 255;

		/// <summary>
		///   Lock that is used when reading data from the streams.
		/// </summary>
		private readonly object _streamReadLock = new object();

		/// <summary>
		///   Opens the file store in the specified directory.
		/// </summary>
		/// <param name="cacheDirectory">The directory containing the index and data files.</param>
		/// <exception cref="CacheException">If any of the main_file_cache.* files could not be found.</exception>
		public FileStore(string cacheDirectory)
		{
			var dataFile = Path.Combine(cacheDirectory, "main_file_cache.dat2");

			if (!File.Exists(dataFile))
			{
				throw new CacheException("Cache data file does not exist.");
			}

			DataStream = File.Open(dataFile, FileMode.Open);

			for (var indexId = 0; indexId < 254; indexId++)
			{
				var indexFile = Path.Combine(cacheDirectory + "main_file_cache.idx" + indexId);

				if (!File.Exists(indexFile))
				{
					continue;
				}

				IndexStreams.Add(indexId, File.Open(indexFile, FileMode.Open));
			}

			if (IndexStreams.Count == 0)
			{
				throw new CacheException("No index files found.");
			}

			var metaFile = Path.Combine(cacheDirectory + $"main_file_cache.idx{MetadataIndexId}");

			if (!File.Exists(metaFile))
			{
				throw new CacheException("Meta index file does not exist.");
			}

			MetaStream = File.Open(metaFile, FileMode.Open);
		}

		private Stream DataStream { get; }
		private IDictionary<int, Stream> IndexStreams { get; } = new Dictionary<int, Stream>();
		private Stream MetaStream { get; }

		/// <summary>
		///   The number of index files, not including the meta index file.
		/// </summary>
		public int IndexCount => IndexStreams.Count;

		/// <summary>
		///   Returns the number of files of the specified type.
		/// </summary>
		/// <param name="indexId"></param>
		/// <returns></returns>
		public int GetFileCount(int indexId)
		{
			if (!IndexStreams.ContainsKey(indexId))
			{
				throw new CacheException("Invalid index specified.");
			}

			return (int) (IndexStreams[indexId].Length / Index.Length);
		}

		public byte[] GetFileData(int indexId, int fileId)
		{
			var meta = indexId == MetadataIndexId;

			if (!meta && !IndexStreams.ContainsKey(indexId))
			{
				throw new CacheException("Invalid index specified.");
			}

			var indexReader = new BinaryReader(meta ? MetaStream : IndexStreams[indexId]);

			var indexPosition = (long) fileId * Index.Length;

			if (indexPosition < 0 || indexPosition >= indexReader.BaseStream.Length)
			{
				throw new CacheException("Given file does not exist.");
			}

			// Lock reading from stream, to allow multiple threads from calling this method at the same time
			lock (_streamReadLock)
			{
				indexReader.BaseStream.Position = indexPosition;

				var indexBytes = indexReader.ReadBytes(Index.Length);

				var index = new Index(indexBytes);

				var chunkId = 0;
				var remaining = index.Size;
				var dataReader = new BinaryReader(DataStream);
				var dataPosition = (long) index.Sector * Sector.Length;
				var extended = fileId > 65535;

				var dataStream = new MemoryStream(index.Size);
				do
				{
					dataReader.BaseStream.Position = dataPosition;

					var sector = new Sector(dataReader.ReadBytes(Sector.Length), extended);

					// Verify the obtained sector is actually what we are looking for
					if (sector.IndexId != indexId)
					{
						throw new CacheException("Sector index id mismatch.");
					}

					if (sector.FileId != fileId)
					{
						throw new CacheException("Sector file id mismatch.");
					}

					if (sector.ChunkId != chunkId)
					{
						throw new CacheException("Sector index mismatch.");
					}

					var bytesRead = Math.Min(extended ? Sector.ExtendedDataLength : Sector.DataLength, remaining);

					dataStream.Write(sector.Data, 0, bytesRead);
					remaining -= bytesRead;

					dataPosition = (long) sector.NextSectorId * Sector.Length;
					chunkId++;
				}
				while (remaining > 0);

				return dataStream.ToArray();
			}
		}

		public void WriteFile(int indexId, int fileId, byte[] data)
		{
			throw new NotImplementedException();
		}

		public void WriteFile(int indexId, int fileId, byte[] data, bool overwrite)
		{
			throw new NotImplementedException();
		}

		public byte[] GetMetadata(int fileId)
		{
			return GetFileData(MetadataIndexId, fileId);
		}
	}
}