namespace Villermen.RuneScapeCacheTools.Cache
{
	/// <summary>
	///   Processor for cache-obtained file data.
	///   Able to guess an extension based on the supplied data.
	/// </summary>
	public interface IExtensionGuesser
	{
		/// <summary>
		///   Tries to supply an extension based on the given file data.
		/// </summary>
		/// <param name="fileData"></param>
		/// <returns>A file extension, or null when no extension could be guessed.</returns>
		string GuessExtension(byte[] fileData);
	}
}