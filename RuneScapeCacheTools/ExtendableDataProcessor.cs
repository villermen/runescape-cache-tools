using System;
using System.Collections.Generic;
using System.Linq;

namespace Villermen.RuneScapeCacheTools
{
	/// <summary>
	///		Extendable implementation of IFileProcessor.
	///   Allows dynamic addition and removal of actions.
	/// </summary>
	public class ExtendableDataProcessor : IDataProcessor
	{
		public delegate void DecompressAction(ref byte[] fileData);

		public delegate string GuessExtensionAction(ref byte[] fileData);

		protected IList<DecompressAction> DecompressActions = new List<DecompressAction>();

		protected IList<GuessExtensionAction> GuessExtensionActions = new List<GuessExtensionAction>();

		public ExtendableDataProcessor()
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
				var extension = guessExtensionAction(ref fileData);

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
			if (DataHasMagicNumber(ref fileData, new byte[] {0x4f, 0x67, 0x67, 0x53}))
			{
				return "ogg";
			}

			// jaga (JAGA)
			if (DataHasMagicNumber(ref fileData, new byte[] {0x4a, 0x41, 0x47, 0x41}))
			{
				return "jaga";
			}

			// png (0x89504e470d0a1a0a)
			if (DataHasMagicNumber(ref fileData, new byte[] {0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a}))
			{
				return "png";
			}

			// gif (GIF87a and GIF89a)
			if (DataHasMagicNumber(ref fileData, new byte[] {0x47, 0x49, 0x46, 0x38, 0x37, 0x61}) ||
			    DataHasMagicNumber(ref fileData, new byte[] {0x47, 0x49, 0x46, 0x38, 0x39, 0x61}))
			{
				return "gif";
			}

			// bmp (BM)
			if (DataHasMagicNumber(ref fileData, new byte[] {0x42, 0x4d}))
			{
				return "bmp";
			}

			// midi (MThd)
			if (DataHasMagicNumber(ref fileData, new byte[] {0x4d, 0x54, 0x68, 0x64}))
			{
				return "mid";
			}

			// gzip (0x1f8b)
			if (DataHasMagicNumber(ref fileData, new byte[] {0x1f, 0x8b}))
			{
				return "gz";
			}

			// bzip2 (BZh)
			if (DataHasMagicNumber(ref fileData, new byte[] {0x42, 0x5a, 0x68}))
			{
				return "bz2";
			}

			// tiff (0x49492a00 and 0x4d4d002a)
			if (DataHasMagicNumber(ref fileData, new byte[] {0x49, 0x49, 0x2a, 0x00}) ||
			    DataHasMagicNumber(ref fileData, new byte[] {0x4d, 0x4d, 0x00, 0x2a}))
			{
				return "tiff";
			}

			// mp3 (0xfffb and ID3).
			if (DataHasMagicNumber(ref fileData, new byte[] {0xff, 0xfb}) ||
			    DataHasMagicNumber(ref fileData, new byte[] {0x49, 0x44, 0x33}))
			{
				return "wav";
			}

			// jpeg (0xffd8ff). Actually multiple numbers, but they all start with the same bytes anyway.
			if (DataHasMagicNumber(ref fileData, new byte[] {0xff, 0xd8, 0xff}))
			{
				return "jpg";
			}

			// zip (0x504b). Same thing here.
			if (DataHasMagicNumber(ref fileData, new byte[] {0x50, 0x4b}))
			{
				return "zip";
			}

			// wav (RIFF). Same thing here.
			if (DataHasMagicNumber(ref fileData, new byte[] {0x52, 0x49, 0x46, 0x46}))
			{
				return "wav";
			}

			// tar (ustar). Same thing here.
			if (DataHasMagicNumber(ref fileData, new byte[] {0x75, 0x73, 0x74, 0x61, 0x72}))
			{
				return "tar";
			}

			// 7zip (0x377abcaf271c)
			if (DataHasMagicNumber(ref fileData, new byte[] {0x37, 0x7a, 0xbc, 0xaf, 0x27, 0x1c}))
			{
				return "7z";
			}

			return null;
		}

		/// <summary>
		///   Helper method to test if a file starts with given bytes.
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

			var actualBytes = new byte[magicNumber.Length];
			Array.Copy(fileData, actualBytes, magicNumber.Length);

			return actualBytes.SequenceEqual(magicNumber);
		}
	}
}