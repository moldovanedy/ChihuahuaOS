using System.Runtime.InteropServices;
using ChihuahuaOS.CoreLib.Extra;

namespace ChihuahuaOS.DogMalloc.Descriptors;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct BucketDescriptor
{
    public const int NUM_SLABS_IN_BUCKET = 32;

    public ArenaDescriptor* ParentArena;

    public SlabDescriptor SlabListStart { get; }
    private SlabDescriptor* _slabListEnd;

    private fixed ulong _freeSlabsQueue[NUM_SLABS_IN_BUCKET];
    private short _queuePointer;
    private short _queueEnd;

    private int _padding;

    public BucketDescriptor(
        ref ulong initialSlabAddress,
        ushort numSlotsInSlab,
        ushort slotSize,
        ArenaDescriptor* parentArena)
    {
        ParentArena = parentArena;

        RawMemory.MemSet((void*)initialSlabAddress, 0, (ulong)sizeof(SlabDescriptor));
        SlabDescriptor* firstSlab = (SlabDescriptor*)initialSlabAddress;

        initialSlabAddress += (ulong)sizeof(SlabDescriptor);
        *firstSlab = new SlabDescriptor(ref initialSlabAddress, numSlotsInSlab, slotSize);
        firstSlab->Initialize();

        SlabListStart = *firstSlab;
        _slabListEnd = firstSlab;
        _freeSlabsQueue[0] = (ulong)firstSlab;
        _queuePointer = 0;
        _queueEnd = NUM_SLABS_IN_BUCKET - 1;

        //8 byte alignment
        initialSlabAddress = (initialSlabAddress + 7) / 8 * 8;
    }

    public ulong Allocate()
    {
        SlabDescriptor* potentialSlab = TryDequeueFreeSlab();
        //if not, allocate new slab
        if (potentialSlab != null)
        {
            return potentialSlab->Allocate();
        }

        SlabDescriptor lastUsedSlab;
        if ((SlabDescriptor*)_freeSlabsQueue[_queuePointer] == null)
        {
            lastUsedSlab = SlabListStart;
        }
        else
        {
            lastUsedSlab = *(SlabDescriptor*)_freeSlabsQueue[_queuePointer];
        }

        //TODO: also track some stats so that we can allocate more slots or less slots depending on the usage

        //allocate memory for the slab descriptor, but also for the slab data 
        int slabSize = lastUsedSlab.TotalSlots * lastUsedSlab.SlotSize + sizeof(SlabDescriptor);
        //8 byte alignment
        slabSize = (slabSize + 7) / 8 * 8;

        ulong newSlabAddress = ParentArena->AllocateSlab(slabSize);
        RawMemory.MemSet((void*)newSlabAddress, 0, (ulong)slabSize);

        SlabDescriptor* newSlab = (SlabDescriptor*)newSlabAddress;
        *newSlab =
            new SlabDescriptor(ref newSlabAddress, lastUsedSlab.TotalSlots, lastUsedSlab.SlotSize);

        InsertSlabInList(newSlab);
        return newSlab->Allocate();
    }

    public void FreeEmptySlabs(int maxCheckedSlabs = 0)
    {
        SlabDescriptor* current = SlabListStart.Next;
        int checkedSlabs = 0;

        while (current != null)
        {
            if (checkedSlabs >= maxCheckedSlabs)
            {
                return;
            }

            if (current->RemainingSlots == current->TotalSlots)
            {
                //TODO: mark the slot as free in the large object heap

                if (current->Previous != null)
                {
                    current->Previous->Next = current->Next;
                }

                if (current->Next != null)
                {
                    current->Next->Previous = current->Previous;
                }
            }

            current = current->Next;
            checkedSlabs++;
        }
    }


    private SlabDescriptor* TryDequeueFreeSlab()
    {
        //queue is empty
        if (_queuePointer == _queueEnd)
        {
            return null;
        }

        SlabDescriptor* potentialSlab = null;
        while (potentialSlab == null)
        {
            potentialSlab = (SlabDescriptor*)_freeSlabsQueue[_queuePointer];
            if (potentialSlab == null || potentialSlab->RemainingSlots == 0)
            {
                //no need to be here if no longer free
                _freeSlabsQueue[_queuePointer] = 0;
                _queuePointer = Mod((short)(_queuePointer - 1), NUM_SLABS_IN_BUCKET);

                //queue was full of null pointers or full slabs
                if (_queuePointer == _queueEnd)
                {
                    return null;
                }

                continue;
            }

            return potentialSlab;
        }

        return null;
    }

    private void EnqueueFreeSlab(SlabDescriptor* slab)
    {
        //if the queue is full
        short nextPointer = Mod((short)(_queuePointer + 1), NUM_SLABS_IN_BUCKET);
        if (nextPointer != _queueEnd)
        {
            _freeSlabsQueue[_queuePointer] = (ulong)slab;
            _queuePointer = Mod((short)(_queuePointer + 1), NUM_SLABS_IN_BUCKET);
            return;
        }

        _queuePointer = nextPointer;
        _queueEnd = Mod((short)(nextPointer + 1), NUM_SLABS_IN_BUCKET);
        _freeSlabsQueue[_queuePointer] = (ulong)slab;
    }

    private void InsertSlabInList(SlabDescriptor* slab)
    {
        _slabListEnd->Next = slab;
        slab->Previous = _slabListEnd;

        _slabListEnd = slab;
    }

    private static short Mod(short x, short m)
    {
        int r = x % m;
        return r < 0 ? (short)(r + m) : (short)r;
    }
}
