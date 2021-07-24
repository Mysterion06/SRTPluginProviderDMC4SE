using System;
using SRTPluginProviderDMC4SE.Structs;

namespace SRTPluginProviderDMC4SE
{
    public interface IGameMemoryDMC4SE
    {
        // Gamename
        string GameName { get; }

        // Player HP
        float PlayerHP { get; set; }

        // Player Max HP
        float PlayerMaxHP { get; set; }

        // Player DT
        float PlayerDT { get; set; }

        // Player DT HP
        float PlayerMaxDT { get; set; }

        // Current Player
        int PlayerChar { get; set; }

        //Current FPS 1 = 60 2 = 120
        byte FPS { get; set; }

        // IGT
        float IGT { get; set; }

        // Red Orbs
        int RedOrbs { get; set; }

        // RoomID
        int RoomID { get; set; }

        // Versioninfo
        string VersionInfo { get; }

        // GameInfo
        string GameInfo { get; set; }
    }
}