using System;
using System.Collections.Generic;
using System.Linq;

namespace Villermen.RuneScapeCacheTools
{
	public class FileProcessor
	{
		public delegate bool DecompressAction(ref byte[] fileData);

		public delegate string GuessExtensionAction(ref byte[] fileData);

		private IList<DecompressAction> DecompressActions = new List<DecompressAction>();

		private IList<GuessExtensionAction> GuessExtensionActions = new List<GuessExtensionAction>();

		public FileProcessor()
		{
			GuessExtensionActions.Add(GuessOggExtensionAction);
		}

		/// <summary>
		/// Tries to decompress 
		/// </summary>
		/// <param name="fileData"></param>
		public void Decompress(ref byte[] fileData)
		{
			foreach(var decompressAction in DecompressActions)
			{
				if (decompressAction(ref fileData))
				{
					break;
				}
			}
		}

		public string GuessExtension(ref byte[] fileData)
		{
			foreach(var guessExtensionAction in GuessExtensionActions)
			{
				string result = guessExtensionAction(ref fileData);

				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		private string GuessOggExtensionAction(ref byte[] fileData)
		{
			if (fileData.Length < 4)
			{
				return null;
			}

			byte[] magicNumber = new byte[] { 0x4f, 0x67, 0x67, 0x53 }; // OggS
			byte[] actualBytes = new byte[4];
			Array.Copy(fileData, actualBytes, 4);

			if (!actualBytes.SequenceEqual(magicNumber))
			{
				return null;
			}

			return "ogg";
		}
	}
}
