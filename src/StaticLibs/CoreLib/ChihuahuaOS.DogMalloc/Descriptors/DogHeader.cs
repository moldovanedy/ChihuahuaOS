using System.Runtime.InteropServices;
using ChihuahuaOS.CoreLib.Extra;

namespace ChihuahuaOS.DogMalloc.Descriptors;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct DogHeader
{
    public ArenaLocator* ListArenaLocators { get; private set; }
    public int NumArenas { get; private set; }

    private int _padding;

    public DogHeader(ArenaLocator* initialArenaLocator)
    {
        ListArenaLocators = initialArenaLocator;

        ArenaDescriptor* descriptor = (ArenaDescriptor*)(initialArenaLocator + 1);
        ListArenaLocators->ArenaDescriptors[0] = (ulong)descriptor;

        RawMemory.MemSet(descriptor, 0, (ulong)sizeof(ArenaDescriptor));
        *descriptor = new ArenaDescriptor(ArenaDescriptor.DEFAULT_STARTING_SIZE);
        descriptor->Initialize();
        NumArenas++;
    }

    public ulong Allocate(ulong size)
    {
        ArenaDescriptor* arena = ListArenaLocators->FindFreeArena();
        if (arena == null)
        {
            //TODO: spinlock, then set up a mutex or something to wait for an arena to be free
            return 0;
        }

        arena->Flags |= ArenaDescriptorFlags.Locked;
        ulong address = arena->Allocate(size);
        arena->Flags &= ~ArenaDescriptorFlags.Locked;

        return address;
    }

    public void Free(ulong address)
    {
        ArenaDescriptor* arena = ListArenaLocators->FindArenaContainingAddress(address);
        if (arena == null)
        {
            return;
        }

        if ((arena->Flags & ArenaDescriptorFlags.Locked) != ArenaDescriptorFlags.None)
        {
            //TODO: wait
            return;
        }

        arena->Flags |= ArenaDescriptorFlags.Locked;
        arena->Free(address);
        arena->Flags &= ~ArenaDescriptorFlags.Locked;
    }


    public bool CreateArena()
    {
        int numArenas = NumArenas;
        ArenaLocator* lastArenaLocator = ListArenaLocators;
        if (lastArenaLocator == null)
        {
            return false;
        }

        while (numArenas > ArenaLocator.NUM_ARENAS_PER_LOCATOR)
        {
            numArenas -= ArenaLocator.NUM_ARENAS_PER_LOCATOR;
            lastArenaLocator = lastArenaLocator->Next;
            if (lastArenaLocator == null)
            {
                return false;
            }
        }

        if (numArenas == ArenaLocator.NUM_ARENAS_PER_LOCATOR)
        {
            lastArenaLocator = CreateArenaLocator();
            numArenas = 0;
        }

        ulong newArenaAddress = DogMallocManager.MemMapDelegate(2 * ArenaDescriptor.DEFAULT_STARTING_SIZE);
        if (newArenaAddress == 0)
        {
            return false;
        }

        RawMemory.MemSet((void*)newArenaAddress, 0, (ulong)sizeof(ArenaDescriptor));
        lastArenaLocator->ArenaDescriptors[numArenas] = newArenaAddress;

        *(ArenaDescriptor*)newArenaAddress = new ArenaDescriptor(2 * ArenaDescriptor.DEFAULT_STARTING_SIZE);
        ((ArenaDescriptor*)newArenaAddress)->Initialize();

        NumArenas++;
        return true;
    }

    private ArenaLocator* CreateArenaLocator()
    {
        return null;
    }
}
