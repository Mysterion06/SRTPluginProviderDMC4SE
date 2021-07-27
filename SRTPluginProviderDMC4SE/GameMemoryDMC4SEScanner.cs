using ProcessMemory;
using static ProcessMemory.Extensions;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using SRTPluginProviderDMC4SE.Structs;
using SRTPluginProviderDMC4SE.Structs.GameStructs;

namespace SRTPluginProviderDMC4SE
{
    internal class GameMemoryDMC4SEScanner : IDisposable
    {
        private static readonly int MAX_ENTITIES = 24;

        // Variables
        private ProcessMemoryHandler memoryAccess;
        private GameMemoryDMC4SE gameMemoryValues;
        public bool HasScanned;
        public bool ProcessRunning => memoryAccess != null && memoryAccess.ProcessRunning;
        public int ProcessExitCode => (memoryAccess != null) ? memoryAccess.ProcessExitCode : 0;

        // Pointer Address Variables
        private int pointerAddressMainBase;
        private int pointerAddressEnemyHP;

        // Pointer Classes
        private IntPtr BaseAddress { get; set; }
        private MultilevelPointer PointerPlayerState { get; set; }
        private MultilevelPointer PointerGameStats { get; set; }
        private MultilevelPointer[] PointerEnemyHP { get; set; }
        
        internal GameMemoryDMC4SEScanner(Process process = null)
        {
            gameMemoryValues = new GameMemoryDMC4SE();
            if (process != null)
                Initialize(process);
        }

        internal unsafe void Initialize(Process process)
        {
            if (process == null)
                return; // Do not continue if this is null.

            SelectPointerAddresses(GameHashes.DetectVersion(process.MainModule.FileName));

            //if (!SelectPointerAddresses(GameHashes.DetectVersion(process.MainModule.FileName)))
            //    return; // Unknown version.

            int pid = GetProcessId(process).Value;
            memoryAccess = new ProcessMemoryHandler(pid);
            if (ProcessRunning)
            {
                BaseAddress = NativeWrappers.GetProcessBaseAddress(pid, PInvoke.ListModules.LIST_MODULES_64BIT); // Bypass .NET's managed solution for getting this and attempt to get this info ourselves via PInvoke since some users are getting 299 PARTIAL COPY when they seemingly shouldn't.

                // Setup the pointers.
                PointerPlayerState = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressMainBase), 0x24);
                PointerGameStats = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressMainBase));

                gameMemoryValues._enemyHealth = new EnemyHP[MAX_ENTITIES];
                for (int i = 0; i < MAX_ENTITIES; ++i)
                    gameMemoryValues._enemyHealth[i] = new EnemyHP();

                GenerateEnemyEntries();
            }
        }

        private void SelectPointerAddresses(GameVersion gv)
        {
            if (gv == GameVersion.DevilMayCry4SpecialEditionWW_20190328_1)
            {
                gameMemoryValues._gameInfo = "Old Patch";
                pointerAddressMainBase = 0xF59F00;
                pointerAddressEnemyHP = 0xF59F0C;
            }
            else if (gv == GameVersion.DevilMayCry4SpecialEditionWW_20210727_1)
            {
                gameMemoryValues._gameInfo = "Latest Release";
                pointerAddressMainBase = 0xEDEEC4;
                pointerAddressEnemyHP = 0xEEEED4;
            }
            else
            {
                gameMemoryValues._gameInfo = "Unknown Release May Not Work. Contact Developers";
                pointerAddressMainBase = 0xEDEEC4;
                pointerAddressEnemyHP = 0xEEEED4;
            }
        }

        private unsafe void GenerateEnemyEntries()
        {
            if (PointerEnemyHP == null)
            {
                PointerEnemyHP = new MultilevelPointer[MAX_ENTITIES];
                for (int i = 0; i < MAX_ENTITIES; ++i)
                {
                    int[] offsets = new int[i + 1];
                    offsets[0] = 0x330;
                    for (int j = 1; j < offsets.Length; ++j)
                        offsets[j] = 0x4;
                    PointerEnemyHP[i] = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressEnemyHP), offsets);
                }
            }
        }
        
        internal void UpdatePointers()
        {
            PointerPlayerState.UpdatePointers();
            PointerGameStats.UpdatePointers();

            GenerateEnemyEntries();
            for (int i = 0; i < MAX_ENTITIES; i++)
                PointerEnemyHP[i].UpdatePointers();
        }

        internal unsafe IGameMemoryDMC4SE Refresh()
        {
            gameMemoryValues._player = PointerPlayerState.Deref<GamePlayer>(0x0);
            gameMemoryValues._stats = PointerGameStats.Deref<GameStats>(0x0);

            // Enemy HP
            for (int i = 0; i < gameMemoryValues._enemyHealth.Length; ++i)
            {
                try
                {
                    // Check to see if the pointer is currently valid. It can become invalid when rooms are changed.
                    if (!PointerEnemyHP[i].IsNullPointer)
                    {
                        gameMemoryValues.EnemyHealth[i]._currentHP = PointerEnemyHP[i].DerefFloat(0x2C);
                        gameMemoryValues.EnemyHealth[i]._maximumHP = PointerEnemyHP[i].DerefFloat(0x28);
                    }
                    else
                    {
                        // Clear these values out so stale data isn't left behind when the pointer address is no longer value and nothing valid gets read.
                        // This happens when the game removes pointers from the table (map/room change).
                        gameMemoryValues.EnemyHealth[i]._maximumHP = 0;
                        gameMemoryValues.EnemyHealth[i]._currentHP = 0;
                    }
                }
                catch
                {
                    gameMemoryValues.EnemyHealth[i]._maximumHP = 0;
                    gameMemoryValues.EnemyHealth[i]._currentHP = 0;
                }
            }

            HasScanned = true;
            return gameMemoryValues;
        }

        private int? GetProcessId(Process process) => process?.Id;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls


        private unsafe bool SafeReadByteArray(IntPtr address, int size, out byte[] readBytes)
        {
            readBytes = new byte[size];
            fixed (byte* p = readBytes)
            {
                return memoryAccess.TryGetByteArrayAt(address, size, p);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (memoryAccess != null)
                        memoryAccess.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~REmake1Memory() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}