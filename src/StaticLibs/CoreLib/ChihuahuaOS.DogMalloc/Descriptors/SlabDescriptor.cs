using System.Runtime.InteropServices;
using ChihuahuaOS.CoreLib;

namespace ChihuahuaOS.DogMalloc.Descriptors;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct SlabDescriptor
{
    public ulong SlabAddress { get; }
    public ushort TotalSlots { get; }
    public ushort RemainingSlots { get; internal set; }
    public ushort SlotSize { get; }
    private ushort StackPointer;

    public SlabDescriptor* Next { get; internal set; }
    public SlabDescriptor* Previous { get; internal set; }

    public SlabDescriptor(ref ulong slabAddress, ushort totalSlots, ushort slotSize)
    {
        SlabAddress = slabAddress;
        TotalSlots = totalSlots;
        RemainingSlots = totalSlots;
        SlotSize = slotSize;

        ulong bytesPerStackSlot = totalSlots > 256 ? 2UL : 1UL;
        slabAddress += (ulong)slotSize * totalSlots + totalSlots * bytesPerStackSlot;
    }

    public ulong Allocate()
    {
        if (RemainingSlots == 0)
        {
            return 0;
        }

        ushort offset = PopFreeSlot();
        RemainingSlots--;
        return SlabAddress + (ulong)SlotSize * offset;
    }

    public void Free(ulong address)
    {
        if (address < SlabAddress)
        {
            return;
        }

        ulong byteOffset = address - SlabAddress;
        if (byteOffset > (ulong)TotalSlots * SlotSize || byteOffset % TotalSlots != 0)
        {
            return;
        }

        RemainingSlots++;
        PushFreeSlot((ushort)(byteOffset / TotalSlots));
    }


    internal void Initialize()
    {
        if (TotalSlots > 256)
        {
            ushort* ptr = (ushort*)(SlabAddress + (ulong)TotalSlots * SlotSize);
            for (uint i = 0; i < TotalSlots; i++)
            {
                ptr[i] = (ushort)(TotalSlots - 1 - i);
            }
        }
        else
        {
            byte* ptr = (byte*)(SlabAddress + (ulong)TotalSlots * SlotSize);

            for (uint i = 0; i < TotalSlots; i++)
            {
                ptr[i] = (byte)(TotalSlots - 1 - i);
            }
        }

        StackPointer = (ushort)(TotalSlots - 1);
    }

    private void PushFreeSlot(ushort offset)
    {
        if (TotalSlots > 256)
        {
            ushort* ptr = (ushort*)(SlabAddress + (ulong)TotalSlots * SlotSize);
            ptr[StackPointer + 1] = offset;
        }
        else
        {
            byte* ptr = (byte*)(SlabAddress + (ulong)TotalSlots * SlotSize);
            ptr[StackPointer + 1] = (byte)offset;
        }

        if (StackPointer >= TotalSlots - 1)
        {
            //should be unreachable
            return;
        }

        StackPointer++;
    }

    private ushort PopFreeSlot()
    {
        if (StackPointer == 0)
        {
            //should be unreachable
            CoreLibManager.Panic((byte*)"Malloc: Popped slab slot when SP was 0!"u8);
        }

        ushort offset;
        if (TotalSlots > 256)
        {
            ushort* ptr = (ushort*)(SlabAddress + (ulong)TotalSlots * SlotSize);
            offset = ptr[StackPointer];
        }
        else
        {
            byte* ptr = (byte*)(SlabAddress + (ulong)TotalSlots * SlotSize);
            offset = ptr[StackPointer];
        }

        StackPointer--;
        return offset;
    }
}
