using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    /// A file store holds multiple files inside a "virtual" file system made up of several index files and a single data file.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Villermen</author>
    public class FileStore
    {
        private readonly Stream _dataStream;
        private readonly IDictionary<int, Stream> _indexStreams = new Dictionary<int, Stream>();
        private readonly Stream _metaStream;

        /// <summary>
        /// Opens the file store in the specified directory.
        /// </summary>
        /// <param name="cacheDirectory">The directory containing the index and data files.</param>
        /// <exception cref="FileNotFoundException">If any of the main_file_cache.* files could not be found.</exception>
        public FileStore(string cacheDirectory)
        {
            var dataFile = Path.Combine(cacheDirectory, "main_file_cache.dat2");

            if (!File.Exists(dataFile))
            {
                throw new FileNotFoundException("Cache data file does not exist.");
            }

            _dataStream = File.Open(dataFile, FileMode.Open);

            for (var indexId = 0; indexId < 254; indexId++)
            {
                var indexFile = Path.Combine(cacheDirectory + "main_file_cache.idx" + indexId);

                if (!File.Exists(indexFile))
                {
                    continue;
                }

                _indexStreams.Add(indexId, File.Open(indexFile, FileMode.Open));
            }

            if (_indexStreams.Count == 0)
            {
                throw new FileNotFoundException("No index files found.");
            }

            var metaFile = Path.Combine(cacheDirectory + "main_file_cache.idx255");

            if (!File.Exists(metaFile))
            {
                throw new FileNotFoundException("Meta index file does not exist.");
            }

            _metaStream = File.Open(metaFile, FileMode.Open);
        }

        /// <summary>
        /// Initializes a new <see cref="FileStore"/> in the given directory.
        /// </summary>
        /// <param name="cacheDirectory"></param>
        /// <param name="amountOfIndexes"></param>
        public FileStore(string cacheDirectory, int amountOfIndexes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the number of files of the specified type.
        /// </summary>
        /// <param name="indexId"></param>
        /// <returns></returns>
        public int GetFileCount(int indexId)
        {
            if (!_indexStreams.ContainsKey(indexId))
            {
                throw new CacheException("Invalid index specified.");
            }

            return (int) (_indexStreams[indexId].Length/Index.DataLength);
        }

        public byte[] GetFileData(int indexId, int fileId)
        {
            if (!_indexStreams.ContainsKey(indexId))
            {
                throw new CacheException("Invalid index specified.");
            }

            var indexReader = new BinaryReader(_indexStreams[indexId]);
            indexReader.BaseStream.Position = 0;

            var ptr = (long) fileId*Index.DataLength;

            if (ptr < 0 || ptr >= indexReader.BaseStream.Length)
            {
                throw new FileNotFoundException("Given file does not exist.");
            }

            var reversedIndexBytes = indexReader.ReadBytes(Index.DataLength);
            Array.Reverse(reversedIndexBytes);

            var index = new Index(reversedIndexBytes);

            var indexData = new byte[index.Size];
            var sectorData = new byte[Sector.TotalLength];

            var chunk = 0;
            var remaining = index.Size;
            ptr = (long) index.Sector * Sector.TotalLength;

            do
            {
                // TODO: code goes here
            }
            while (remaining > 0);

            Array.Reverse(indexData);
            return indexData;
        }

        public void WriteFile(int indexId, int fileId, byte[] data)
        {
            throw new NotImplementedException();
        }

        public void WriteFile(int indexId, int fileId, byte[] data, bool overwrite)
        {
            throw new NotImplementedException();
        }
    }
}
