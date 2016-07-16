using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;

namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    /// RuneTek5 (RS3 in NXT & HTML) cache format.
    /// </summary>
	public class RuneTek5Cache : Cache
	{
		private const string CacheFileName = "main_file_cache.dat2";
		private const string IndexFilePrefix = "main_file_cache.idx";

		public override string DefaultCacheDirectory => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jagexcache/runescape/LIVE/";

        public override IEnumerable<int> GetIndexIds()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<int> GetFileIds(int indexId)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<int> GetArchiveFileIds(int indexId, int archiveId)
        {
            throw new NotImplementedException();
        }

        public override byte[] GetFileData(int indexId, int fileId)
        {
            throw new NotImplementedException();
        }

        public override byte[] GetArchiveFileData(int indexId, int archiveId, int fileId)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		///     Obtains the ids of all the present index files.
		/// </summary>
		public static IEnumerable<int> GetArchiveIds()
		{
            var tableFile = new Container();
            var referenceTable = new ReferenceTable();

			return Directory.EnumerateFiles(CacheDirectory, IndexFilePrefix + "???").Select(file =>
			{
				int archiveId;
				if (int.TryParse(file.Substring(file.LastIndexOf(IndexFilePrefix) + IndexFilePrefix.Length), out archiveId))
					return archiveId;

				return -1;
			}).Where(id => id != -1).OrderBy(id => id);
		}

		/// <summary>
		///     Checks for existence of the archive directory in the output directory.
		/// </summary>
		public static bool ArchiveExtracted(int archiveId)
		{
			return Directory.Exists(OutputDirectory + "cache/" + archiveId);
		}

		/// <summary>
		///     Returns a path to a specified output file.
		/// </summary>
		/// <exception cref="FileNotFoundException"></exception>
		public static string GetFile(int archiveId, int fileId, bool extractOnFailure = false)
		{
			//quick filter and then regex filter to decide whether the file is actually what we're looking for
			var file = FindFile(archiveId, fileId);

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
			//find it by the start of the file first, then do a regex filter against the result to filter out false positives (e.g. 5 would match 534.ogg)
			var files =
			Directory.EnumerateFiles($"{OutputDirectory}cache/{archiveId}/", $"{fileId}*")
			.Where(file => Regex.IsMatch(file, $@"(/|\\){fileId}(\..+)?$"))
			.ToList();

			return files.FirstOrDefault();
		}
	}
}
