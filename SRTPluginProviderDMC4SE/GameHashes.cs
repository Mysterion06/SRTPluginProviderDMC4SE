using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SRTPluginProviderDMC4SE
{
    /// <summary>
    /// SHA256 hashes for the DMC4SE game executables.
    /// </summary>
    public static class GameHashes
    {
        private static readonly byte[] DevilMayCry4SpecialEditionWW_20190328_1 = new byte[32] { 0x64, 0x2D, 0x11, 0xB9, 0x4F, 0xC8, 0x22, 0x6D, 0x0A, 0x90, 0x63, 0x01, 0xC0, 0xD4, 0x28, 0x36, 0x26, 0x3F, 0xC8, 0x60, 0xC7, 0x51, 0x50, 0x22, 0x72, 0xBF, 0xAC, 0xC7, 0xFE, 0xE5, 0x1E, 0xFC };
        private static readonly byte[] DevilMayCry4SpecialEditionWW_20210727_1 = new byte[32] { 0xED, 0x63, 0xFB, 0xF6, 0x01, 0x84, 0x21, 0x9C, 0x45, 0x5C, 0x38, 0x7B, 0xEB, 0x54, 0x92, 0x69, 0x4D, 0x60, 0xC2, 0x0E, 0xEE, 0x9C, 0xF6, 0x4C, 0xF9, 0xEA, 0xAD, 0x9D, 0x64, 0x79, 0x9A, 0x46 };

        public static GameVersion DetectVersion(string filePath)
        {
            byte[] checksum;
            using (SHA256 hashFunc = SHA256.Create())
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                checksum = hashFunc.ComputeHash(fs);

            if (checksum.SequenceEqual(DevilMayCry4SpecialEditionWW_20190328_1))
            {
                Console.WriteLine("Old Patch");
                return GameVersion.DevilMayCry4SpecialEditionWW_20190328_1;
            }

            else if (checksum.SequenceEqual(DevilMayCry4SpecialEditionWW_20210727_1))
            {
                Console.WriteLine("Latest Release");
                return GameVersion.DevilMayCry4SpecialEditionWW_20210727_1;
            }

            Console.WriteLine("Unknown Version");
            return GameVersion.Unknown;
        }
    }
}