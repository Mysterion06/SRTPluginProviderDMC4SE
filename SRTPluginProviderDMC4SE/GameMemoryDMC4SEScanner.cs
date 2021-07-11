using ProcessMemory;
using static ProcessMemory.Extensions;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using SRTPluginProviderDMC4SE.Structs;

namespace SRTPluginProviderDMC4SE
{
    internal class GameMemoryDMC4SEScanner : IDisposable
    {
        private static readonly int MAX_ENTITIES = 64;
        private static readonly int MAX_ITEMS = 256;

        // Variables
        private ProcessMemoryHandler memoryAccess;
        private GameMemoryDMC4SE gameMemoryValues;
        public bool HasScanned;
        public bool ProcessRunning => memoryAccess != null && memoryAccess.ProcessRunning;
        public int ProcessExitCode => (memoryAccess != null) ? memoryAccess.ProcessExitCode : 0;

        // Pointer Address Variables
        private int pointerAddressMainBase;
        private int pointerAddressMainBase2;
        private int pointerAddressEnemyHP;
        // Pointer Classes
        private IntPtr BaseAddress { get; set; }
        private MultilevelPointer PointerPlayerState { get; set; }
        private MultilevelPointer PointerPlayerStateMax { get; set; }
        private MultilevelPointer PointerPlayerIGT { get; set; }
        private MultilevelPointer PointerPlayerRedOrbs { get; set; }
        private MultilevelPointer PointerPlayerRoomID { get; set; }
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

            SelectPointerAddresses();
            gameMemoryValues._gameInfo = GameHashes.DetectVersion(process.MainModule.FileName);

            //if (!SelectPointerAddresses(GameHashes.DetectVersion(process.MainModule.FileName)))
            //    return; // Unknown version.

            int pid = GetProcessId(process).Value;
            memoryAccess = new ProcessMemoryHandler(pid);
            if (ProcessRunning)
            {
                BaseAddress = NativeWrappers.GetProcessBaseAddress(pid, PInvoke.ListModules.LIST_MODULES_64BIT); // Bypass .NET's managed solution for getting this and attempt to get this info ourselves via PInvoke since some users are getting 299 PARTIAL COPY when they seemingly shouldn't.

                // Setup the pointers.
                PointerPlayerState = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressMainBase), 0x24);
                PointerPlayerStateMax = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressMainBase), 0x24);
                PointerPlayerIGT = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressMainBase));
                PointerPlayerRedOrbs = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressMainBase));
                PointerPlayerRoomID = new MultilevelPointer(memoryAccess, IntPtr.Add(BaseAddress, pointerAddressMainBase2));

                GenerateEnemyEntries();
            }
        }

        private void SelectPointerAddresses()
        {
            pointerAddressMainBase = 0xF59F00;
            pointerAddressMainBase2 = 0x1359F00;
            pointerAddressEnemyHP = 0x0F59F0C;
        }


        private unsafe void GenerateEnemyEntries()
        {
            if (PointerEnemyHP == null)
            {
                PointerEnemyHP = new MultilevelPointer[24];
                for (int i = 0; i < PointerEnemyHP.Length; ++i)
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
            PointerPlayerStateMax.UpdatePointers();
            PointerPlayerIGT.UpdatePointers();
            PointerPlayerRedOrbs.UpdatePointers();
            PointerPlayerRoomID.UpdatePointers();

            GenerateEnemyEntries();
            for (int i = 0; i < PointerEnemyHP.Length; i++)
                PointerEnemyHP[i].UpdatePointers();
        }

        internal unsafe IGameMemoryDMC4SE Refresh()
        {
            bool success;

            // Player HP
            fixed (float* p = &gameMemoryValues._playerHP)
                success = PointerPlayerState.TryDerefFloat(0x1B00, p);

            // Player Max HP
            fixed (float* p = &gameMemoryValues._playerMaxHP)
                success = PointerPlayerStateMax.TryDerefFloat(0x1B04, p);

            // Player DT
            fixed (float* p = &gameMemoryValues._playerDT)
                success = PointerPlayerState.TryDerefFloat(0x2504, p);

            // Player Max DT
            fixed (float* p = &gameMemoryValues._playerMaxDT)
                success = PointerPlayerStateMax.TryDerefFloat(0x2508, p);

            // IGT
            fixed (float* p = &gameMemoryValues._igt)
                success = PointerPlayerIGT.TryDerefFloat(0x264, p);

            // Red Orbs
            fixed (int* p = &gameMemoryValues._redOrbs)
                success = PointerPlayerRedOrbs.TryDerefInt(0x184, p);

            // Room ID
            fixed (int* p = &gameMemoryValues._roomID)
                success = PointerPlayerRedOrbs.TryDerefInt(0x154, p);

            // Current Player
            fixed (int* p = &gameMemoryValues._playerChar)
                success = PointerPlayerState.TryDerefInt(0x19AC, p);

            // FPS
            fixed (byte* p = &gameMemoryValues._fps)
                success = PointerPlayerIGT.TryDerefByte(0x640, p);

            // Enemy HP
            if (gameMemoryValues._enemyHealth == null)
            {
                gameMemoryValues._enemyHealth = new EnemyHP[24];
                for (int i = 0; i < gameMemoryValues._enemyHealth.Length; ++i)
                    gameMemoryValues._enemyHealth[i] = new EnemyHP();
            }
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