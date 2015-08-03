using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuneScapeCacheTools
{
	public static class Cache
	{
		public const string CacheFileName = "main_file_cache.dat2";
		public const string IndexFilePrefix = "main_file_cache.idx";

		public static readonly string DefaultCacheDirectory = DirectoryHelper.FormatDirectory(@"%USERPROFILE%/jagexcache/runescape/LIVE/");
		public static readonly string DefaultOutputDirectory = DirectoryHelper.FormatDirectory(@"%TEMP%/rscachetools/");

		private static string _cacheDirectory = DefaultCacheDirectory;
		private static string _outputDirectory = DefaultOutputDirectory;
		private static string _tempDirectory = DirectoryHelper.FormatDirectory(@"%TEMP%/rscachetools/");

		/// <summary>
		/// Directory from which the cache is read.
		/// This directory should contain the cache file 'main_file_cache.dat2' and index files like 'main_file_cache.idx#'.
		/// </summary>
		public static string CacheDirectory
		{
			get
			{
				//check if directory exists/is readable
				if (!Directory.Exists(_cacheDirectory))
					throw new DirectoryNotFoundException("The given cache directory does not exist or is not readable.");

				return _cacheDirectory;
			}
			set { _cacheDirectory = DirectoryHelper.FormatDirectory(value); }
		}

		/// <summary>
		/// Directory where the cache will be extracted to.
		/// </summary>
		public static string OutputDirectory
		{
			get
			{
				//check if directory exists/is readable
				if (!Directory.Exists(_outputDirectory))
					throw new DirectoryNotFoundException("The given output directory does not exist or is not readable.");

				return _outputDirectory;
			}
			set { _outputDirectory = DirectoryHelper.FormatDirectory(value); }
		}

		/// <summary>
		/// Directory for temporary files, used in processing.
		/// </summary>
		public static string TempDirectory
		{
			get
			{
				//check if directory exists/is readable
				if (!Directory.Exists(_tempDirectory))
					throw new DirectoryNotFoundException("The given temp directory does not exist or is not readable.");

				return _tempDirectory;
			}
			set { _tempDirectory = DirectoryHelper.FormatDirectory(value); }
		}

		static Cache()
		{
			//create temporary directory if it doesn't exist yet
			Directory.CreateDirectory(_tempDirectory);
		}

		/// <summary>
		/// Obtains the ids of all the present index files.
		/// </summary>
		public static IEnumerable<int> GetArchiveIds()
		{
			return Directory.EnumerateFiles(CacheDirectory, IndexFilePrefix + "???").Select(file =>
			{
				int archiveId;
				if (int.TryParse(file.Substring(file.LastIndexOf(IndexFilePrefix) + IndexFilePrefix.Length), out archiveId))
					return archiveId;

				return -1;
			}).Where(id => id != -1).OrderBy(id => id);
		}

		/// <summary>
		/// Returns a path to a specified output file.
		/// </summary>
		/// <exception cref="FileNotFoundException"></exception>
		public static string GetFile(int archiveId, int fileId, bool extractOnFailure)
		{
			//quick filter and then regex filter to decide whether the file is actually what we're looking for
			string file = FindFile(archiveId, fileId);

			if (file != null)
				return file;

			if (!extractOnFailure)
				throw new FileNotFoundException();

			new CacheExtractJob(archiveId, fileId).Start();

			file = FindFile(archiveId, fileId);

			if (file == null)
				throw new FileNotFoundException();

			return file;
		}

		private static string FindFile(int archiveId, int fileId)
		{
			var files = Directory.EnumerateFiles(archiveId + "/" + fileId + "*").Where(file => Regex.IsMatch(file,
				$@"[^\d]{archiveId}(/|\\){fileId}(\..+)$")).ToList();

			return files.FirstOrDefault();
		}
	}
}