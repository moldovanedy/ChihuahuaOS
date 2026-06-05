using System;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.CoreLib.Extra;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.BootServices;
using ChihuahuaOS.MemPaginator;

namespace ChihuahuaOS.Kernel.MemoryManager.PMM;

public unsafe struct PhysicalMemManager
{
    private ChunkLevel2* _rootChunkPtr;

    public PhysicalMemManager(PagingManager pagingManager, EfiMapWrapper efiMap)
    {
        InitDescriptors(pagingManager, efiMap);
    }

    public long Allocate(long blockSize)
    {
        return _rootChunkPtr->Allocate(blockSize);
    }

    public void Deallocate(long blockSize, long blockOffset)
    {
        _rootChunkPtr->Deallocate(blockSize, blockOffset);
    }

    public void InitializeFromEfiMap(EfiMapWrapper efiMap)
    {
        //definitely set the first chunk as allocated, as it contains the physical address 0, which we treat as invalid
        // and will cause all sorts of issues later, so it's better to waste max. 32 KiB than to handle separate cases
        // for address 0
        _rootChunkPtr->SetInitiallyAllocatedBits(ChunkLevel1.MIN_CHUNK_SIZE, 0);

        for (int i = 0; i < efiMap.ArrayLength; i++)
        {
            if (efiMap[i].Type.IsAvailable())
            {
                continue;
            }

            //the number of pages refers to 4 KiB pages, but we need 32 KiB chinks, so we divide by 8,
            // but with an extra chunk to accomodate for the number of pages indivisible by the chunk's min. size 
            long blockSize = (long)((efiMap[i].NumberOfPages + 7) / 8 * ChunkLevel1.MIN_CHUNK_SIZE);

            //align the offset on 32 KiB chunks
            long blockOffset = (long)efiMap[i].PhysicalStart / ChunkLevel1.MIN_CHUNK_SIZE * ChunkLevel1.MIN_CHUNK_SIZE;
            _rootChunkPtr->SetInitiallyAllocatedBits(blockSize, blockOffset);
        }
    }


    private void InitDescriptors(PagingManager pagingManager, EfiMapWrapper efiMap)
    {
        _rootChunkPtr = WriteDescriptors(pagingManager, efiMap);
    }

