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
    private MemAvlTree _kernelTree;

    //2 MiB and 64 MiB, respectively
    private const ulong MEM_SIZE_MEDIUM = 2 * 1024 * 1024;
    private const ulong MEM_SIZE_LARGE = 32 * MEM_SIZE_MEDIUM;

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
            VirtuallyMapArea(efiMap[i].PhysicalStart, efiMap[i].PhysicalStart, totalSize, ref _kernelTree, isFreeable);
        }

        //map the kernel executable memory
        for (int i = 0; i < kParams->KernelExecInfo.NumSegmentsLoaded; i++)
        {
            KernelExecutableInfo.SegmentDescriptor descriptor = kParams->KernelExecInfo.SegmentsDescriptorsArray[i];
            ulong usedSizeNormalized =
                (descriptor.Size + (EfiConstants.EFI_PAGE_SIZE - 1)) / EfiConstants.EFI_PAGE_SIZE *
                EfiConstants.EFI_PAGE_SIZE;

            VirtuallyMapArea(
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
        VirtuallyMapArea(
            kParams->VirtualSpaceInfo.KStackBottom,
            0,
            kParams->VirtualSpaceInfo.KStackTop - kParams->VirtualSpaceInfo.KStackBottom,
            ref _kernelTree,
            false);
        VirtuallyMapArea(
            kParams->VirtualSpaceInfo.InitRdBase,
            0,
            kParams->VirtualSpaceInfo.InitRdLimit - kParams->VirtualSpaceInfo.InitRdBase,
            ref _kernelTree,
            false);
        VirtuallyMapArea(
            kParams->VirtualSpaceInfo.GopBase,
            0,
            kParams->VirtualSpaceInfo.GopLimit - kParams->VirtualSpaceInfo.GopBase,
            ref _kernelTree,
            false);

        //map the PMM chunks
        VirtuallyMapArea(
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

            VirtuallyMapArea(
                descriptor.Entry,
                descriptor.Entry,
                (ulong)sizeof(ChunkLevel1),
                ref _kernelTree,
                false);
        }
    }

    public ulong ExpandKernelHeap(ulong withSize)
    {
        return AllocateVirtualMem(
            withSize,
            ref _kernelTree,
            _kernelTree.HeapEndPointer,
            PageFlags.Present | PageFlags.ReadPermission | PageFlags.WritePermission,
            false,
            VirtualSegmentType.Heap);
    }

    public ulong AllocateKernelVirtualMem(
        ulong size,
        ulong minVirtualAddress,
        PageFlags flags,
        bool isFixedVirtualAddress = false,
        VirtualSegmentType segmentType = VirtualSegmentType.Unknown)
    {
        return AllocateVirtualMem(
            size,
            ref _kernelTree,
            minVirtualAddress,
            flags,
            isFixedVirtualAddress,
            segmentType);
    }

    /// <summary>
    /// Allocates virtual memory for the process and returns the virtual address start.
    /// </summary>
    /// <param name="size">
    /// The size (in bytes) that you want to allocate. Must be of 4 KiB granularity (i.e., divisible by 4096).
    /// </param>
    /// <param name="memAvlTree">The avl tree.</param>
    /// <param name="minVirtualAddress">
    /// The virtual address hint that the kernel tries to use, but it's not obligated; it can be higher in virtual
    /// address, but never lower.
    /// </param>
    /// <param name="flags">The memory mapping flags.</param>
    /// <param name="isFixedVirtualAddress">
    /// If true, will treat minVirtualAddress as a fixed, mandatory address, so it won't go higher than it.
    /// </param>
    /// <param name="segmentType">
    /// The virtual segment type of the allocated memory. It is by default unknown.
    /// </param>
    /// <returns>The chunk's virtual address start or 0 if the allocation failed</returns>
    public static ulong AllocateVirtualMem(
        ulong size,
        ref MemAvlTree memAvlTree,
        ulong minVirtualAddress,
        PageFlags flags,
        bool isFixedVirtualAddress = false,
        VirtualSegmentType segmentType = VirtualSegmentType.Unknown)
    {
        const ulong MAX_SEARCHED_OFFSET = 4 * 1024 * EfiConstants.EFI_PAGE_SIZE;
        size = (size + (EfiConstants.EFI_PAGE_SIZE - 1)) / EfiConstants.EFI_PAGE_SIZE * EfiConstants.EFI_PAGE_SIZE;

        long physicalAddress = MainMemManager.Pmm.Allocate((long)size);
        //if a large block is requested, we split the physical allocation in smaller chunks
        if (physicalAddress <= 0)
        {
            //if the entire virtual chunk is free, proceed
            if (memAvlTree.IsChunkFree(minVirtualAddress, size))
            {
                return AllocateVirtualMemChunked(size, ref memAvlTree, minVirtualAddress, flags);
            }

            if (isFixedVirtualAddress)
            {
                return 0;
            }

            //otherwise, search for a free address (note that this only happens at start, as the chunks MUST
            // be continuous in virtual memory) and retry
            minVirtualAddress = memAvlTree.TryGetFreeAddress(
                minVirtualAddress,
                minVirtualAddress + MAX_SEARCHED_OFFSET,
                size);
            if (minVirtualAddress == 0)
            {
                return 0;
            }

            return AllocateVirtualMemChunked(size, ref memAvlTree, minVirtualAddress, flags);
        }

        if (memAvlTree.IsChunkFree(minVirtualAddress, size))
        {
            return MapArea(minVirtualAddress, physicalAddress, size, flags, ref memAvlTree, segmentType);
        }

        if (isFixedVirtualAddress)
        {
            return 0;
        }

        minVirtualAddress = memAvlTree.TryGetFreeAddress(
            minVirtualAddress,
            minVirtualAddress + MAX_SEARCHED_OFFSET,
            size);
        if (minVirtualAddress == 0)
        {
            MainMemManager.Pmm.Deallocate((long)size, physicalAddress);
            return 0;
        }

        return MapArea(minVirtualAddress, physicalAddress, size, flags, ref memAvlTree, segmentType);
    }

    /// <summary>
    /// NOTE: this is only for a temporary stage of the page frame allocator, don't use this afterwards.
    /// </summary>
    /// <returns></returns>
    internal bool VirtuallyMapKernelArea(
        ulong virtualStart,
        ulong physicalStart,
        ulong totalSize,
        bool isFreeable = true)
    {
        return VirtuallyMapArea(virtualStart, physicalStart, totalSize, ref _kernelTree, isFreeable);
    }


    private static ulong AllocateVirtualMemChunked(
        ulong size,
        ref MemAvlTree memAvlTree,
        ulong fixedVirtualAddress,
        PageFlags flags)
    {
        ulong chunkSize;
        switch (size)
        {
            case >= MEM_SIZE_LARGE:
                chunkSize = MEM_SIZE_LARGE;
                break;
            case >= MEM_SIZE_MEDIUM:
                chunkSize = MEM_SIZE_MEDIUM;
                break;
            default:
                chunkSize = size;
                break;
        }

        ulong numChunks = (size + (chunkSize - 1)) / chunkSize;
        for (ulong i = 0; i < numChunks; i++)
        {
            ulong allocSize = Math.Min(size, chunkSize);
            ulong intermediateAddress = AllocateVirtualMem(
                allocSize,
                ref memAvlTree,
                fixedVirtualAddress,
                flags,
                true);

            size -= allocSize;
            fixedVirtualAddress += allocSize;

            if (intermediateAddress == 0)
            {
                return 0;
            }
        }

        return fixedVirtualAddress;
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
            _kernelTree = new MemAvlTree((MemAvlTreeNode*)avlPhysAddress);
        }

        return true;
    }

    /// <summary>
    /// This will update the VMM structures to mark this memory used and also map the memory in the paging structures.
    /// </summary>
    /// <returns></returns>
    private static ulong MapArea(
        ulong virtualAddress,
        long physicalAddress,
        ulong size,
        PageFlags flags,
        ref MemAvlTree memAvlTree,
        VirtualSegmentType segmentType = VirtualSegmentType.Unknown)
    {
        bool success = VirtuallyMapArea(virtualAddress, (ulong)physicalAddress, size, ref memAvlTree);
        if (!success)
        {
            MainMemManager.Pmm.Deallocate((long)size, physicalAddress);
            return 0;
        }

        ulong numPages = (size + (EfiConstants.EFI_PAGE_SIZE - 1)) / EfiConstants.EFI_PAGE_SIZE;
        PageError pageError = MainMemManager.KPagingManager.MapRegion(
            (ulong)physicalAddress,
            virtualAddress,
            flags,
            numPages,
            out _);
        if (pageError != PageError.Success)
        {
            MainMemManager.Pmm.Deallocate((long)size, physicalAddress);
            return 0;
        }

        if (segmentType == VirtualSegmentType.Heap)
        {
            memAvlTree.HeapEndPointer = virtualAddress + size;
        }

        return virtualAddress;
    }

    /// <summary>
    /// Refers to updating VMM structures to indicate that this region is occupied, not actually mapping the memory
    /// in the paging structures.
    /// </summary>
    /// <returns></returns>
    private static bool VirtuallyMapArea(
        ulong virtualStart,
        ulong physicalStart,
        ulong totalSize,
        ref MemAvlTree memAvlTree,
        bool isFreeable = true)
    {
        ulong mappedSize = 0;
        while (totalSize > 0)
        {
            ulong size = Math.Min(uint.MaxValue, totalSize);
            bool success = memAvlTree.TryInsert(
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
