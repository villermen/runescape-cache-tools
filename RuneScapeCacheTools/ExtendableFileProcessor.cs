using System;
using System.Collections.Generic;
using System.Linq;

namespace Villermen.RuneScapeCacheTools
{
	/// <summary>
	/// Extendable implementation of IFileProcessor.
	/// Allows dynamic addition and removal of actions.
	/// </summary>
	public class ExtendableFileProcessor : IFileProcessor
	{
		public delegate void DecompressAction(ref byte[] fileData);

		public delegate string GuessExtensionAction(ref byte[] fileData);

		protected IList<DecompressAction> DecompressActions = new List<DecompressAction>();

		protected IList<GuessExtensionAction> GuessExtensionActions = new List<GuessExtensionAction>();

		public ExtendableFileProcessor()
		{
			GuessExtensionActions.Add(GuessExtensionsAction);
		}

		public void Process(ref byte[] fileData)
		{
			foreach (var decompressAction in DecompressActions)
			{
				decompressAction(ref fileData);
			}
		}

		public string GuessExtension(ref byte[] fileData)
		{
			foreach (var guessExtensionAction in GuessExtensionActions)
			{
				string extension = guessExtensionAction(ref fileData);

				if (extension != null)
				{
					return extension;
				}
			}

			return null;
		}

		private string GuessExtensionsAction(ref byte[] fileData)
		{
			// ogg (OggS)
			if (DataHasMagicNumber(ref fileData, new byte[] { 0x4f, 0x67, 0x67, 0x53 }))
			{
				return "ogg";
			}

			// jaga (JAGA)
			if (DataHasMagicNumber(ref fileData, new byte[] { 0x4a, 0x41, 0x47, 0x41 }))
			{
				return "jaga";
			}

			// png (0x89504e470d0a1a0a)
			if (DataHasMagicNumber(ref fileData, new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }))
			{
				return "png";
			}

			// gif (GIF87a and GIF89a)
			if (DataHasMagicNumber(ref fileData, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }) ||
				DataHasMagicNumber(ref fileData, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }))
			{
				return "gif";
			}

			// bmp (BM)
			if (DataHasMagicNumber(ref fileData, new byte[] { 0x42, 0x4d }))
			{
				return "bmp";
			}

			// midi (MThd)
			if (DataHasMagicNumber(ref fileData, new byte[] { 0x4d, 0x54, 0x68, 0x64 }))
			{
				return "mid";
			}

			// gzip (0x1f8b)
			if (DataHasMagicNumber(ref fileData, new byte[] { 0x1f, 0x8b }))
			{
				return "gz";
			}

			// bzip2 (BZh)
			if (DataHasMagicNumber(ref fileData, new byte[] { 0x42, 0x5a, 0x68 }))
			{
				return "bz2";
			}

			return null;
		}

		/// <summary>
		/// Helper method to test if a file starts with given bytes.
		/// </summary>
		/// <param name="fileData"></param>
		/// <param name="magicNumber"></param>
		/// <returns></returns>
		protected bool DataHasMagicNumber(ref byte[] fileData, byte[] magicNumber)
		{
			// It can't have the magic number if it doesn't even have enough bytes now can it?
			if (fileData.Length < magicNumber.Length)
			{
				return false;
			}

			byte[] actualBytes = new byte[magicNumber.Length];
			Array.Copy(fileData, actualBytes, magicNumber.Length);

			return actualBytes.SequenceEqual(magicNumber);
		}
	}
}
