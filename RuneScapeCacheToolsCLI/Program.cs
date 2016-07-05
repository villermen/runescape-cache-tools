namespace Villermen.RuneScapeCacheTools.CLI
{
	class Program
	{
		static void Main(string[] args)
		{
			Cache cache = new NXTCache();
			cache.OutputDirectory = @"C:/Data/Temp/rsnxtcache/";

			var d = cache.getArchiveIds();

		}
	}
}
