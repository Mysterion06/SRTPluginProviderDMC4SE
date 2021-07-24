using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
using SRTPluginProviderDMC4SE.Structs;

namespace SRTPluginProviderDMC4SE
{
    public class GameMemoryDMC4SE : IGameMemoryDMC4SE
    {
        // Gamename
        public string GameName => "DMC4SE";

        // Player HP
        public float PlayerHP { get => _playerHP; set => _playerHP = value;  }
        internal float _playerHP;

        // Player Max HP
        public float PlayerMaxHP { get => _playerMaxHP; set => _playerMaxHP = value; }
        internal float _playerMaxHP;

        // Player DT
        public float PlayerDT { get => _playerDT; set => _playerDT = value; }
        internal float _playerDT;

        // Player Max DT
        public float PlayerMaxDT { get => _playerMaxDT; set => _playerMaxDT = value; }
        internal float _playerMaxDT;

        // Current Player
        public int PlayerChar { get => _playerChar; set => _playerChar = value; }
        internal int _playerChar;

        // FPS
        public byte FPS { get => _fps; set => _fps = value; }
        internal byte _fps;

        // IGT
        public float IGT { get => _igt; set => _igt = value; }
        internal float _igt;

        // Red Orbs
        public int RedOrbs { get => _redOrbs; set => _redOrbs = value; }
        internal int _redOrbs;

        // RoomID
        public int RoomID { get => _roomID; set => _roomID = value; }
        internal int _roomID;

        // Enemy HP
        public EnemyHP[] EnemyHealth { get => _enemyHealth; set => _enemyHealth = value; }
        internal EnemyHP[] _enemyHealth;

        // Versioninfo
        public string VersionInfo => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

        // GameInfo
        public string GameInfo { get =>_gameInfo; set => _gameInfo = value; }
        internal string _gameInfo;
    }
}
