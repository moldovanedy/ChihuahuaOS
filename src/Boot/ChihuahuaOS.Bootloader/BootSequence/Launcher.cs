using System;
using System.Collections.Generic;
using System.IO;
using ChihuahuaOS.Bootloader.ASM;
using ChihuahuaOS.Bootloader.EfiInteractions;
using ChihuahuaOS.BootParams;
using ChihuahuaOS.BootParams.ParamsData;
using ChihuahuaOS.CoreLib.Extra;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.BootServices;
using ChihuahuaOS.Elf;
using ChihuahuaOS.Elf.FileHeader;
using ChihuahuaOS.Elf.ProgramHeader;
using ChihuahuaOS.MemPaginator;
using ChihuahuaOS.MinimalUtils.ASM;
using ChihuahuaOS.MinimalUtils.Toml;

namespace ChihuahuaOS.Bootloader.BootSequence;

public static unsafe class Launcher
{
    internal static KernelSettings KSettings;
    internal static OsVersion BootedOsVersion;

    public static void StartBoot(OsVersion osVersion)
    {
        //re-enable the watchdog to 60 seconds
        if (Environment.EfiSysTable != null)
        {
            Environment.EfiSysTable->BootServices->SetWatchdogTimer(60, 0, 0, null);
        }

        EfiBootServices* bs = Environment.EfiSysTable->BootServices;
        if (bs == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("FATAL ERROR: Assertion failed: EFI system table is null");
            Console.ForegroundColor = ConsoleColor.White;
            Fail();
            return;
        }

        BootedOsVersion = osVersion;
        Console.Clear();
        Console.CursorLeft = 0;
        Console.CursorTop = 0;

        string bootUpMessage = "Start booting ChihuahuaOS version " + osVersion + "!";
        Console.WriteLine(bootUpMessage);
        bootUpMessage.Dispose();

        bool success = LoadKernelSettings();
        //not fatal, we can go on
        if (!success)
        {
            KSettings = new KernelSettings();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("WARN: Could not load kernel settings. Continuing with the default settings.");
            Console.ForegroundColor = ConsoleColor.White;
        }

        success = GopSetter.SetAppropriateFramebuffer();
        // unsafe
        // {
        //     EfiSystemTable* st = Environment.EfiSysTable;
        //     st->ConOut->Reset(st->ConOut, false);
        // }

        if (success)
        {
            Console.WriteLine("Set the display resolution");
        }
        else
        {
            //not fatal, we can go on
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                "WARN: Could not change the display resolution according to the settings." +
                " Continuing with the current mode");
            Console.ForegroundColor = ConsoleColor.White;
        }

        MemMap.EfiMap? efiMapOpt = MemMap.GetMemoryMap();
        if (efiMapOpt == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(
                "FATAL ERROR: Could not retrieve the system memory map!");
            Console.ForegroundColor = ConsoleColor.White;
            Fail();
            return;
        }

        MemMap.EfiMap efiMap = efiMapOpt.Value;
        Console.WriteLine("Retrieved the system memory map");

        success = MemMap.SetupPagingStructures(efiMap, out PagingManager? pagingManagerOpt);
        efiMap.Dispose();

        if (success && pagingManagerOpt != null)
        {
            Console.WriteLine("Setup paging structures");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(
                "FATAL ERROR: Could not setup paging structures!");
            Console.ForegroundColor = ConsoleColor.White;
            Fail();
            return;
        }

        PagingManager pagingManager = pagingManagerOpt.Value;

        Span<KernelExecutableInfo.SegmentDescriptor> segmentDescriptors =
            stackalloc KernelExecutableInfo.SegmentDescriptor[255];
        success = LoadKernelInMemory(
            pagingManager,
            segmentDescriptors,
            out int numSegmentDescriptors,
            out ulong kEntryPoint);
        if (success)
        {
            Console.WriteLine("Loaded kernel in memory");
        }
        else
        {
            Fail();
            return;
        }

        success = KParamsSetter.Setup(
            bs,
            pagingManager,
            segmentDescriptors,
            numSegmentDescriptors,
            out KParams* kParams);
        if (success)
        {
            Console.WriteLine("Set the kernel parameters");
        }
        else
        {
            Fail();
            return;
        }

        success = AllocateKernelStackMemory(bs, pagingManager, kParams);
        if (success)
        {
            Console.WriteLine("Allocated stack memory for the kernel");
        }
        else
        {
            Fail();
            return;
        }

        success = InitRdLoader.Load(bs, pagingManager, BootedOsVersion, kParams);
        if (success)
        {
            Console.WriteLine("Loaded init-ramdisk in memory");
        }
        else
        {
            Fail();
            return;
        }

