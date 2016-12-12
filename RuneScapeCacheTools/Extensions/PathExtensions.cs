using System;

namespace Villermen.RuneScapeCacheTools.Extensions
{
    public static class PathExtensions
    {
        public static char[] InvalidCharacters = { '/', ':', '"', '*', '?', '>', '<', '|', '\0' };

        /// <summary>
        ///     Parses the given directory and unifies its format, to be applied to unpredictable user input.
        ///     Converts backslashes to forward slashes, and appends a directory separator.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string FixDirectory(string path)
        {
            // Expand environment variables
            var result = Environment.ExpandEnvironmentVariables(path);

            // Replace backslashes with forward slashes
            result = result.Replace('\\', '/');

            // Add trailing slash if not present
            if (!result.EndsWith("/"))
            {
                result += "/";
            }

            return result;
        }
    }
}