    private static ChunkLevel2* WriteDescriptors(PagingManager pagingManager, EfiMapWrapper efiMap)
    {
        ulong chunkLevel2RequiredPages =
            (ulong)(sizeof(ChunkLevel2) + (EfiConstants.EFI_PAGE_SIZE - 1)) / EfiConstants.EFI_PAGE_SIZE;
        ulong chunkLevel1RequiredPages =
            (ulong)(sizeof(ChunkLevel1) + (EfiConstants.EFI_PAGE_SIZE - 1)) / EfiConstants.EFI_PAGE_SIZE;

        ChunkLevel2* chunkLevel2Ptr = null;

        ulong highestPhysicalMemory =
            efiMap[efiMap.ArrayLength - 1].PhysicalStart +
            EfiConstants.EFI_PAGE_SIZE * efiMap[efiMap.ArrayLength - 1].NumberOfPages;
        //each level 1 chunk maps 1 GiB (1 << 30 bytes) of physical memory
        ulong numLevel1ChunksRequired =
            Math.Min(ChunkLevel2.NUM_DESCRIPTORS, (highestPhysicalMemory + ((1 << 30) - 1)) / (1 << 30));
        ulong numLevel1ChunksMapped = 0;

        //first, find a suitable place for the memory chunks
        for (int i = 0; i < efiMap.ArrayLength; i++)
        {
            EfiMemoryDescriptor descriptor = efiMap[i];
            if (!descriptor.Type.IsAvailable() || descriptor.PhysicalStart == 0)
            {
                continue;
            }

            ulong physicalAddress = descriptor.PhysicalStart;
            ulong numFreePages = descriptor.NumberOfPages;
            if (chunkLevel2Ptr == null && numFreePages >= chunkLevel2RequiredPages)
            {
                //firstly, map the region
                bool success = true;
                for (ulong j = 0; j < chunkLevel2RequiredPages; j++)
                {
                    PageError pgError = pagingManager.IdentityMapPage(
                        physicalAddress + j * EfiConstants.EFI_PAGE_SIZE,
                        PageFlags.Present | PageFlags.ReadPermission | PageFlags.WritePermission);
                    if (pgError != PageError.Success)
                    {
                        success = false;
                    }
                }

                if (!success)
                {
                    continue;
                }

                PageError error = pagingManager.SubmitChanges();
                if (error != PageError.Success)
                {
                    CoreLibManager.Panic((byte*)"PMM: Could not submit paging changes; state corrupted\0"u8);
                    return null;
                }

                RawMemory.MemSet(
                    (void*)physicalAddress, 0, chunkLevel2RequiredPages * EfiConstants.EFI_PAGE_SIZE);

                //then write the descriptor
                chunkLevel2Ptr = (ChunkLevel2*)physicalAddress;
                *chunkLevel2Ptr = new ChunkLevel2();
                numFreePages -= chunkLevel2RequiredPages;
                physicalAddress += chunkLevel2RequiredPages * EfiConstants.EFI_PAGE_SIZE;
            }

            if (chunkLevel2Ptr == null)
            {
                continue;
            }

            //we try to write all the level 1 chunks in the same region; if we can't, we simply go to the next region
            // and try there as well
            while (numFreePages >= chunkLevel1RequiredPages && numLevel1ChunksMapped < numLevel1ChunksRequired)
            {
                //the same as before (map the region, then write into it)
                bool success = true;
                for (ulong j = 0; j < chunkLevel1RequiredPages; j++)
                {
                    PageError pgError = pagingManager.IdentityMapPage(
                        physicalAddress + j * EfiConstants.EFI_PAGE_SIZE,
                        PageFlags.Present | PageFlags.ReadPermission | PageFlags.WritePermission);
                    if (pgError != PageError.Success)
                    {
                        success = false;
                    }
                }

                if (!success)
                {
                    continue;
                }

                PageError error = pagingManager.SubmitChanges();
                if (error != PageError.Success)
                {
                    CoreLibManager.Panic((byte*)"PMM: Could not submit paging changes; state corrupted\0"u8);
                    return null;
                }

                RawMemory.MemSet(
                    (void*)physicalAddress, 0, chunkLevel1RequiredPages * EfiConstants.EFI_PAGE_SIZE);

                //TODO: after several level 1 chunks, update the page frame allocator to use regions directly from the
                // general memory, so we don't exceed the kernels's 2 MiB free space (this is only for devices with
                // lots of RAM, it won't be a problem for RAM < 128 GiB)
                ChunkLevel1* chunkLevel1Ptr = (ChunkLevel1*)physicalAddress;
                *chunkLevel1Ptr = new ChunkLevel1();

                (*chunkLevel2Ptr)[(int)numLevel1ChunksMapped] = new ChunkLevel2.Level1Descriptor(
                    physicalAddress,
                    ChunkLevel2.Level1Descriptor.Flags.None);

                //update variables
                numFreePages -= chunkLevel1RequiredPages;
                numLevel1ChunksMapped++;
                physicalAddress += chunkLevel1RequiredPages * EfiConstants.EFI_PAGE_SIZE;
            }
        }

        if (chunkLevel2Ptr == null)
        {
            CoreLibManager.Panic((byte*)"PMM: Could not find free space for root chunk\0"u8);
            return null;
        }

        if (numLevel1ChunksMapped < numLevel1ChunksRequired)
        {
            CoreLibManager.Panic((byte*)"PMM: Could not find free space for all the chunk descriptors\0"u8);
            return null;
        }

        return chunkLevel2Ptr;
    }
}
