using System;
using System.Collections.Generic;
using System.IO;
using ChihuahuaOS.Bootloader.EfiInteractions;
using ChihuahuaOS.BootParams;
using ChihuahuaOS.CoreLib.Extra;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.BootServices;
using ChihuahuaOS.Elf;
using ChihuahuaOS.Elf.FileHeader;
using ChihuahuaOS.Elf.ProgramHeader;
using ChihuahuaOS.Elf.SectionHeader;
using ChihuahuaOS.MemPaginator;
using ChihuahuaOS.MinimalUtils;
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
            Console.WriteLine("Set the display resolution.");
        }
        else
        {
            //not fatal, we can go on
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                "WARN: Could not change the display resolution according to the settings." +
                " Continuing with the current mode.");
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
        Console.WriteLine("Retrieved the system memory map.");

        success = MemMap.SetupPagingStructures(efiMap, out PagingManager? pagingManagerOpt);
        efiMap.Dispose();

        if (success && pagingManagerOpt != null)
        {
            Console.WriteLine("Setup paging structures.");
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
        success = LoadKernelInMemory(pagingManager, out ulong kEntryPoint);
        if (success)
        {
            Console.WriteLine("Loaded kernel in memory.");
        }
        else
        {
            Fail();
            return;
        }

        success = AllocateKernelStackMemory(bs, pagingManager);
        if (success)
        {
            Console.WriteLine("Allocated stack memory for the kernel.");
        }
        else
        {
            Fail();
            return;
        }

        success = Gop.Remap(pagingManager);
        if (success)
        {
            Console.WriteLine("Remapped the framebuffer for use in OS.");
        }
        else
        {
            Fail();
            return;
        }

        success = KParamsSetter.Setup(bs, pagingManager, out ulong kParamsAddr);
        if (success)
        {
            Console.WriteLine("Set the kernel parameters");
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
            out EfiMemoryDescriptor* _,
            out ulong _,
            out ulong mapKey,
            out ulong _,
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

        ulong rootPageTable = pagingManager.GetRootPageTablePhysicalAddress();
        SetupAndJumpToKernel.Call(rootPageTable, kEntryPoint, kParamsAddr);

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

    private static bool LoadKernelInMemory(PagingManager pgManager, out ulong kernelEntryPoint)
    {
        kernelEntryPoint = 0;
        using string osVersion = BootedOsVersion.ToString();
        using string kernelFilePath = "\\EFI\\BOOT\\ChihuahuaOS.Kernel." + osVersion + ".elf";
        using FileStream? fs = File.OpenRead(kernelFilePath);
        if (fs == null)
        {
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
                EfiMemoryType.EfiLoaderData,
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

            for (ulong i = 0; i < pageCount; i++)
            {
                PageError pageError = pgManager.MapPage(
                    physicalAddress + EfiConstants.EFI_PAGE_SIZE * i,
                    address + EfiConstants.EFI_PAGE_SIZE * i,
                    pageFlags);

                if (pageError != PageError.Success)
                {
                    return 0;
                }
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
        ElfSectionHeader[]? sectionHeaders = null;
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
                elfLoader.LoadExecutableSegment(ref progHeaders[i], allocator);
            }

            sectionHeaders = elfLoader.GetSectionHeaders(out error);
            if (error != ElfError.Success || sectionHeaders == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                using string errString = ((int)error).ToString();
                Console.WriteLine(
                    "FATAL ERROR: Could not read kernel executable section headers! Error code: " + errString);
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }

            for (int i = 0; i < sectionHeaders.Length; i++)
            {
                elfLoader.LoadSection(ref sectionHeaders[i], allocator);
            }

            return true;
        }
        finally
        {
            progHeaders?.Dispose();
            sectionHeaders?.Dispose();
        }
    }

    private static bool AllocateKernelStackMemory(EfiBootServices* bs, PagingManager pgManager)
    {
        const PageFlags KSTACK_PAGE_FLAGS = PageFlags.Present | PageFlags.ReadPermission | PageFlags.WritePermission;
        const ulong KSTACK_SIZE = KVirtualAddresses.KERNEL_STACK_TOP - KVirtualAddresses.KERNEL_STACK_BOTTOM;
        //last page will be the stack overrun protector
        const ulong PAGE_COUNT = (KSTACK_SIZE + (EfiConstants.EFI_PAGE_SIZE - 1)) / EfiConstants.EFI_PAGE_SIZE + 1;

        ulong physicalAddress = 0;
        EfiStatus status = bs->AllocatePages(
            EfiAllocateType.AllocateAnyPages,
            EfiMemoryType.EfiLoaderData,
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

        RawMemory.MemSet((void*)physicalAddress, 0, PAGE_COUNT * EfiConstants.EFI_PAGE_SIZE);

        //map the overrun page
        PageError overrunPageError = pgManager.MapPage(
            physicalAddress,
            KVirtualAddresses.KERNEL_STACK_OVERRUN_PROTECTOR,
            PageFlags.Present);
        if (overrunPageError != PageError.Success)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            using string err = ((int)overrunPageError).ToString();
            Console.WriteLine(
                "FATAL ERROR: Kernel stack allocation: could not map the overrun page; error code: " + err);
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }

        for (ulong i = 0; i < PAGE_COUNT - 1; i++)
        {
            PageError pageError = pgManager.MapPage(
                physicalAddress + EfiConstants.EFI_PAGE_SIZE * (i + 1),
                KVirtualAddresses.KERNEL_STACK_BOTTOM + EfiConstants.EFI_PAGE_SIZE * i,
                KSTACK_PAGE_FLAGS);
            if (pageError != PageError.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                using string err = ((int)pageError).ToString();
                Console.WriteLine(
                    "FATAL ERROR: Kernel stack allocation: could not map a page; error code: " + err);
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
        }

        return true;
    }
}