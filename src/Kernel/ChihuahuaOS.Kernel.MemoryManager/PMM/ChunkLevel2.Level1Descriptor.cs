using System.Runtime.InteropServices;

namespace ChihuahuaOS.Kernel.MemoryManager.PMM;

internal partial struct ChunkLevel2
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct Level1Descriptor
    {
        public const ulong PHYSICAL_ADDRESS_MASK = 0xFFFF_FFFF_FFFF_F000;

        internal enum Flags : ulong
        {
            None = 0,
            Locked = 1
        }

        public Level1Descriptor(ulong address, Flags flags, long remainingSize = 1 << 30)
        {
            Entry = (address & PHYSICAL_ADDRESS_MASK) | (ulong)flags;
            RemainingSize = remainingSize;
        }


        /// <summary>
        /// Is composed of the physical address of the level 1 chunk (bits 12-63) together with <see cref="Flags"/>
        /// (bits 0-11).
        /// </summary>
        public ulong Entry;

        public long RemainingSize;
    }
}
