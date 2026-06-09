using System.Runtime.CompilerServices;

#if ARCH_X64
using ChihuahuaOS.MemPaginator.ASM.X64;
using ChihuahuaOS.MemPaginator.Implementations.X64;
#endif

namespace ChihuahuaOS.MemPaginator;

public readonly unsafe struct PagingManager
{
    public const ulong PAGE_TABLE_SIZE = 4096;

    //we store like that so we don't have problems with interface fields (weird issues after that)

#if ARCH_X64
    private readonly X64Paging _x64Paging;
#endif

    public PagingManager(PageTable* rootPageTable, delegate* unmanaged<ulong> frameAllocator)
    {
#if ARCH_X64
        _x64Paging = new X64Paging(rootPageTable, frameAllocator);
#endif
    }

    public PageError MapRegion(
        ulong physicalAddress,
        ulong virtualAddress,
        PageFlags flags,
        ulong numPages,
        out ulong numPagesSuccessfullyMapped,
        bool canUseHugePages = false)
    {
        numPagesSuccessfullyMapped = 0;
        for (ulong i = 0; i < numPages; i++)
        {
            PageError error = MapPage(
                physicalAddress + i * PAGE_TABLE_SIZE,
                virtualAddress + i * PAGE_TABLE_SIZE,
                flags);
            if (error != PageError.Success)
            {
                return error;
            }

            numPagesSuccessfullyMapped++;
        }

        return PageError.Success;
    }

    public PageError IdentityMapRegion(
        ulong address,
        PageFlags flags,
        ulong numPages,
        out ulong numPagesSuccessfullyMapped,
        bool canUseHugePages = false)
    {
        numPagesSuccessfullyMapped = 0;
        for (ulong i = 0; i < numPages; i++)
        {
            PageError error = IdentityMapPage(address + i * PAGE_TABLE_SIZE, flags);
            if (error != PageError.Success)
            {
                return error;
            }

            numPagesSuccessfullyMapped++;
        }

        return PageError.Success;
    }

    /// <summary>
    /// For the initial retrieving of the root page table (e.g., on X64 it will directly take it from the CR3 register).
    /// </summary>
    /// <returns></returns>
    public static ulong GetRootPageTableInitial()
    {
#if ARCH_X64
        return X64PagingSubmit.GetRootPageTable();
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PageError MapPage(ulong physicalAddress, ulong virtualAddress, PageFlags flags)
    {
#if ARCH_X64
        return _x64Paging.MapPage(physicalAddress, virtualAddress, flags);
#else
        return PageError.UnknownError;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PageError IdentityMapPage(ulong address, PageFlags flags)
    {
        return MapPage(address, address, flags);
    }

    public void UnmapPage(ulong virtualAddress)
    {
#if ARCH_X64
        _x64Paging.UnmapPage(virtualAddress);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PageError SubmitChanges()
    {
#if ARCH_X64
        return _x64Paging.SubmitChanges();
#else
        return PageError.UnknownError;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetRootPageTablePhysicalAddress()
    {
#if ARCH_X64
        return _x64Paging.GetRootPageTablePhysicalAddress();
#else
        return 0;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong VirtualToPhysical(ulong virtualAddress)
    {
#if ARCH_X64
        return _x64Paging.VirtualToPhysical(virtualAddress);
#else
        return 0;
#endif
    }
}
