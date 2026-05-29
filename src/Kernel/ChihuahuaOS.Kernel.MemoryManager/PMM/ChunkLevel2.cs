using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ChihuahuaOS.Kernel.MemoryManager.PMM;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe partial struct ChunkLevel2
{
    internal const int NUM_DESCRIPTORS = 1024;

    private Level1DescriptorArray _descriptors;

    public Level1Descriptor this[int index]
    {
        get => _descriptors[index];
        set => _descriptors[index] = value;
    }

    public long Allocate(long blockSize)
    {
        //TODO (later): busy-wait for the unlocking when we have SMP
        for (int i = 0; i < NUM_DESCRIPTORS; i++)
        {
            //this will be the first null entry, so return
            if (_descriptors[i].Entry == 0)
            {
                return -1;
            }

            if (_descriptors[i].RemainingSize < blockSize
                || (_descriptors[i].Entry & (ulong)Level1Descriptor.Flags.Locked) != 0)
            {
                continue;
            }

            ulong physAddress = _descriptors[i].Entry & Level1Descriptor.PHYSICAL_ADDRESS_MASK;

            _descriptors[i].Entry |= (ulong)Level1Descriptor.Flags.Locked;
            long offset = (*(ChunkLevel1*)physAddress).Allocate(blockSize);
            _descriptors[i].Entry &= ~(ulong)Level1Descriptor.Flags.Locked;

            if (offset >= 0)
            {
                return i * (1 << 30) + offset;
            }
        }

        return -1;
    }
}

[InlineArray(ChunkLevel2.NUM_DESCRIPTORS)]
internal struct Level1DescriptorArray
{
    private ChunkLevel2.Level1Descriptor _descriptor;
}
