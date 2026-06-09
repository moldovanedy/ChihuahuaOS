using System;
using ChihuahuaOS.BootParams;
using ChihuahuaOS.BootParams.ParamsData;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.CoreLib.Extra;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.BootServices;
using ChihuahuaOS.Kernel.MemoryManager.InternalAvlTree;
using ChihuahuaOS.Kernel.MemoryManager.PMM;
using ChihuahuaOS.MemPaginator;

namespace ChihuahuaOS.Kernel.MemoryManager.VMM;

public struct VirtualMemManager
{
    private AvlTree _kernelTree;

    public VirtualMemManager()
    {
        bool success = SetupKernelAvlTree();
        if (!success)
        {
            unsafe
            {
                CoreLibManager.Panic((byte*)"VMM: Could not set up the kernel's AVL tree!\0"u8);
            }
        }
    }

    public unsafe void InitializeFromCurrentState(EfiMapWrapper efiMap, KParams* kParams)
    {
        _kernelTree.StackBottomPointer = kParams->VirtualSpaceInfo.KStackBottom;

        //handle the identity-mapped UEFI memory
        for (int i = 0; i < efiMap.ArrayLength; i++)
        {
            //for the kernel memory itself and the init-ramdisk memory, we will handle them further down
            if (efiMap[i].Type.IsAvailable()
                || efiMap[i].Type == EfiMemoryType.ChihuahuaKernelMemory
                || efiMap[i].Type == EfiMemoryType.ChihuahuaInitRdMemory)
            {
                continue;
            }

            bool isFreeable =
                efiMap[i].Type == EfiMemoryType.EfiAcpiReclaimMemory
                || efiMap[i].Type == EfiMemoryType.ChihuahuaEfiMemMap
                || efiMap[i].Type == EfiMemoryType.ChihuahuaPageTables
                || efiMap[i].Type == EfiMemoryType.ChihuahuaFreeKernelMemory;

            //identity map, since the EFI map generally doesn't have the virtual address set
            ulong totalSize = efiMap[i].NumberOfPages * EfiConstants.EFI_PAGE_SIZE;
            MapArea(efiMap[i].PhysicalStart, efiMap[i].PhysicalStart, totalSize, ref _kernelTree, isFreeable);
        }

        //map the kernel executable memory
        for (int i = 0; i < kParams->KernelExecInfo.NumSegmentsLoaded; i++)
        {
            KernelExecutableInfo.SegmentDescriptor descriptor = kParams->KernelExecInfo.SegmentsDescriptorsArray[i];
            ulong usedSizeNormalized =
                (descriptor.Size + (EfiConstants.EFI_PAGE_SIZE - 1)) / EfiConstants.EFI_PAGE_SIZE *
                EfiConstants.EFI_PAGE_SIZE;

            MapArea(
                descriptor.VirtualStart,
                descriptor.PhysicalStart,
                usedSizeNormalized,
                ref _kernelTree,
                false);

            //set the heap end to the first page after the kernel executable (with a 2-page minimum offset, similar
            // to the other sections)
            _kernelTree.HeapEndPointer =
                Math.Max(_kernelTree.HeapEndPointer, descriptor.VirtualStart + usedSizeNormalized);
            _kernelTree.HeapEndPointer += 2 * EfiConstants.EFI_PAGE_SIZE;
        }

        //then, add a random offset for the kernel heap start
        _kernelTree.HeapEndPointer += Random.NextMersenne(0, 1 << 13) * EfiConstants.EFI_PAGE_SIZE;

        //map the kernel-used memory
        MapArea(
            kParams->VirtualSpaceInfo.KStackBottom,
            0,
            kParams->VirtualSpaceInfo.KStackTop - kParams->VirtualSpaceInfo.KStackBottom,
            ref _kernelTree,
            false);
        MapArea(
            kParams->VirtualSpaceInfo.InitRdBase,
            0,
            kParams->VirtualSpaceInfo.InitRdLimit - kParams->VirtualSpaceInfo.InitRdBase,
            ref _kernelTree,
            false);
        MapArea(
            kParams->VirtualSpaceInfo.GopBase,
            0,
            kParams->VirtualSpaceInfo.GopLimit - kParams->VirtualSpaceInfo.GopBase,
            ref _kernelTree,
            false);

        //map the PMM chunks
        MapArea(
            (ulong)MainMemManager.Pmm.RootChunkPtr,
            (ulong)MainMemManager.Pmm.RootChunkPtr,
            (ulong)sizeof(ChunkLevel2),
            ref _kernelTree,
            false);
        for (int i = 0; i < ChunkLevel2.NUM_DESCRIPTORS; i++)
        {
            ChunkLevel2.Level1Descriptor descriptor = (*MainMemManager.Pmm.RootChunkPtr)[i];
            //this will be the first null entry, so break here
            if (descriptor.Entry == 0)
            {
                break;
            }

            MapArea(
                descriptor.Entry,
                descriptor.Entry,
                (ulong)sizeof(ChunkLevel1),
                ref _kernelTree,
                false);
        }
    }

