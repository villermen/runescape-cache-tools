using System.IO;
using System.Linq;
using Serilog;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.File
{
    public class NxtModelFile
    {
        public static NxtModelFile Decode(byte[] data)
        {
            var file = new NxtModelFile();

            using var dataStream = new MemoryStream(data);
            using var dataReader = new BinaryReader(dataStream);

            // Note that this all seems to use little-endian encoding as opposed to most other file formats.
            do
            {
                var opcode = dataReader.ReadByte();
                Log.Debug($"Opcode: {opcode}.");
                switch (opcode)
                {
                    case 1:
                        var size1 = dataReader.ReadUInt16();
                        var unknown1 = dataReader.ReadUInt16();
                        var array1 = new ushort[size1];
                        for (var i = 0; i < size1; i++)
                        {
                            array1[i] = dataReader.ReadUInt16();
                        }
                        break;

                    case 2: // Unknown (version?, header?) start of every file
                        break;

                    case 3: // Unknown after start of every file
                        var unknown3 = dataReader.ReadBytesExactly(13);
                        var size3 = dataReader.ReadUInt16();
                        var array3A = new ushort[size3];
                        for (var i = 0; i < size3; i++)
                        {
                            array3A[i] = dataReader.ReadUInt16();
                        }
                        var array3B = new byte[size3];
                        for (var i = 0; i < size3; i++)
                        {
                            array3B[i] = dataReader.ReadByte();
                        }
                        var array3C = new ushort[size3];
                        for (var i = 0; i < size3; i++)
                        {
                            array3C[i] = dataReader.ReadUInt16();
                        }
                        break;

                    default:
                        Log.Debug("Unknown opcode.");
                        return file;
                }
            }
            while (dataStream.Position < dataStream.Length);

            Log.Debug("We went through the whole file? Highly unlikely.");

            return file;
        }
    }
}
