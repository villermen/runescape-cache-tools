namespace Villermen.RuneScapeCacheTools
{
	/// <summary>
	/// Processor for cache-obtained file data.
	/// Able to transform data, or guess its extension based on its contents.
	/// </summary>
	public interface IFileProcessor
	{
		/// <summary>
		/// Processes the given file data, transforming it.
		/// </summary>
		/// <param name="fileData"></param>
		void Process(ref byte[] fileData);

		/// <summary>
		/// Tries to supply an extension based on the given file data.
		/// </summary>
		/// <param name="fileData"></param>
		/// <returns>A file extension, or null when no extension could be guessed.</returns>
		string GuessExtension(ref byte[] fileData);
	}
}
