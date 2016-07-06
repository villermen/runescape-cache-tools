namespace Villermen.RuneScapeCacheTools.CLI
{
	class Program
	{
		static void Main(string[] args)
		{
			NXTCache cache = new NXTCache();
			cache.OutputDirectory = @"C:/Data/Temp/rsnxtcache/";

			var d = cache.getArchiveIds();

			var e = cache.GetFileData(40, 2628);

		}
	}
}
