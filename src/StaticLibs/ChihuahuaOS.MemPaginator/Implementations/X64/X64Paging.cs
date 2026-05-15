#if ARCH_X64
using System;
using System.Runtime.CompilerServices;
using ChihuahuaOS.CoreLib.Extra;

namespace ChihuahuaOS.MemPaginator.Implementations.X64;

public readonly unsafe struct X64Paging : IPagingImplementation
{
    private const ulong INDEX_MASK = 0x1FF;
    private const ulong PHYSICAL_ADDRESS_MASK = 0x000FFFFFFFFFF000;

    private const int P4_SHIFT = 39;
    private const int P3_SHIFT = 30;
    private const int P2_SHIFT = 21;
    private const int P1_SHIFT = 12;

    private readonly PageTable* _rootPageTable;
    private readonly bool _isCurrentlyActive;
    private readonly Func<ulong> _frameAllocator;

    public X64Paging(PageTable* rootPageTable, bool isCurrentlyActive, Func<ulong> frameAllocator)
    {
        _rootPageTable = rootPageTable;
        _isCurrentlyActive = isCurrentlyActive;
        _frameAllocator = frameAllocator;
    }

#if DEBUG
    /// <summary>
    /// A small test to determine whether the mapping is correct. Will return the entry for the given virtual address,
    /// being formed from the physical address and flags.
    /// </summary>
    /// <param name="virtualAddress"></param>
    /// <returns></returns>
    public ulong DebugTestPaging(ulong virtualAddress)
    {
        ulong l4Idx = (virtualAddress >> P4_SHIFT) & INDEX_MASK;
        ulong l3Idx = (virtualAddress >> P3_SHIFT) & INDEX_MASK;
        ulong l2Idx = (virtualAddress >> P2_SHIFT) & INDEX_MASK;
        ulong l1Idx = (virtualAddress >> P1_SHIFT) & INDEX_MASK;

        ulong entryForL3 = _rootPageTable->Entries[l4Idx];
        if (entryForL3 == 0)
        {
            return 0;
        }

        PageTable* l3Table = GetPageTableFromPhysicalAddress(entryForL3 & PHYSICAL_ADDRESS_MASK, false);
        ulong entryForL2 = l3Table->Entries[l3Idx];
        if (entryForL2 == 0)
        {
            return 0;
        }

        PageTable* l2Table = GetPageTableFromPhysicalAddress(entryForL2 & PHYSICAL_ADDRESS_MASK, false);
        ulong entryForL1 = l2Table->Entries[l2Idx];
        if (entryForL1 == 0)
        {
            return 0;
        }

        PageTable* l1Table = GetPageTableFromPhysicalAddress(entryForL1 & PHYSICAL_ADDRESS_MASK, false);
        return l1Table->Entries[l1Idx];
    }
#endif

    public PageError MapPage(ulong physicalAddress, ulong virtualAddress, PageFlags flags)
    {
        const PageFlags NON_TERMINAL_PAGE_TABLE_FLAGS =
            PageFlags.Present
            | PageFlags.ReadPermission
            | PageFlags.WritePermission
            | PageFlags.ExecutePermission
            | PageFlags.UserSpaceAccessible;

        ulong l4Idx = (virtualAddress >> P4_SHIFT) & INDEX_MASK;
        ulong l3Idx = (virtualAddress >> P3_SHIFT) & INDEX_MASK;
        ulong l2Idx = (virtualAddress >> P2_SHIFT) & INDEX_MASK;
        ulong l1Idx = (virtualAddress >> P1_SHIFT) & INDEX_MASK;

        //the address must be canonical: all upper bits need to be all 0 or all 1
        ulong signExtension = virtualAddress >> 48;
        if (signExtension != 0 && signExtension != 0xFFFF)
        {
            return PageError.InvalidVirtualAddress;
        }

        ulong entryForL3 = _rootPageTable->Entries[l4Idx];
        if (entryForL3 == 0)
        {
            ulong physAddr = _frameAllocator();
            if (physAddr == 0)
            {
                return PageError.OutOfMemory;
            }

            RawMemory.MemSet(
                GetPageTableFromPhysicalAddress(physAddr, _isCurrentlyActive), 0, PagingManager.PAGE_TABLE_SIZE);
            entryForL3 = ConstructTableEntry(physAddr, NON_TERMINAL_PAGE_TABLE_FLAGS);
            _rootPageTable->Entries[l4Idx] = entryForL3;
        }

        PageTable* l3Table = GetPageTableFromPhysicalAddress(entryForL3 & PHYSICAL_ADDRESS_MASK, _isCurrentlyActive);
        if (l3Table == null)
        {
            return PageError.UnknownError;
        }

        ulong entryForL2 = l3Table->Entries[l3Idx];
        if (entryForL2 == 0)
        {
            ulong physAddr = _frameAllocator();
            if (physAddr == 0)
            {
                return PageError.OutOfMemory;
            }

            RawMemory.MemSet(
                GetPageTableFromPhysicalAddress(physAddr, _isCurrentlyActive), 0, PagingManager.PAGE_TABLE_SIZE);
            entryForL2 = ConstructTableEntry(physAddr, NON_TERMINAL_PAGE_TABLE_FLAGS);
            l3Table->Entries[l3Idx] = entryForL2;
        }

        PageTable* l2Table = GetPageTableFromPhysicalAddress(entryForL2 & PHYSICAL_ADDRESS_MASK, _isCurrentlyActive);
        if (l2Table == null)
        {
            return PageError.UnknownError;
        }

        ulong entryForL1 = l2Table->Entries[l2Idx];
        if (entryForL1 == 0)
        {
            ulong physAddr = _frameAllocator();
            if (physAddr == 0)
            {
                return PageError.OutOfMemory;
            }

            RawMemory.MemSet(
                GetPageTableFromPhysicalAddress(physAddr, _isCurrentlyActive), 0, PagingManager.PAGE_TABLE_SIZE);
            entryForL1 = ConstructTableEntry(physAddr, NON_TERMINAL_PAGE_TABLE_FLAGS);
            l2Table->Entries[l2Idx] = entryForL1;
        }

        PageTable* l1Table = GetPageTableFromPhysicalAddress(entryForL1 & PHYSICAL_ADDRESS_MASK, _isCurrentlyActive);
        if (l1Table == null)
        {
            return PageError.UnknownError;
        }

        ulong actualPhysicalAddress = l1Table->Entries[l1Idx];
        if (actualPhysicalAddress != 0)
        {
            X64PagingSubmit.InvalidatePage(virtualAddress);
        }

        l1Table->Entries[l1Idx] = ConstructTableEntry(physicalAddress, flags);
        return PageError.Success;
    }

    public void UnmapPage(ulong virtualAddress)
    {
        //TODO
    }

    public PageError SubmitChanges()
    {
        X64PagingSubmit.SubmitPageTable((ulong)_rootPageTable);
        return PageError.Success;
    }

    public ulong GetRootPageTablePhysicalAddress()
    {
        return (ulong)_rootPageTable;
    }

    public ulong VirtualToPhysical(ulong virtualAddress)
    {
        if (virtualAddress >= PagingManager.PHYSICAL_MEMORY_OFFSET)
        {
            return virtualAddress - PagingManager.PHYSICAL_MEMORY_OFFSET;
        }

        return virtualAddress;
    }

    public ulong PhysicalToVirtual(ulong physicalAddress)
    {
        if (physicalAddress >= PagingManager.PHYSICAL_MEMORY_OFFSET)
        {
            return 0;
        }

        return physicalAddress + PagingManager.PHYSICAL_MEMORY_OFFSET;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private PageTable* GetPageTableFromPhysicalAddress(ulong physicalAddress, bool isCurrentlyActive)
    {
        if (isCurrentlyActive)
        {
            return (PageTable*)PhysicalToVirtual(physicalAddress);
        }

        return (PageTable*)physicalAddress;
    }

    private static ulong ConstructTableEntry(ulong physicalAddress, PageFlags flags)
    {
        var x64PageFlags = X64PageFlags.None;

        if ((flags & PageFlags.Present) != PageFlags.None)
        {
            x64PageFlags |= X64PageFlags.Present;
        }

        if ((flags & PageFlags.UserSpaceAccessible) != PageFlags.None)
        {
            x64PageFlags |= X64PageFlags.UserSpaceAccessible;
        }

        if ((flags & PageFlags.WritePermission) != PageFlags.None)
        {
            x64PageFlags |= X64PageFlags.WriteEnable;
        }

        if ((flags & PageFlags.IsHugePage) != PageFlags.None)
        {
            x64PageFlags |= X64PageFlags.HugePage;
        }

        if ((flags & PageFlags.ExecutePermission) == PageFlags.None)
        {
            x64PageFlags |= X64PageFlags.ExecuteDisable;
        }

        ulong data = (ulong)x64PageFlags | (physicalAddress & PHYSICAL_ADDRESS_MASK);
        return data;
    }
}

#endif