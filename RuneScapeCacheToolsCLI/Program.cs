using System;
using System.Diagnostics;
using System.IO;
using RuneScapeCacheTools;
using System.Collections.Generic;

namespace Villermen.RuneScapeCacheTools.CLI
{
	class Program
	{
		private const string OutputDirectory = "C:/Data/Temp/rsnxtcache/";

		static void Main(string[] args)
		{
			Directory.CreateDirectory(OutputDirectory);

			NXTCache cache = new NXTCache();
			cache.OutputDirectory = OutputDirectory;

			//var archiveIds = cache.getArchiveIds();
			//Debug.WriteLine(archiveIds);

			//var fileIds = cache.getFileIds(2);
			//Debug.WriteLine(fileIds);

			//cache.ExtractFile(40, 2628);

			//Debug.WriteLine(cache.GetFileOutputPath(40, 2628));

			//cache.ExtractAllAsync().Wait();

			using (var reader = new BinaryReader(File.OpenRead($"{OutputDirectory}cache/17/6")))
			{
				reader.ReadByte(); // 1, for reasons unknown

				// Create a list of enum start positions
				long fileSize = reader.BaseStream.Length;
				Dictionary<int, uint> enumPositions = new Dictionary<int, uint>();
				int enumIndex = 0;

				while (true)
				{
					uint enumPosition = reader.ReadUInt32BigEndian();

					// Last enum position (seems to) always points to the file's size, so we can use that to determine where the positions end
					if (enumPosition == fileSize)
					{
						break;
					}

					// Only add the position if it is pointing to a valid enum
					long returnFilePosition = reader.BaseStream.Position;
					reader.BaseStream.Position = enumPosition;

					// Check validity of enum position by verifying it starts with e(something)f
					byte[] enumBytes = reader.ReadBytes(3);
					if (enumBytes[0] == 0x65 && enumBytes[2] == 0x66)
					{
						enumPositions.Add(enumIndex, enumPosition);
					}

					reader.BaseStream.Position = returnFilePosition;

					enumIndex++;
				}
			}

			Console.ReadLine();
		}

		private void ScriptVarType(char id)
		{
			if (id == 'O')
			{
				Console.WriteLine("OBJ");
			}
		}
	}
}
