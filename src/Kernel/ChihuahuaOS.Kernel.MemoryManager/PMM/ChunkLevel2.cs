using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.CoreLib.Extra;

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
            long offset = ((ChunkLevel1*)physAddress)->Allocate(blockSize);
            _descriptors[i].RemainingSize -= blockSize;
            _descriptors[i].Entry &= ~(ulong)Level1Descriptor.Flags.Locked;

            if (offset >= 0)
            {
                return i * (1 << 30) + offset;
            }
        }

        return -1;
    }

    public void Deallocate(long blockSize, long blockOffset)
    {
        if (blockSize > 1 << 30)
        {
            return;
        }

        int descriptorIndex = (int)(blockOffset / (1 << 30));
        if (descriptorIndex < 0 || descriptorIndex >= NUM_DESCRIPTORS)
        {
            return;
        }

        if (_descriptors[descriptorIndex].Entry == 0)
        {
            return;
        }

        ulong physAddress = _descriptors[descriptorIndex].Entry & Level1Descriptor.PHYSICAL_ADDRESS_MASK;

        _descriptors[descriptorIndex].Entry |= (ulong)Level1Descriptor.Flags.Locked;
        ((ChunkLevel1*)physAddress)->Deallocate(blockSize, blockOffset);
        _descriptors[descriptorIndex].RemainingSize += blockSize;
        _descriptors[descriptorIndex].Entry &= ~(ulong)Level1Descriptor.Flags.Locked;
    }

    internal void SetInitiallyAllocatedBits(long blockSize, long blockOffset)
    {
        while (blockSize > 0)
        {
            int descriptorIndex = (int)(blockOffset / (1 << 30));
            if (descriptorIndex < 0 || descriptorIndex >= NUM_DESCRIPTORS)
            {
                return;
            }

            if (_descriptors[descriptorIndex].Entry == 0)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("FATAL ERROR: PMM: Initialize: Descriptor was null. Descriptor index: \0"u8);

                // ReSharper disable once StackAllocInsideLoop
                Span<byte> buffer = stackalloc byte[NumberParserNoAlloc.MAX_SYMBOLS_BASE_10];
                NumberParserNoAlloc.ParseInteger(descriptorIndex, buffer);
                ReadOnlySpan<byte> stringBuffer = buffer;
                Console.WriteLine(stringBuffer);

                CoreLibManager.Panic((byte*)"A descriptor was null\0"u8);
                return;
            }

            ulong physAddress = _descriptors[descriptorIndex].Entry & Level1Descriptor.PHYSICAL_ADDRESS_MASK;

            //NOTE: this normally always runs in a single-threaded environment, so no need to set locks, but we do it
            // anyway for consistency
            _descriptors[descriptorIndex].Entry |= (ulong)Level1Descriptor.Flags.Locked;
            ((ChunkLevel1*)physAddress)->SetInitiallyAllocatedBits(blockSize, blockOffset);
            _descriptors[descriptorIndex].RemainingSize -= blockSize;
            _descriptors[descriptorIndex].Entry &= ~(ulong)Level1Descriptor.Flags.Locked;

            blockSize /= 1 << 30;
            blockOffset += 1 << 30;
        }
    }
}

[InlineArray(ChunkLevel2.NUM_DESCRIPTORS)]
internal struct Level1DescriptorArray
{
    private ChunkLevel2.Level1Descriptor _descriptor;
}
