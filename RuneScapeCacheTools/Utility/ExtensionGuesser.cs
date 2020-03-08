using System;
using System.Collections.Generic;
using System.Linq;

namespace Villermen.RuneScapeCacheTools.Utility
{
    /// <summary>
    /// Static helper that can determine from a select set of file extensions based on a file's first bytes.
    /// </summary>
    public static class ExtensionGuesser
    {
        private static readonly Dictionary<string, byte[][]> KnownMagicNumbers = new Dictionary<string, byte[][]> {
            {"ogg", new [] { new byte[] { 0x4f, 0x67, 0x67, 0x53 }}}, // OggS
            {"jaga", new [] { new byte[] { 0x4a, 0x41, 0x47, 0x41 }}}, // JAGA
            {"png", new [] { new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }}}, //0x89504e47 + .PNG
            {"gif", new [] { new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }}}, // GIF87a / GIF89a
            {"bmp", new [] { new byte[] { 0x42, 0x4d }}}, // BM
            {"mid", new [] { new byte[] { 0x4d, 0x54, 0x68, 0x64 }}}, // MThd
            {"gz", new [] { new byte[] { 0x1f, 0x8b }}}, // 0x1f8b
            {"bz2", new [] { new byte[] { 0x42, 0x5a, 0x68 }}}, // BZh
            {"tiff", new [] { new byte[] { 0x49, 0x49, 0x2a, 0x00 }, new byte[] { 0x4d, 0x4d, 0x00, 0x2a }}}, // 0x49492a00 / 0x4d4d002a
            {"mp3", new [] { new byte[] { 0xff, 0xfb }, new byte[] {0x49, 0x44, 0x33 }}}, // 0xfffb / ID3
            {"jpg", new [] { new byte[] { 0xff, 0xd8, 0xff }}}, // 0xffd8ff - Actually multiple numbers but they all start with the same bytes.
            {"zip", new [] { new byte[] { 0x50, 0x4b }}}, // 0x504b - Same as above.
            {"wav", new [] { new byte[] { 0x52, 0x49, 0x46, 0x46 }}}, // RIFF - Same as above.
            {"tar", new [] { new byte[] { 0x75, 0x73, 0x74, 0x61, 0x72 }}}, // ustar - Same as above.
            {"7z", new [] { new byte[] { 0x37, 0x7a, 0xbc, 0xaf, 0x27, 0x1c }}}, // 0x377abcaf271c
        };

        public static string? GuessExtension(byte[] fileData)
        {
            foreach (var magicNumberPair in ExtensionGuesser.KnownMagicNumbers)
            {
                foreach (var magicNumber in magicNumberPair.Value)
                {
                    if (ExtensionGuesser.DataHasMagicNumber(fileData, magicNumber))
                    {
                        return magicNumberPair.Key;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns whether a file's data starts with the given magic number bytes.
        /// </summary>
        public static bool DataHasMagicNumber(byte[] fileData, byte[] magicNumber)
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
