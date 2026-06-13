using System.Runtime.InteropServices;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.MemPaginator;

namespace ChihuahuaOS.Kernel.MemoryManager.PMM;

public static unsafe class PageFrameAllocator
{
    //only one of these is used at a given time, since there can only be one function running at one time
    private static ulong _freeKMemoryPhysAddressStart;
    private static ulong _inProgressChunk;

    private static ulong _pageFrameIndex;

    /// <summary>
    /// Can allocate memory for the page tables before the actual PMM initialization is done.
    /// </summary>
    /// <returns></returns>
    [UnmanagedCallersOnly]
    public static ulong AllocPageFramesRaw()
    {
        const int MAX_ALLOCATED_CHUNKS = 512;
        if (_freeKMemoryPhysAddressStart == 0)
        {
            CoreLibManager.Panic(
                (byte*)"Paging frame alloc: There was no free kernel memory set up by the bootloader\0"u8);
        }

        if (_pageFrameIndex + 1 >= MAX_ALLOCATED_CHUNKS)
        {
            CoreLibManager.Panic(
                (byte*)
                "Paging frame alloc: All the free kernel memory was used before the actual PMM initialization\0"u8);
        }

        ulong physAddress = _freeKMemoryPhysAddressStart + _pageFrameIndex * EfiConstants.EFI_PAGE_SIZE;
        _pageFrameIndex++;
        return physAddress;
    }

    [UnmanagedCallersOnly]
    public static ulong AllocPageFramesFromPmm()
    {
        //reset and allocate a new frame
        if (_pageFrameIndex == 8)
        {
            _inProgressChunk = 0;
        }

        if (_inProgressChunk == 0)
        {
            long physAddress = MainMemManager.Pmm.Allocate(ChunkLevel1.MIN_CHUNK_SIZE);
            if (physAddress <= 0)
            {
                CoreLibManager.Panic((byte*)"Paging frame alloc: Could not allocate chunk from PMM\0"u8);
            }

            _inProgressChunk = (ulong)physAddress;
            _pageFrameIndex = 0;
        }

        ulong address = _inProgressChunk + _pageFrameIndex * EfiConstants.EFI_PAGE_SIZE;
        PageError error = MainMemManager.KPagingManager.IdentityMapPage(
            address,
            PageFlags.Present | PageFlags.ReadPermission | PageFlags.WritePermission);
        if (error != PageError.Success)
        {
            return 0;
        }

        _pageFrameIndex++;
        return address;
    }

    [UnmanagedCallersOnly]
    public static ulong AllocPageFramesFromVmm()
    {
        //reset and allocate a new frame
        if (_pageFrameIndex == 8)
        {
            _inProgressChunk = 0;
        }

        if (_inProgressChunk == 0)
        {
            long physAddress = MainMemManager.Pmm.Allocate(ChunkLevel1.MIN_CHUNK_SIZE);
            if (physAddress <= 0)
            {
                CoreLibManager.Panic((byte*)"Paging frame alloc: Could not allocate chunk from VMM\0"u8);
            }

            _inProgressChunk = (ulong)physAddress;
            _pageFrameIndex = 0;
        }

        ulong address = _inProgressChunk + _pageFrameIndex * EfiConstants.EFI_PAGE_SIZE;
        PageError error = MainMemManager.KPagingManager.IdentityMapPage(
            address,
            PageFlags.Present | PageFlags.ReadPermission | PageFlags.WritePermission);
        if (error != PageError.Success)
        {
            return 0;
        }

        MainMemManager.Vmm.VirtuallyMapKernelArea(
            address,
            address,
            EfiConstants.EFI_PAGE_SIZE,
            false);

        _pageFrameIndex++;
        return address;
    }

    [UnmanagedCallersOnly]
    public static ulong AllocPageFramesFromKernelHeap()
    {
        //TODO
        return 0;
    }

    public static void SetFreeKernelMemoryStart(ulong physAddress)
    {
        _freeKMemoryPhysAddressStart = physAddress;
    }

    /// <summary>
    /// Will reset the internal state. It is necessary when changing the allocation function.
    /// </summary>
    public static void Reset()
    {
        _inProgressChunk = 0;
        _freeKMemoryPhysAddressStart = 0;

        _pageFrameIndex = 0;
    }
}
