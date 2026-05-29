using System;
using System.Runtime.InteropServices;
using ChihuahuaOS.CoreLib;

// ReSharper disable TailRecursiveCall
namespace ChihuahuaOS.Kernel.MemoryManager.PMM;

/// <summary>
/// Size of struct: 8192 bytes.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct ChunkLevel1
{
    private fixed byte _buddy0[4096]; //32 KiB
    private fixed byte _buddy1[2048]; //64 KiB
    private fixed byte _buddy2[1024]; //128 KiB
    private fixed byte _buddy3[512]; //256 KiB
    private fixed byte _buddy4[256]; //512 KiB
    private fixed byte _buddy5[128]; //1 MiB
    private fixed byte _buddy6[64]; //2 MiB
    private fixed byte _buddy7[32]; //4 MiB
    private fixed byte _buddy8[16]; //8 MiB
    private fixed byte _buddy9[8]; //16 MiB
    private fixed byte _buddy10[4]; //32 MiB
    private fixed byte _buddy11[2]; //64 MiB
    private byte _buddy12; //128 MiB
    private byte _buddies13_14_15; //bits 0-3: 256 MiB, bits 4-5: 512 MiB, bit 6: the entire buddy (1 GiB)

    /// <summary>
    /// Will find a free continuous block with the given size (if one exists).
    /// </summary>
    /// <param name="blockSize">The size of the desired block in bytes.</param>
    /// <returns>
    /// The offset of the block's start relative to the entire physical chunk's start (i.e., as if the chunk
    /// started at 0) if the allocation succeeds, -1 if it doesn't.
    /// </returns>
    public long Allocate(long blockSize)
    {
        if (blockSize > 1 << 30)
        {
            return -1;
        }

        if (blockSize > 1 << 29)
        {
            if (((_buddies13_14_15 >> 6) & 1) == 0)
            {
                ToggleAllocations(true, 15, blockSize, 0);
                return 0;
            }

            return -1;
        }

        if (blockSize > 1 << 28)
        {
            if (((_buddies13_14_15 >> 4) & 1) == 0)
            {
                ToggleAllocations(true, 14, blockSize, 0);
                return 0;
            }

            if (((_buddies13_14_15 >> 5) & 1) == 0)
            {
                ToggleAllocations(true, 14, blockSize, 1 << 29);
                return 1 << 29;
            }

            return -1;
        }

        if (blockSize > 1 << 27)
        {
            for (int i = 0; i < 4; i++)
            {
                if (((_buddies13_14_15 >> i) & 1) == 0)
                {
                    long blockOffset = (long)i * (1 << 28);
                    ToggleAllocations(true, 13, blockSize, blockOffset);
                    return blockOffset;
                }
            }

            return -1;
        }

        if (blockSize > 1 << 26)
        {
            if (_buddy12 == 0xFF)
            {
                return -1;
            }

            for (int i = 0; i < 8; i++)
            {
                if (((_buddy12 >> i) & 1) == 0)
                {
                    long blockOffset = (long)i * (1 << 27);
                    ToggleAllocations(true, 12, blockSize, blockOffset);
                    return blockOffset;
                }
            }

            return -1;
        }

        //for the rest of the sizes, we can just make a loop
        for (int multiplier = 0; multiplier < 12; multiplier++)
        {
            //the size to be checked
            long baseSize = 1L << (25 - multiplier);

            byte* buddy = GetBuddyPtr(11 - multiplier);
            if (buddy == null)
            {
                return -1;
            }

            //if smaller than the size, go to the lower level
            if (blockSize < baseSize && multiplier < 11)
            {
                continue;
            }

            //for each byte
            for (int i = 0; i < 1 << (multiplier + 1); i++)
            {
                //fast check, skips 8 blocks at once
                if (buddy[i] == 0xFF)
                {
                    continue;
                }

                //for each bit
                for (int j = 0; j < 8; j++)
                {
                    if (((buddy[i] >> j) & 1) == 0)
                    {
                        long blockOffset = i * (baseSize << 4) + j * (baseSize << 1);
                        ToggleAllocations(true, 11 - multiplier, blockSize, blockOffset);
                        return blockOffset;
                    }
                }
            }

            return -1;
        }

        return -1;
    }


    private void ToggleAllocations(bool setAsAllocated, int buddyIndex, long blockSize, long blockOffset)
    {
        switch (buddyIndex)
        {
            case 15:
            {
                if (setAsAllocated)
                {
                    _buddies13_14_15 |= 1 << 6;
                }
                else
                {
                    _buddies13_14_15 &= ~(1 << 6) & 0xFF;
                }

                ToggleAllocations(setAsAllocated, 14, 1 << 29, 0);
                if (blockSize / (1 << 29) > 0 && blockSize % (1 << 29) > 0)
                {
                    ToggleAllocations(setAsAllocated, 14, blockSize % (1 << 29), 1 << 29);
                }

                return;
            }
            case 14:
            {
                if (setAsAllocated)
                {
                    _buddies13_14_15 |= (byte)(1 << (blockOffset < 1 << 29 ? 4 : 5));
                }
                else
                {
                    _buddies13_14_15 &= (byte)~(1 << (blockOffset < 1 << 29 ? 4 : 5));
                }

                ToggleAllocations(setAsAllocated, 13, 1 << 28, blockOffset);
                if (blockSize / (1 << 28) > 0 && blockSize % (1 << 28) > 0)
                {
                    ToggleAllocations(setAsAllocated, 13, blockSize % (1 << 28), blockOffset + (1 << 28));
                }

                SetHigherBitsAsAllocated(14, blockSize, blockOffset);
                return;
            }
            case 13:
            {
                int index = (int)(blockOffset / (1 << 28));

                if (setAsAllocated)
                {
                    _buddies13_14_15 |= (byte)(1 << index);
                }
                else
                {
                    _buddies13_14_15 &= (byte)~(1 << index);
                }

                ToggleAllocations(setAsAllocated, 12, 1 << 27, blockOffset);
                if (blockSize / (1 << 27) > 0 && blockSize % (1 << 27) > 0)
                {
                    ToggleAllocations(setAsAllocated, 12, blockSize % (1 << 27), blockOffset + (1 << 27));
                }

                SetHigherBitsAsAllocated(13, blockSize, blockOffset);
                return;
            }
            case 12:
            {
                int index = (int)(blockOffset / (1 << 27));

                if (setAsAllocated)
                {
                    _buddy12 |= (byte)(1 << index);
                }
                else
                {
                    _buddy12 &= (byte)~(1 << index);
                }

                ToggleAllocations(setAsAllocated, 11, 1 << 26, blockOffset);
                if (blockSize / (1 << 26) > 0 && blockSize % (1 << 26) > 0)
                {
                    ToggleAllocations(setAsAllocated, 11, blockSize % (1 << 26), blockOffset + (1 << 26));
                }

                SetHigherBitsAsAllocated(12, blockSize, blockOffset);
                return;
            }
        }

        {
            long baseSize = 1L << (25 - (11 - buddyIndex));

            byte* buddy = GetBuddyPtr(buddyIndex);
            if (buddy == null)
            {
                CoreLibManager.Panic((byte*)"Could not get a buddy when allocating physical memory"u8);
                return;
            }

            //this is the bit index, so don't index buddies directly!
            int index = (int)(blockOffset / (baseSize << 1));

            if (setAsAllocated)
            {
                buddy[index / 8] |= (byte)(1 << (index % 8));
            }
            else
            {
                buddy[index / 8] &= (byte)~(1 << (index % 8));
            }

            if (buddyIndex > 0)
            {
                ToggleAllocations(setAsAllocated, buddyIndex - 1, baseSize, blockOffset);
                if (blockSize / baseSize > 0 && blockSize % baseSize > 0)
                {
                    ToggleAllocations(setAsAllocated, buddyIndex - 1, blockSize % baseSize, blockOffset + baseSize);
                }

                SetHigherBitsAsAllocated(buddyIndex, blockSize, blockOffset);
            }
        }
    }

    private void SetHigherBitsAsAllocated(int buddyIndex, long blockSize, long blockOffset)
    {
        //NOTE: this is one index higher (e.g., 28 instead of 27) than the other functions because it's the higher
        // buddy that matters
        if (buddyIndex >= 15)
        {
            return;
        }

        switch (buddyIndex)
        {
            case 14:
            {
                _buddies13_14_15 |= 1 << 6;
                return;
            }
            case 13:
            {
                _buddies13_14_15 |= (byte)(1 << (blockOffset < 1 << 29 ? 4 : 5));
                _buddies13_14_15 |= (byte)(1 << (blockOffset + blockSize < 1 << 29 ? 4 : 5));
                SetHigherBitsAsAllocated(14, blockSize, blockOffset);
                return;
            }
            case 12:
            {
                //between 0 and 3
                int lowIndex = (int)(blockOffset / (1 << 28));
                int highIndex = Math.Max(3, (int)(blockOffset + blockSize / (1 << 28)));

                for (; lowIndex <= highIndex; lowIndex++)
                {
                    _buddies13_14_15 |= (byte)(1 << lowIndex);
                    long newBlockSize = blockSize - blockSize % (1 << 29);
                    if (lowIndex % 2 == 0 && newBlockSize > 0)
                    {
                        SetHigherBitsAsAllocated(
                            13,
                            newBlockSize,
                            Math.Max(1 << 29, blockOffset));
                    }
                }

                return;
            }
            case 11:
            {
                //between 0 and 7
                int lowIndex = (int)(blockOffset / (1 << 27));
                int highIndex = Math.Max(7, (int)(blockOffset + blockSize / (1 << 27)));

                for (; lowIndex <= highIndex; lowIndex++)
                {
                    _buddy12 |= (byte)(1 << lowIndex);
                    long newBlockSize = blockSize - blockSize % (1 << 28);
                    if (lowIndex % 4 == 0 && newBlockSize > 0)
                    {
                        SetHigherBitsAsAllocated(
                            12,
                            newBlockSize,
                            Math.Max(1 << 28, blockOffset));
                    }
                }

                return;
            }
            default:
            {
                byte* buddy = GetBuddyPtr(buddyIndex);
                if (buddy == null)
                {
                    CoreLibManager.Panic((byte*)"Could not get a buddy when allocating physical memory"u8);
                    return;
                }

                uint baseSize = 1U << (26 - (10 - buddyIndex));
                uint multiplier = 16U << (10 - buddyIndex);

                int lowIndex = (int)(blockOffset / baseSize);
                int highIndex = (int)Math.Max(multiplier - 1, (int)((blockOffset + blockSize) / baseSize));

                for (; lowIndex <= highIndex; lowIndex++)
                {
                    int byteIndex = lowIndex / 8;
                    if (lowIndex % 8 == 0 && lowIndex + 7 <= highIndex)
                    {
                        buddy[byteIndex] |= 0xFF;
                        //note that the index is automatically incremented by one at the next iteration, so use
                        // 7 instead of 8
                        lowIndex += 7;
                    }
                    else
                    {
                        buddy[byteIndex] |= (byte)(1 << (lowIndex % 8));
                    }

                    if (lowIndex % (multiplier >> 1) == 0)
                    {
                        long newBlockSize = blockSize - blockSize % (baseSize << 1);
                        if (newBlockSize > 0)
                        {
                            SetHigherBitsAsAllocated(
                                buddyIndex + 1,
                                newBlockSize,
                                Math.Max(baseSize << 1, blockOffset));
                        }
                    }
                }

                return;
            }
        }
    }

    private byte* GetBuddyPtr(int buddyIndex)
    {
        byte* buddy;
        switch (buddyIndex)
        {
            case 0:
            {
                fixed (byte* buddyPtr = _buddy0)
                {
                    buddy = buddyPtr;
                }

                break;
            }
            case 1:
            {
                fixed (byte* buddyPtr = _buddy1)
                {
                    buddy = buddyPtr;
                }

                break;
            }
            case 2:
            {
                fixed (byte* buddyPtr = _buddy2)
                {
                    buddy = buddyPtr;
                }

                break;
            }
            case 3:
            {
                fixed (byte* buddyPtr = _buddy3)
                {
                    buddy = buddyPtr;
                }

                break;
            }
            case 4:
            {
                fixed (byte* buddyPtr = _buddy4)
                {
                    buddy = buddyPtr;
                }

                break;
            }
            case 5:
            {
                fixed (byte* buddyPtr = _buddy5)
                {
                    buddy = buddyPtr;
                }

                break;
            }
            case 6:
            {
                fixed (byte* buddyPtr = _buddy6)
                {
                    buddy = buddyPtr;
                }

                break;
            }
            case 7:
            {
                fixed (byte* buddyPtr = _buddy7)
                {
                    buddy = buddyPtr;
                }

                break;
            }
            case 8:
            {
                fixed (byte* buddyPtr = _buddy8)
                {
                    buddy = buddyPtr;
                }

                break;
            }
            case 9:
            {
                fixed (byte* buddyPtr = _buddy9)
                {
                    buddy = buddyPtr;
                }

                break;
            }
            case 10:
            {
                fixed (byte* buddyPtr = _buddy10)
                {
                    buddy = buddyPtr;
                }

                break;
            }
            case 11:
            {
                fixed (byte* buddyPtr = _buddy11)
                {
                    buddy = buddyPtr;
                }

                break;
            }
            default:
                return null;
        }

        return buddy;
    }
}
