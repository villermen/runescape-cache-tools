using System;

namespace RuneScapeCacheTools
{
	public static class DirectoryHelper
	{
		/// <summary>
		/// Formats a directory, replacing backslashes with forward slashes and adding a trailing slash if absent.
		/// </summary>
		/// <param name="directory"></param>
		/// <returns></returns>
		public static string FormatDirectory(string directory)
		{
			// Expand
			directory = Environment.ExpandEnvironmentVariables(directory);

			// Normalize
			directory = directory.Replace('\\', '/');

			// Prefix with slash
			if (!directory.EndsWith("/"))
				directory += "/";

			return directory;
		}
	}
}
