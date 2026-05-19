using System;
using ChihuahuaOS.CoreLib.Extra;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.BootServices;
using ChihuahuaOS.MemPaginator;

namespace ChihuahuaOS.Bootloader.EfiInteractions;

public static partial class MemMap
{
    public static unsafe EfiMap? GetMemoryMap()
    {
        bool success = GetMemoryMapDirect(
            out EfiMemoryDescriptor* rawMemMap,
            out ulong mapSize,
            out ulong _,
            out ulong mapDescriptorSize,
            out uint _);

        if (success)
        {
            return new EfiMap(rawMemMap, (int)(mapSize / mapDescriptorSize), mapDescriptorSize);
        }

        return null;
    }

    public static unsafe bool GetMemoryMapDirect(
        out EfiMemoryDescriptor* outRawMemMap,
        out ulong mapSize,
        out ulong mapKey,
        out ulong mapDescriptorSize,
        out uint mapDescriptorVersion)
    {
        outRawMemMap = null;
        mapSize = 0;
        mapKey = 0;
        mapDescriptorSize = 0;
        mapDescriptorVersion = 0;

        ulong localMapSize = 0;
        ulong localMapKey = 0;
        ulong localMapDescriptorSize = 0;
        uint localMapDescriptorVersion = 0;

        if (Environment.EfiSysTable == null)
        {
            return false;
        }

        EfiBootServices* bs = Environment.EfiSysTable->BootServices;

        //make a dummy call to get the map size
        EfiStatus status = bs->GetMemoryMap(
            &localMapSize, outRawMemMap, &localMapKey, &localMapDescriptorSize, &localMapDescriptorVersion);
        //it MUST return BufferTooSmall, since we haven't even given a buffer
        if (status != EfiStatus.BufferTooSmall)
        {
            return false;
        }

        //the memory map cam be quite tricky to get: you need to allocate memory for the map itself, which changes
        // the map and might again be too little memory left; however, repeating the call should now be ok, since the
        // map can't increase with a full page at once; that's why we retry max. 3 times
        int numRetries = 0;
        while (numRetries < 3)
        {
            ulong numPages = (localMapSize + (EfiConstants.EFI_PAGE_SIZE - 1)) / EfiConstants.EFI_PAGE_SIZE;
            ulong physAddress = 0;
            status = bs->AllocatePages(
                EfiAllocateType.AllocateAnyPages, EfiMemoryType.EfiLoaderData, numPages, &physAddress);
            if (status != EfiStatus.Success || physAddress == 0)
            {
                Console.WriteLine("ERROR: could not allocate memory in GetMemoryMap");
                return false;
            }

            RawMemory.MemSet((void*)physAddress, 0, numPages * EfiConstants.EFI_PAGE_SIZE);
            outRawMemMap = (EfiMemoryDescriptor*)physAddress;

            status = bs->GetMemoryMap(
                &localMapSize, outRawMemMap, &localMapKey, &localMapDescriptorSize, &localMapDescriptorVersion);
            if (status != EfiStatus.Success)
            {
                bs->FreePages(physAddress, numPages);
                //some invalid parameter, so just return
                if (status != EfiStatus.BufferTooSmall)
                {
                    Console.WriteLine("ASSERT: invalid parameter in GetMemoryMap");
                    return false;
                }

                numRetries++;
                continue;
            }

            mapSize = localMapSize;
            mapKey = localMapKey;
            mapDescriptorSize = localMapDescriptorSize;
            mapDescriptorVersion = localMapDescriptorVersion;
            return true;
        }

        Console.WriteLine("ERROR: number of retries for GetMemoryMap exceeded");
        return false;
    }

    /// <summary>
    /// Creates the paging structures and maps the available memory correctly. Does NOT switch to the desired
    /// paging mode, it just sets it up for future enabling. 
    /// </summary>
    /// <param name="memMap"></param>
    /// <param name="pagingManagerOpt"></param>
    /// <returns></returns>
    public static unsafe bool SetupPagingStructures(EfiMap memMap, out PagingManager? pagingManagerOpt)
    {
        pagingManagerOpt = null;
        if (Environment.EfiSysTable == null)
        {
            return false;
        }

        EfiBootServices* bs = Environment.EfiSysTable->BootServices;
        ulong physAddress = 0;

        EfiStatus status = bs->AllocatePages(
            EfiAllocateType.AllocateAnyPages, EfiMemoryType.EfiLoaderData, 1, &physAddress);
        if (status != EfiStatus.Success)
        {
            Console.WriteLine("ERROR: could not allocate memory in SetupPaging");
            return false;
        }

        RawMemory.MemSet((void*)physAddress, 0, EfiConstants.EFI_PAGE_SIZE);
        pagingManagerOpt = new PagingManager(
            (PageTable*)physAddress,
            false,
            () =>
            {
                ulong framePhysAddress = 0;
                EfiStatus stat = bs->AllocatePages(
                    EfiAllocateType.AllocateAnyPages, EfiMemoryType.EfiLoaderData, 1, &framePhysAddress);
                if (stat != EfiStatus.Success)
                {
                    Console.WriteLine("ERROR: could not allocate memory in frame allocator in SetupPaging");
                    return 0;
                }

                RawMemory.MemSet((void*)framePhysAddress, 0, EfiConstants.EFI_PAGE_SIZE);
                return framePhysAddress;
            });
        PagingManager pagingManager = pagingManagerOpt.Value;

        for (int i = 0; i < memMap.ArrayLength; i++)
        {
            EfiMemoryDescriptor entry = memMap[i];
            bool needsIdentityMapping =
                entry.Type != EfiMemoryType.EfiConventionalMemory
                && entry.Type != EfiMemoryType.EfiUnusableMemory;

            for (ulong j = 0; j < entry.NumberOfPages; j++)
            {
                const PageFlags USED_PAGE_FLAGS =
                    PageFlags.Present
                    | PageFlags.ReadPermission
                    | PageFlags.WritePermission;

                ulong physicalAddress = entry.PhysicalStart + j * EfiConstants.EFI_PAGE_SIZE;
                PageError pgError;

                //identity map (for used memory)
                if (needsIdentityMapping)
                {
                    pgError = pagingManager.IdentityMapPage(
                        physicalAddress,
                        USED_PAGE_FLAGS | PageFlags.ExecutePermission);
                    if (pgError != PageError.Success)
                    {
                        Console.WriteLine("ERROR: could not set address in SetupPaging (identity)");
                        return false;
                    }
                }

                //offset map (for all memory)
                pgError = pagingManager.MapPage(
                    physicalAddress,
                    physicalAddress + PagingManager.PHYSICAL_MEMORY_OFFSET,
                    USED_PAGE_FLAGS);
                if (pgError != PageError.Success)
                {
                    Console.WriteLine("ERROR: could not set address in SetupPaging (offset)");
                    return false;
                }
            }
        }

        //do not submit paging structures here, as we do later in the assembly trampoline code 
        return true;
    }
}