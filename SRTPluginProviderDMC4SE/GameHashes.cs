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

        public static GameVersion DetectVersion(string filePath)
        {
            byte[] checksum;
            using (SHA256 hashFunc = SHA256.Create())
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                checksum = hashFunc.ComputeHash(fs);

            if (checksum.SequenceEqual(DevilMayCry4SpecialEditionWW_20190328_1))
            {
                Console.WriteLine("old_Patch");
                return GameVersion.DevilMayCry4SpecialEditionWW_20190328_1;
            }

            Console.WriteLine("Unknown Version");
            return GameVersion.Unknown;
        }
    }
}