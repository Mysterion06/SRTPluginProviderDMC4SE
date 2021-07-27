using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
using SRTPluginProviderDMC4SE.Structs;
using SRTPluginProviderDMC4SE.Structs.GameStructs;

namespace SRTPluginProviderDMC4SE
{
    public class GameMemoryDMC4SE : IGameMemoryDMC4SE
    {
        private const string IGT_TIMESPAN_STRING_FORMAT = @"hh\:mm\:ss";

        // Gamename
        public string GameName => "DMC4SE";
        
        // Player
        public GamePlayer Player { get => _player; set => _player = value; }
        internal GamePlayer _player;

        public GameStats Stats { get => _stats; set => _stats = value; }
        internal GameStats _stats;

        // Enemy HP
        public EnemyHP[] EnemyHealth { get => _enemyHealth; set => _enemyHealth = value; }
        internal EnemyHP[] _enemyHealth;

        // Versioninfo
        public string VersionInfo => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

        // GameInfo
        public string GameInfo { get =>_gameInfo; set => _gameInfo = value; }
        internal string _gameInfo;

        public TimeSpan IGTTimeSpan
        {
            get
            {
                TimeSpan timespanIGT;

                if (Stats.IGT >= 0f)
                    timespanIGT = TimeSpan.FromSeconds(Stats.IGT);
                else
                    timespanIGT = new TimeSpan();

                return timespanIGT;
            }
        }

        public string IGTFormattedString => IGTTimeSpan.ToString(IGT_TIMESPAN_STRING_FORMAT, CultureInfo.InvariantCulture);
    }
}
