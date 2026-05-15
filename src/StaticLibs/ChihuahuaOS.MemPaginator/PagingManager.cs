using System;
using System.Runtime.CompilerServices;

#if ARCH_X64
using ChihuahuaOS.MemPaginator.Implementations.X64;
#endif

namespace ChihuahuaOS.MemPaginator;

public readonly unsafe struct PagingManager
{
    public const ulong PAGE_TABLE_SIZE = 4096;
    public const ulong PHYSICAL_MEMORY_OFFSET = 16UL * 1024UL * 1024UL * 1024UL * 1024UL;

    //we store like that so we don't have problems with interface fields (weird issues after that)

#if ARCH_X64
    private readonly X64Paging _x64Paging;
#endif

    public PagingManager(PageTable* rootPageTable, bool isPagingDisabled, Func<ulong> frameAllocator)
    {
#if ARCH_X64
        _x64Paging = new X64Paging(rootPageTable, isPagingDisabled, frameAllocator);
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

#if DEBUG
    public ulong DebugTestPaging(ulong virtualAddress)
    {
#if ARCH_X64
        return _x64Paging.DebugTestPaging(virtualAddress);
#else
        return 0;
#endif
    }
#endif

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong PhysicalToVirtual(ulong physicalAddress)
    {
#if ARCH_X64
        return _x64Paging.PhysicalToVirtual(physicalAddress);
#else
        return 0;
#endif
    }
}