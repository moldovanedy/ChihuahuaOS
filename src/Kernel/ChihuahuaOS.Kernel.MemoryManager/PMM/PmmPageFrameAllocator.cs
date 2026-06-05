using System.Runtime.InteropServices;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.EfiApi;

namespace ChihuahuaOS.Kernel.MemoryManager.PMM;

public static unsafe class PmmPageFrameAllocator
{
    private const int MAX_ALLOCATED_CHUNKS = 512;

    private static ulong _pageFrameIndex;

    private static ulong _freeKMemoryPhysAddressStart;

    /// <summary>
    /// Can allocate memory for the page tables before the actual PMM initialization is done.
    /// </summary>
    /// <returns></returns>
    [UnmanagedCallersOnly]
    public static ulong AllocPageFramesRaw()
    {
        if (_freeKMemoryPhysAddressStart == 0)
        {
            CoreLibManager.Panic((byte*)"PMM: There was no free kernel memory set up by the bootloader\0"u8);
        }

        if (_pageFrameIndex + 1 >= MAX_ALLOCATED_CHUNKS)
        {
            CoreLibManager.Panic(
                (byte*)"PMM: All the free kernel memory was used before the actual PMM initialization\0"u8);
        }

        ulong physAddress = _freeKMemoryPhysAddressStart + _pageFrameIndex * EfiConstants.EFI_PAGE_SIZE;
        _pageFrameIndex++;
        return physAddress;
    }

    public static void SetFreeKernelMemoryStart(ulong physAddress)
    {
        _freeKMemoryPhysAddressStart = physAddress;
    }
}
