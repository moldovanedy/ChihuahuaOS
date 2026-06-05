using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable TailRecursiveCall
namespace ChihuahuaOS.Kernel.MemoryManager.PMM;

/// <summary>
/// Size of struct: 8192 bytes.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct ChunkLevel1
{
    public const int MIN_CHUNK_SIZE = 1 << 15;

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
                SetHigherBitsAsAllocated(blockSize, 0);
                return 0;
            }

            if (((_buddies13_14_15 >> 5) & 1) == 0)
            {
                ToggleAllocations(true, 14, blockSize, 1 << 29);
                SetHigherBitsAsAllocated(blockSize, 1 << 29);
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
                    SetHigherBitsAsAllocated(blockSize, blockOffset);
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
                    SetHigherBitsAsAllocated(blockSize, blockOffset);
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

            //if smaller than or equal to the size, go to the lower level
            if (blockSize <= baseSize && multiplier < 11)
            {
                continue;
            }

            //for each byte
            for (int i = 0; i < 1 << (multiplier + 1); i++)
            {
                //fast check, skips 8 blocks at once
                if (GetBuddyByte(11 - multiplier, i) == 0xFF)
                {
                    continue;
                }

                //for each bit
                for (int j = 0; j < 8; j++)
                {
                    if (((GetBuddyByte(11 - multiplier, i) >> j) & 1) == 0)
                    {
                        long blockOffset = i * (baseSize << 4) + j * (baseSize << 1);
                        ToggleAllocations(true, 11 - multiplier, blockSize, blockOffset);
                        SetHigherBitsAsAllocated(blockSize, blockOffset);
                        return blockOffset;
                    }
                }
            }

            return -1;
        }

        return -1;
    }

    public void Deallocate(long blockSize, long blockOffset)
    {
        if (blockSize > 1 << 30 || blockOffset > 1 << 30)
        {
            return;
        }

        for (int i = 0; i < 15; i++)
        {
            long baseSize = 1L << (29 - i);
            if (blockSize <= baseSize)
            {
                continue;
            }

            //first, toggle the lower buddies
            ToggleAllocations(
                false,
                14 - i,
                Math.Min(blockSize, baseSize) - blockOffset % baseSize,
                blockOffset);

            if (blockSize / baseSize == 1 && blockSize % baseSize > 0)
            {
                ToggleAllocations(
                    false,
                    14 - i,
                    blockSize % baseSize,
                    blockOffset + baseSize);
            }
            else if (blockSize / baseSize >= 2)
            {
                ToggleAllocations(
                    false,
                    14 - i,
                    baseSize,
                    blockOffset + baseSize);
            }

            //then, set the current buddy
            int index = (int)(blockOffset / (baseSize << 1));
            switch (15 - i)
            {
                case 15:
                    _buddies13_14_15 &= 0xFF & ~(1 << 6);
                    break;
                case 14:
                    _buddies13_14_15 &= (byte)~(1 << (blockOffset < 1 << 29 ? 4 : 5));
                    break;
                case 13:
                    _buddies13_14_15 &= (byte)~(1 << Math.Min(3, index));
                    break;
                case 12:
                    _buddy12 &= (byte)~(1 << Math.Min(7, index));
                    break;
                default:
                {
                    int byteIndex = index / 8;
                    SetBuddyByte(
                        15 - i,
                        byteIndex,
                        (byte)(GetBuddyByte(15 - i, byteIndex) & ~(1 << (index % 8))));
                    break;
                }
            }

            //finally, set the upper buddies
            //NOTE: start from this buddy, not the one lower as the previous functions
            SetHigherBitsAsDeallocatedIfNeeded(15 - i, blockOffset);
            return;
        }
    }

    /// <summary>
    /// Sets the initially allocated bits, practically initializing the chunk. This should only be used at the start,
    /// when traversing the EFI map data and setting the occupied bits. 
    /// </summary>
    /// <param name="blockSize"></param>
    /// <param name="blockOffset"></param>
    internal void SetInitiallyAllocatedBits(long blockSize, long blockOffset)
    {
        for (int i = 0; i < 16; i++)
        {
            long baseSize = 1L << (29 - i);
            if (blockSize <= baseSize && i < 15)
            {
                continue;
            }

            ToggleAllocations(true, 15 - i, blockSize, blockOffset);
            return;
        }
    }


    private void ToggleAllocations(bool setAsAllocated, int buddyIndex, long blockSize, long blockOffset)
    {
        switch (buddyIndex)
        {
            case 15:
            {
                const int BASE_SIZE = 1 << 29;
                if (setAsAllocated)
                {
                    _buddies13_14_15 |= 1 << 6;
                }
                else
                {
                    _buddies13_14_15 &= ~(1 << 6) & 0xFF;
                }

                ToggleAllocations(setAsAllocated, 14, Math.Min(BASE_SIZE, blockSize), blockOffset);
                if (blockSize / BASE_SIZE == 1 && blockSize % BASE_SIZE > 0)
                {
                    ToggleAllocations(setAsAllocated, 14, blockSize % BASE_SIZE, blockOffset + BASE_SIZE);
                }
                else if (blockSize / BASE_SIZE >= 2)
                {
                    ToggleAllocations(setAsAllocated, 14, BASE_SIZE, blockOffset + BASE_SIZE);
                }

                return;
            }
            case 14:
            {
                const int BASE_SIZE = 1 << 28;
                if (setAsAllocated)
                {
                    _buddies13_14_15 |= (byte)(1 << (blockOffset < BASE_SIZE << 1 ? 4 : 5));
                }
                else
                {
                    _buddies13_14_15 &= (byte)~(1 << (blockOffset < BASE_SIZE << 1 ? 4 : 5));
                }

                ToggleAllocations(setAsAllocated, 13, Math.Min(BASE_SIZE, blockSize), blockOffset);
                if (blockSize / BASE_SIZE == 1 && blockSize % BASE_SIZE > 0)
                {
                    ToggleAllocations(setAsAllocated, 13, blockSize % BASE_SIZE, blockOffset + BASE_SIZE);
                }
                else if (blockSize / BASE_SIZE >= 2)
                {
                    ToggleAllocations(setAsAllocated, 13, BASE_SIZE, blockOffset + BASE_SIZE);
                }

                return;
            }
            case 13:
            {
                const int BASE_SIZE = 1 << 27;
                int index = (int)(blockOffset / (BASE_SIZE << 1));

                if (setAsAllocated)
                {
                    _buddies13_14_15 |= (byte)(1 << index);
                }
                else
                {
                    _buddies13_14_15 &= (byte)~(1 << index);
                }

                ToggleAllocations(setAsAllocated, 12, Math.Min(BASE_SIZE, blockSize), blockOffset);
                if (blockSize / BASE_SIZE == 1 && blockSize % BASE_SIZE > 0)
                {
                    ToggleAllocations(setAsAllocated, 12, blockSize % BASE_SIZE, blockOffset + BASE_SIZE);
                }
                else if (blockSize / BASE_SIZE >= 2)
                {
                    ToggleAllocations(setAsAllocated, 12, BASE_SIZE, blockOffset + BASE_SIZE);
                }

                return;
            }
            case 12:
            {
                const int BASE_SIZE = 1 << 26;
                int index = (int)(blockOffset / (BASE_SIZE << 1));

                if (setAsAllocated)
                {
                    _buddy12 |= (byte)(1 << index);
                }
                else
                {
                    _buddy12 &= (byte)~(1 << index);
                }

                ToggleAllocations(setAsAllocated, 11, Math.Min(BASE_SIZE, blockSize), blockOffset);
                if (blockSize / BASE_SIZE == 1 && blockSize % BASE_SIZE > 0)
                {
                    ToggleAllocations(setAsAllocated, 11, blockSize % BASE_SIZE, blockOffset + BASE_SIZE);
                }
                else if (blockSize / BASE_SIZE >= 2)
                {
                    ToggleAllocations(setAsAllocated, 11, BASE_SIZE, blockOffset + BASE_SIZE);
                }

                return;
            }
        }

        {
            long baseSize = 1L << (25 - (11 - buddyIndex));
            //this is the bit index, so don't index buddies directly!
            int index = (int)(blockOffset / (baseSize << 1));

            if (setAsAllocated)
            {
                SetBuddyByte(
                    buddyIndex,
                    index / 8,
                    (byte)(GetBuddyByte(buddyIndex, index / 8) | (1 << (index % 8))));
            }
            else
            {
                SetBuddyByte(
                    buddyIndex,
                    index / 8,
                    (byte)(GetBuddyByte(buddyIndex, index / 8) & ~(1 << (index % 8))));
            }

            if (buddyIndex > 0)
            {
                ToggleAllocations(setAsAllocated, buddyIndex - 1, Math.Min(baseSize, blockSize), blockOffset);
                if (blockSize / baseSize == 1 && blockSize % baseSize > 0)
                {
                    ToggleAllocations(setAsAllocated, buddyIndex - 1, blockSize % baseSize, blockOffset + baseSize);
                }
                else if (blockSize / baseSize >= 2)
                {
                    ToggleAllocations(setAsAllocated, buddyIndex - 1, baseSize, blockOffset + baseSize);
                }
            }
        }
    }

    private void SetHigherBitsAsAllocated(long blockSize, long givenOffset)
    {
        switch (blockSize)
        {
            case > 1 << 29:
                return;
            case > 1 << 28:
                _buddies13_14_15 |= 1 << 6;
                break;
            case > 1 << 27:
            {
                _buddies13_14_15 |= (byte)(1 << (givenOffset < 1 << 29 ? 4 : 5));
                SetHigherBitsAsAllocated(1 << 29, givenOffset);
                break;
            }
            case > 1 << 26:
            {
                int index = (int)(givenOffset / (1 << 28));
                _buddies13_14_15 |= (byte)(1 << index);
                SetHigherBitsAsAllocated(1 << 28, givenOffset);
                break;
            }
            case > 1 << 25:
            {
                int index = (int)(givenOffset / (1 << 27));
                _buddy12 |= (byte)(1 << index);
                SetHigherBitsAsAllocated(1 << 27, givenOffset);
                break;
            }
            default:
            {
                //for the rest of the sizes, we can just make a loop
                for (int multiplier = 0; multiplier < 11; multiplier++)
                {
                    long baseSize = 1 << (24 - multiplier);
                    if (blockSize <= baseSize && multiplier < 10)
                    {
                        continue;
                    }

                    //bit index, don't index buddies directly
                    int index = (int)(givenOffset / (baseSize << 2));
                    int byteIndex = index / 8;

                    SetBuddyByte(
                        11 - multiplier,
                        byteIndex,
                        (byte)(GetBuddyByte(11 - multiplier, byteIndex) | (1 << (index % 8))));

                    SetHigherBitsAsAllocated(baseSize << 2, givenOffset);
                    return;
                }

                break;
            }
        }
    }

    private void SetHigherBitsAsDeallocatedIfNeeded(int buddyIndex, long givenOffset)
    {
        while (true)
        {
            if (buddyIndex > 14)
            {
                return;
            }

            switch (buddyIndex)
            {
                case 14:
                {
                    if (((_buddies13_14_15 >> 4) & 1) == 0 && ((_buddies13_14_15 >> 5) & 1) == 0)
                    {
                        _buddies13_14_15 &= 0xFF & ~(1 << 6);
                    }

                    return;
                }
                case 13:
                {
                    //we get the index of this buddy base size and check the adjacent one (it is always even, then odd)
                    // if both are 0, we also set the higher bit to 0 and recursively go to the upper buddy
                    const int BASE_SIZE = 1 << 28;
                    const int UPPER_BASE_SIZE = 1 << 29;
                    int index = (int)(givenOffset / BASE_SIZE);

                    bool bit1 = ((_buddies13_14_15 >> index) & 1) != 0;
                    bool bit2;
                    if (index % 2 == 0)
                    {
                        bit2 = ((_buddies13_14_15 >> (index + 1)) & 1) != 0;
                    }
                    else
                    {
                        bit2 = ((_buddies13_14_15 >> (index - 1)) & 1) != 0;
                    }

                    if (!bit1 && !bit2)
                    {
                        int upperIndex = (int)(givenOffset / UPPER_BASE_SIZE);
                        _buddies13_14_15 &= (byte)~(1 << (4 + Math.Min(1, upperIndex)));
                    }
                    else
                    {
                        return;
                    }

                    buddyIndex = 14;
                    continue;
                }
                case 12:
                {
                    const int BASE_SIZE = 1 << 27;
                    const int UPPER_BASE_SIZE = 1 << 28;
                    int index = (int)(givenOffset / BASE_SIZE);

                    bool bit1 = ((_buddy12 >> index) & 1) != 0;
                    bool bit2;
                    if (index % 2 == 0)
                    {
                        bit2 = ((_buddy12 >> (index + 1)) & 1) != 0;
                    }
                    else
                    {
                        bit2 = ((_buddy12 >> (index - 1)) & 1) != 0;
                    }

                    if (!bit1 && !bit2)
                    {
                        int upperIndex = (int)(givenOffset / UPPER_BASE_SIZE);
                        _buddies13_14_15 &= (byte)~(1 << Math.Min(3, upperIndex));
                    }
                    else
                    {
                        return;
                    }

                    buddyIndex = 13;
                    continue;
                }
                case 11:
                {
                    const int BASE_SIZE = 1 << 26;
                    const int UPPER_BASE_SIZE = 1 << 27;

                    int index = (int)(givenOffset / BASE_SIZE);
                    int byteIndex = index / 8;

                    bool bit1 = ((GetBuddyByte(11, byteIndex) >> (index % 8)) & 1) != 0;
                    bool bit2;
                    if (index % 2 == 0)
                    {
                        if (index % 8 == 7)
                        {
                            bit2 = ((GetBuddyByte(11, byteIndex + 1) >> (index % 8)) & 1) != 0;
                        }
                        else
                        {
                            bit2 = ((GetBuddyByte(11, byteIndex) >> (index % 8)) & 1) != 0;
                        }
                    }
                    else
                    {
                        if (index % 8 == 0)
                        {
                            bit2 = ((GetBuddyByte(11, byteIndex - 1) >> (index % 8)) & 1) != 0;
                        }
                        else
                        {
                            bit2 = ((GetBuddyByte(11, byteIndex) >> (index % 8)) & 1) != 0;
                        }
                    }

                    if (!bit1 && !bit2)
                    {
                        int upperIndex = (int)(givenOffset / UPPER_BASE_SIZE);
                        _buddy12 &= (byte)~(1 << Math.Min(7, upperIndex));
                    }
                    else
                    {
                        return;
                    }

                    buddyIndex = 12;
                    continue;
                }
                default:
                {
                    int baseSize = 1 << (25 - (10 - buddyIndex));
                    int index = (int)(givenOffset / baseSize);
                    int byteIndex = index / 8;

                    bool bit1 = ((GetBuddyByte(buddyIndex, byteIndex) >> (index % 8)) & 1) != 0;
                    bool bit2;

                    if (index % 2 == 0)
                    {
                        index++;
                        //if there was an overflow
                        if (index % 8 == 0)
                        {
                            bit2 = ((GetBuddyByte(buddyIndex, byteIndex + 1) >> (index % 8)) & 1) != 0;
                        }
                        else
                        {
                            bit2 = ((GetBuddyByte(buddyIndex, byteIndex) >> (index % 8)) & 1) != 0;
                        }
                    }
                    else
                    {
                        index--;
                        //if there was an underflow
                        if (index % 8 == 7)
                        {
                            bit2 = ((GetBuddyByte(buddyIndex, byteIndex - 1) >> (index % 8)) & 1) != 0;
                        }
                        else
                        {
                            bit2 = ((GetBuddyByte(buddyIndex, byteIndex) >> (index % 8)) & 1) != 0;
                        }
                    }

                    if (!bit1 && !bit2)
                    {
                        int upperBaseSize = baseSize << 1;
                        int upperIndex = (int)(givenOffset / upperBaseSize);
                        int upperByteIndex = upperIndex / 8;

                        SetBuddyByte(buddyIndex + 1, upperByteIndex,
                            (byte)(GetBuddyByte(buddyIndex + 1, upperByteIndex) & ~(1 << (upperByteIndex % 8))));
                    }
                    else
                    {
                        return;
                    }

                    buddyIndex += 1;
                    continue;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetBuddyByte(int buddyIndex, int byteIndex, byte value)
    {
        switch (buddyIndex)
        {
            case 0:
            {
                _buddy0[byteIndex] = value;
                break;
            }
            case 1:
            {
                _buddy1[byteIndex] = value;
                break;
            }
            case 2:
            {
                _buddy2[byteIndex] = value;
                break;
            }
            case 3:
            {
                _buddy3[byteIndex] = value;
                break;
            }
            case 4:
            {
                _buddy4[byteIndex] = value;
                break;
            }
            case 5:
            {
                _buddy5[byteIndex] = value;
                break;
            }
            case 6:
            {
                _buddy6[byteIndex] = value;
                break;
            }
            case 7:
            {
                _buddy7[byteIndex] = value;
                break;
            }
            case 8:
            {
                _buddy8[byteIndex] = value;
                break;
            }
            case 9:
            {
                _buddy9[byteIndex] = value;
                break;
            }
            case 10:
            {
                _buddy10[byteIndex] = value;
                break;
            }
            case 11:
            {
                _buddy11[byteIndex] = value;
                break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte GetBuddyByte(int buddyIndex, int byteIndex)
    {
        switch (buddyIndex)
        {
            case 0:
            {
                return _buddy0[byteIndex];
            }
            case 1:
            {
                return _buddy1[byteIndex];
            }
            case 2:
            {
                return _buddy2[byteIndex];
            }
            case 3:
            {
                return _buddy3[byteIndex];
            }
            case 4:
            {
                return _buddy4[byteIndex];
            }
            case 5:
            {
                return _buddy5[byteIndex];
            }
            case 6:
            {
                return _buddy6[byteIndex];
            }
            case 7:
            {
                return _buddy7[byteIndex];
            }
            case 8:
            {
                return _buddy8[byteIndex];
            }
            case 9:
            {
                return _buddy9[byteIndex];
            }
            case 10:
            {
                return _buddy10[byteIndex];
            }
            case 11:
            {
                return _buddy11[byteIndex];
            }
            default:
                return 0;
        }
    }
}
