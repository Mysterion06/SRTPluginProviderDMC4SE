using System.Runtime.InteropServices;

namespace SRTPluginProviderDMC4SE.Structs.GameStructs
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x250C)]

    public struct GamePlayer
    {
        [FieldOffset(0x19AC)] private int id;
        [FieldOffset(0x1B00)] private float currentHP;
        [FieldOffset(0x1B04)] private float maxHP;
        [FieldOffset(0x2504)] private float currentDT;
        [FieldOffset(0x2508)] private float maxDT;

        public PlayerName ID => (PlayerName)id;
        public string Name => string.Format("{0}: ", ID.ToString());
        public float CurrentHP => currentHP;
        public float MaxHP => maxHP;
        public float CurrentDT => currentDT;
        public float MaxDT => maxDT;
        public bool IsAlive => CurrentHP != 0 && MaxHP != 0 && CurrentHP > 0 && CurrentHP <= MaxHP;
        public float PercentageHP => IsAlive ? (float)CurrentHP / (float)MaxHP : 0f;
        public float PercentageDT => IsAlive ? (float)CurrentDT / (float)MaxDT : 0f;
        public PlayerState HealthState
        {
            get =>
                !IsAlive ? PlayerState.Dead :
                PercentageHP >= 0.66f ? PlayerState.Fine :
                PercentageHP >= 0.33f ? PlayerState.Caution :
                PlayerState.Danger;
        }
    }

    public enum PlayerName : int
    {
        Dante,
        Nero,
        Vergil
    }

    public enum PlayerState
    {
        Dead,
        Fine,
        Caution,
        Danger
    }
}