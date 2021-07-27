using System;
using SRTPluginProviderDMC4SE.Structs;
using SRTPluginProviderDMC4SE.Structs.GameStructs;

namespace SRTPluginProviderDMC4SE
{
    public interface IGameMemoryDMC4SE
    {
        // Gamename
        string GameName { get; }

        // Player
        GamePlayer Player { get; set; }
        GameStats Stats { get; set; }
        EnemyHP[] EnemyHealth { get; set; }

        // Versioninfo
        string VersionInfo { get; }

        // GameInfo
        string GameInfo { get; set; }

        TimeSpan IGTTimeSpan { get; }
        string IGTFormattedString { get; }

    }
}