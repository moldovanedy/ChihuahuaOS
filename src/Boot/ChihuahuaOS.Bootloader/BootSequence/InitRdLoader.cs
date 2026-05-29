using System;
using System.IO;
using ChihuahuaOS.BootParams;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.BootServices;
using ChihuahuaOS.MemPaginator;

namespace ChihuahuaOS.Bootloader.BootSequence;

public static unsafe class InitRdLoader
{
    public static bool Load(EfiBootServices* bs, PagingManager pagingManager, OsVersion osVersion, out ulong fileSize)
    {
        fileSize = 0;
        using string osVersionStr = osVersion.ToString();
        using string rdFilePath = "\\EFI\\BOOT\\init." + osVersionStr + ".rd";
        using FileStream? fs = File.OpenRead(rdFilePath);
        if (fs == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            using string errString = ((int)File.LastOpenError).ToString();
            Console.WriteLine(
                "FATAL ERROR: Could not read init-ramdisk file! Error code (EFI): " + errString);
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }

        ulong physicalAddress = 0;
        ulong requiredPages = ((ulong)fs.Length + (EfiConstants.EFI_PAGE_SIZE - 1)) / EfiConstants.EFI_PAGE_SIZE;
        fileSize = (ulong)fs.Length;

        bool success = AllocatePhysicalMemory(bs, &physicalAddress, requiredPages);
        if (!success)
        {
            return false;
        }

        for (ulong i = 0; i < requiredPages; i++)
        {
            ulong offsetPhysicalAddress = physicalAddress + i * EfiConstants.EFI_PAGE_SIZE;
            PageError pageError = pagingManager.MapPage(
                offsetPhysicalAddress,
                KVirtualAddresses.INITRD_BASE + i * EfiConstants.EFI_PAGE_SIZE,
                PageFlags.Present | PageFlags.ReadPermission);

            if (pageError != PageError.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                using string errString = ((int)pageError).ToString();
                Console.WriteLine(
                    "FATAL ERROR: Could not map memory for init-ramdisk! Error code (EFI): " + errString);
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }

            int bytesRead = fs.ReadRaw((byte*)offsetPhysicalAddress, 0, EfiConstants.EFI_PAGE_SIZE);
            if (bytesRead == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                using string errString = ((int)File.LastOpenError).ToString();
                Console.WriteLine(
                    "FATAL ERROR: Could not read from init-ramdisk file! Error code (EFI): " + errString);
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
        }

        return true;
    }

    private static bool AllocatePhysicalMemory(EfiBootServices* bs, ulong* physicalAddress, ulong requiredPages)
    {
        EfiStatus status = bs->AllocatePages(
            EfiAllocateType.AllocateAnyPages,
            EfiMemoryType.ChihuahuaInitRdMemory,
            requiredPages,
            physicalAddress);

        if (status != EfiStatus.Success || physicalAddress == null || *physicalAddress == 0)
        {
            //TODO: since initrd might be large, also try allocating memory in chunks (physical memory doesn't
            // have to be continuous)

            Console.ForegroundColor = ConsoleColor.Red;
            using string errString = ((int)status).ToString();
            Console.WriteLine(
                "FATAL ERROR: Could not allocate memory for init-ramdisk! Error code (EFI): " + errString);
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }

        return true;
    }
}