        success = Gop.Remap(pagingManager, kParams);
        if (success)
        {
            Console.WriteLine("Remapped the framebuffer for use in OS");
        }
        else
        {
            Fail();
            return;
        }

        if (Environment.EfiSysTable == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("FATAL ERROR: Assertion failed: EFI system table is null");
            Console.ForegroundColor = ConsoleColor.White;
            Fail();
            return;
        }

        if (Environment.EfiImageHandle == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("FATAL ERROR: Assertion failed: EFI image handle is null");
            Console.ForegroundColor = ConsoleColor.White;
            Fail();
            return;
        }

        Console.WriteLine("Exiting boot services and jumping to kernel...");
        success = MemMap.GetMemoryMapDirect(
            out EfiMemoryDescriptor* memMap,
            out ulong memMapSize,
            out ulong mapKey,
            out ulong memMapEntrySize,
            out uint _);
        if (!success)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("FATAL ERROR: Could not get the final EFI memory map!");
            Console.ForegroundColor = ConsoleColor.White;
            Fail();
            return;
        }

        //NOTE: it's very important to not execute anything between the final memory map retrieval
        // and ExitBootServices, as it might invalidate the map and fail to exit boot services

        //NOTE: this is the point of no return: once we call ExitBootServices, no matter the outcome, we can't
        // return from this function, as the firmware might have already shutdown most of its services

        EfiStatus status = Environment.EfiSysTable->BootServices->ExitBootServices(
            Environment.EfiImageHandle, mapKey);
        if (status != EfiStatus.Success)
        {
            SpinLocks.HaltingInfiniteLoop();
        }

        kParams->EfiMemMapStart = memMap;
        kParams->EfiMemMapNumEntries = memMapSize / memMapEntrySize;
        kParams->EfiMemMapEntrySize = memMapEntrySize;

        ulong numPagesEfiMap = (memMapSize + (EfiConstants.EFI_PAGE_SIZE - 1)) / EfiConstants.EFI_PAGE_SIZE;
        //map as read-write so the kernel can sort the map
        PageError pgError = pagingManager.IdentityMapRegion(
            (ulong)memMap,
            PageFlags.Present | PageFlags.ReadPermission | PageFlags.WritePermission,
            numPagesEfiMap,
            out _);
        if (pgError != PageError.Success)
        {
            SpinLocks.HaltingInfiniteLoop();
        }

        ulong rootPageTable = pagingManager.GetRootPageTablePhysicalAddress();
        SetupAndJumpToKernel.Call(rootPageTable, kEntryPoint, (ulong)kParams, kParams->VirtualSpaceInfo.KStackTop);

        //NOTE: this is unreachable
        SpinLocks.HaltingInfiniteLoop();
    }

    private static void Fail()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Boot failed! Press any key to return to the main menu...");
        Console.ForegroundColor = ConsoleColor.White;
        _ = Console.ReadKey();
    }

    private static bool LoadKernelSettings()
    {
        using string osVersion = BootedOsVersion.ToString();
        using string settingsFilePath = "\\EFI\\BOOT\\ChiOS_" + osVersion + ".CFG";
        using FileStream? fs = File.OpenRead(settingsFilePath);
        if (fs == null)
        {
            return false;
        }

        List<TomlSetting> settings = TomlManager.ReadFromStream(fs, KernelSettings.NUM_SETTINGS);
        KSettings = KernelSettings.FromConfigList(settings);
        settings.Dispose();
        return true;
    }

    private static bool LoadKernelInMemory(
        PagingManager pgManager,
        Span<KernelExecutableInfo.SegmentDescriptor> segmentDescriptors,
        out int numSegmentDescriptors,
        out ulong kernelEntryPoint)
    {
        kernelEntryPoint = 0;
        numSegmentDescriptors = 0;

        using string osVersion = BootedOsVersion.ToString();
        using string kernelFilePath = "\\EFI\\BOOT\\ChihuahuaOS.Kernel." + osVersion + ".elf";
        using FileStream? fs = File.OpenRead(kernelFilePath);
        if (fs == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            using string errString = ((int)File.LastOpenError).ToString();
            Console.WriteLine(
                "FATAL ERROR: Could not read kernel executable file! Error code (EFI): " + errString);
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }

        using ElfLoader elfLoader = new(fs);
        ElfLoader.SegmentMemoryAllocator allocator = (ulong size, ulong address, ElfSegmentFlags flags) =>
        {
            if (Environment.EfiSysTable == null)
            {
                return 0;
            }

            EfiBootServices* bs = Environment.EfiSysTable->BootServices;
            ulong physicalAddress = 0;
            ulong pageCount = (size + (EfiConstants.EFI_PAGE_SIZE - 1)) / EfiConstants.EFI_PAGE_SIZE;

            EfiStatus status = bs->AllocatePages(
                EfiAllocateType.AllocateAnyPages,
                EfiMemoryType.ChihuahuaKernelMemory,
                pageCount,
                &physicalAddress);
            if (status != EfiStatus.Success)
            {
                return 0;
            }

            RawMemory.MemSet((void*)physicalAddress, 0, pageCount * EfiConstants.EFI_PAGE_SIZE);

            var pageFlags = PageFlags.Present;
            if ((flags & ElfSegmentFlags.Readable) != ElfSegmentFlags.None)
            {
                pageFlags |= PageFlags.ReadPermission;
            }

            if ((flags & ElfSegmentFlags.Writable) != ElfSegmentFlags.None)
            {
                pageFlags |= PageFlags.WritePermission;
            }

            if ((flags & ElfSegmentFlags.Executable) != ElfSegmentFlags.None)
            {
                pageFlags |= PageFlags.ExecutePermission;
            }

            PageError pageError = pgManager.MapRegion(
                physicalAddress,
                address,
                pageFlags,
                pageCount,
                out _);

            if (pageError != PageError.Success)
            {
                return 0;
            }

            return physicalAddress;
        };

        ElfHeader? elfHeaderOpt = elfLoader.GetElfHeader(out ElfError error);
        if (error != ElfError.Success || elfHeaderOpt == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            using string errString = ((int)error).ToString();
            Console.WriteLine(
                "FATAL ERROR: Could not read kernel executable ELF header! Error code: " + errString);
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }

        kernelEntryPoint = elfHeaderOpt.Value.EntryPoint;

        ElfProgramHeader[]? progHeaders = null;
        try
        {
            progHeaders = elfLoader.GetProgramHeaders(out error);
            if (error != ElfError.Success || progHeaders == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                using string errString = ((int)error).ToString();
                Console.WriteLine(
                    "FATAL ERROR: Could not read kernel executable program headers! Error code: " + errString);
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }

            for (int i = 0; i < progHeaders.Length; i++)
            {
                segmentDescriptors[numSegmentDescriptors] = new KernelExecutableInfo.SegmentDescriptor
                {
                    PhysicalStart = progHeaders[i].PhysicalAddress,
                    VirtualStart = progHeaders[i].VirtualAddress,
                    Size = progHeaders[i].SizeInMemory
                };
                numSegmentDescriptors++;

                error = elfLoader.LoadExecutableSegment(ref progHeaders[i], allocator);
                if (error != ElfError.Success && error != ElfError.ElfSectionNotLoadable)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    using string errString = ((int)error).ToString();
                    Console.WriteLine(
                        "FATAL ERROR: Could not load kernel executable program headers! Error code: " + errString);
                    Console.ForegroundColor = ConsoleColor.White;
                    return false;
                }
            }

            return true;
        }
        finally
        {
            progHeaders?.Dispose();
        }
    }

    private static bool AllocateKernelStackMemory(EfiBootServices* bs, PagingManager pgManager, KParams* kParams)
    {
        const PageFlags KSTACK_PAGE_FLAGS = PageFlags.Present | PageFlags.ReadPermission | PageFlags.WritePermission;
        const ulong PAGE_COUNT = VirtualAddressesInfo.KERNEL_STACK_SIZE / EfiConstants.EFI_PAGE_SIZE;

        ulong physicalAddress = 0;
        EfiStatus status = bs->AllocatePages(
            EfiAllocateType.AllocateAnyPages,
            EfiMemoryType.ChihuahuaKernelMemory,
            PAGE_COUNT,
            &physicalAddress);
        if (status != EfiStatus.Success)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            using string err = ((int)status).ToString();
            Console.WriteLine(
                "FATAL ERROR: Kernel stack allocation: failed to allocate memory; error code:" + err);
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }

        ulong baseAddress = VirtualAddressesInfo.KERNEL_HIGHEST_POSSIBLE_STACK_TOP -
                            VirtualAddressesInfo.KERNEL_STACK_SIZE;
        baseAddress -= Random.NextMersenne(0, 2048) * EfiConstants.EFI_PAGE_SIZE;
        kParams->VirtualSpaceInfo.KStackBottom = baseAddress;
        kParams->VirtualSpaceInfo.KStackTop = baseAddress + VirtualAddressesInfo.KERNEL_STACK_SIZE;

        PageError pageError = pgManager.MapRegion(
            physicalAddress,
            baseAddress,
            KSTACK_PAGE_FLAGS,
            PAGE_COUNT,
            out _);
        if (pageError != PageError.Success)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            using string err = ((int)pageError).ToString();
            Console.WriteLine(
                "FATAL ERROR: Kernel stack allocation: could not map a page; error code: " + err);
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }

        return true;
    }
}
