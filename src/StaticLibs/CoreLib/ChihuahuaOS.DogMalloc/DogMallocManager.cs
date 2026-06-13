using ChihuahuaOS.CoreLib;
using ChihuahuaOS.CoreLib.Extra;
using ChihuahuaOS.DogMalloc.Descriptors;

namespace ChihuahuaOS.DogMalloc;

public static unsafe class DogMallocManager
{
    internal static delegate* unmanaged<ulong, ulong> MemMapDelegate { get; private set; }

    internal static delegate* unmanaged<ulong, void> MemUnmapDelegate { get; private set; }

    private static DogHeader* _header;

    public static void SetMemMapDelegate(delegate* unmanaged<ulong, ulong> fn)
    {
        MemMapDelegate = fn;
    }

    public static void SetMemUnmapDelegate(delegate* unmanaged<ulong, void> fn)
    {
        MemUnmapDelegate = fn;
    }

    public static ulong MemAlloc(ulong size)
    {
        if (_header != null)
        {
            return _header->Allocate(size);
        }

        InitializeStructures();
        if (_header == null)
        {
            CoreLibManager.PrimitiveDebug(
                (byte*)"Malloc: Assertion failed: header was null even after initialization"u8);
            return 0;
        }

        return _header->Allocate(size);
    }

    public static void MemFree(ulong address)
    {
        if (_header != null)
        {
            _header->Free(address);
        }
    }


    private static void InitializeStructures()
    {
        if (MemMapDelegate == null)
        {
            CoreLibManager.PrimitiveDebug((byte*)"Malloc: MemMapDelegate was null"u8);
            return;
        }

        ulong initialAddress = MemMapDelegate(2 * ArenaDescriptor.DEFAULT_STARTING_SIZE);
        if (initialAddress == 0)
        {
            CoreLibManager.PrimitiveDebug((byte*)"Malloc: MemMapDelegate returned 0 initially"u8);
            return;
        }

        RawMemory.MemSet((void*)initialAddress, 0, (ulong)sizeof(DogHeader));
        DogHeader* header = (DogHeader*)initialAddress;
        initialAddress += (ulong)sizeof(DogHeader);

        RawMemory.MemSet((void*)initialAddress, 0, (ulong)sizeof(ArenaLocator));
        ArenaLocator* locator = (ArenaLocator*)initialAddress;

        _header = header;
        *header = new DogHeader(locator);
    }

    //TODO: for user-space, use ModuleInitializer that initializes these functions with calls to mmap/munmap
}
