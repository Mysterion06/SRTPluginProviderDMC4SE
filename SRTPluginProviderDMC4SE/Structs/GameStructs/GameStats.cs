using System.Runtime.InteropServices;

namespace SRTPluginProviderDMC4SE.Structs.GameStructs
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x641)]

    public struct GameStats
    {
        [FieldOffset(0x154)] private int roomID;
        [FieldOffset(0x184)] private int redOrbs;
        [FieldOffset(0x264)] private float frames;
        [FieldOffset(0x640)] private byte fps;

        public int RoomID => roomID;
        public int RedOrbs => redOrbs;
        private float Frames => frames;
        private int FPS => fps == 1 ? 72 : 60;
        public float IGT => Frames / FPS;
    }
}