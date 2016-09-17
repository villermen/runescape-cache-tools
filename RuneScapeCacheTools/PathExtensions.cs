using System.IO;

namespace Villermen.RuneScapeCacheTools
{
    public static class PathExtensions
    {
        /// <summary>
        /// Converts backslashes to forward slashes, and appends a directory separator.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string FixDirectory(string path)
        {
            return path.Replace('\\', '/').TrimEnd('/') + '/';
        }
    }
}
