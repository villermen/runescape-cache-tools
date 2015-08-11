using System;

namespace RuneScapeCacheTools
{
	public static class DirectoryHelper
	{
		/// <summary>
		///     Formats a directory, replacing backslashes with forward slashes and adding a trailing slash if absent.
		/// </summary>
		public static string FormatDirectory(string directory)
		{
			//format directory
			directory = Environment.ExpandEnvironmentVariables(directory);
			directory = directory.Replace('\\', '/');

			if (!directory.EndsWith("/"))
				directory += "/";

			return directory;
		}
	}
}