    public ulong AllocateKernelVirtualMem(ulong size)
    {
        return AllocateVirtualMem(size, ref _kernelTree, _kernelTree.HeapEndPointer);
    }

    /// <summary>
    /// Allocates virtual memory for the process and returns the virtual address start.
    /// </summary>
    /// <param name="size">
    /// The size (in bytes) that you want to allocate. Must be of 4 KiB granularity (i.e., divisible by 4096).
    /// </param>
    /// <param name="avlTree">The avl tree.</param>
    /// <param name="minVirtualAddress">
    /// The virtual address hint that the kernel tries to use, but it's not obligated; it can be higher in virtual
    /// address, but never lower.
    /// </param>
    /// <returns>The chunk's virtual address start or 0 if the allocation failed</returns>
    public static ulong AllocateVirtualMem(ulong size, ref AvlTree avlTree, ulong minVirtualAddress)
    {
        long physicalAddress = MainMemManager.Pmm.Allocate((long)size);
        if (physicalAddress <= 0)
        {
            return 0;
        }

        bool success;
        if (avlTree.IsAddressFree(minVirtualAddress))
        {
            success = MapArea(minVirtualAddress, (ulong)physicalAddress, size, ref avlTree);
            if (success)
            {
                return minVirtualAddress;
            }
        }

        const ulong MAX_SEARCHED_OFFSET = 4 * 1024 * EfiConstants.EFI_PAGE_SIZE;
        minVirtualAddress = avlTree.TryGetFreeAddress(
            minVirtualAddress,
            minVirtualAddress + MAX_SEARCHED_OFFSET,
            size);
        if (minVirtualAddress == 0)
        {
            MainMemManager.Pmm.Deallocate((long)size, physicalAddress);
            return 0;
        }

        success = MapArea(minVirtualAddress, (ulong)physicalAddress, size, ref avlTree);
        if (success)
        {
            return minVirtualAddress;
        }

        MainMemManager.Pmm.Deallocate((long)size, physicalAddress);
        return 0;
    }


    private bool SetupKernelAvlTree()
    {
        long avlPhysAddress = MainMemManager.Pmm.Allocate(ChunkLevel1.MIN_CHUNK_SIZE);
        if (avlPhysAddress <= 0)
        {
            return false;
        }

        PageError error = MainMemManager.KPagingManager.IdentityMapRegion(
            (ulong)avlPhysAddress,
            PageFlags.Present | PageFlags.ReadPermission | PageFlags.WritePermission,
            ChunkLevel1.MIN_CHUNK_SIZE / EfiConstants.EFI_PAGE_SIZE,
            out _);
        if (error != PageError.Success)
        {
            return false;
        }

        unsafe
        {
            RawMemory.MemSet((void*)avlPhysAddress, 0, ChunkLevel1.MIN_CHUNK_SIZE);
            _kernelTree = new AvlTree((AvlTreeNode*)avlPhysAddress);
        }

        return true;
    }

    private static bool MapArea(
        ulong virtualStart,
        ulong physicalStart,
        ulong totalSize,
        ref AvlTree avlTree,
        bool isFreeable = true)
    {
        ulong mappedSize = 0;
        while (totalSize > 0)
        {
            ulong size = Math.Min(uint.MaxValue, totalSize);
            bool success = avlTree.TryInsert(
                virtualStart + mappedSize,
                physicalStart + mappedSize,
                (uint)size,
                isFreeable);
            if (!success)
            {
                return false;
            }

            totalSize -= size;
            mappedSize += size;
        }

        return true;
    }
}
