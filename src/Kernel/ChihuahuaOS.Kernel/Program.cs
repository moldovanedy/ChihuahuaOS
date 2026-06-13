using System;
using System.Runtime.InteropServices;
using ChihuahuaOS.BootParams;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.CoreLib.ASM;
using ChihuahuaOS.Kernel.FramebufferManager;
using ChihuahuaOS.Kernel.MemoryManager;
using ChihuahuaOS.Kernel.MemoryManager.PMM;
using ChihuahuaOS.Kernel.MemoryManager.VMM;
using ChihuahuaOS.MemPaginator;
using ChihuahuaOS.MinimalUtils.ASM;

namespace ChihuahuaOS.Kernel;

internal static unsafe class Program
{
    internal static KParams* KernelParamsPtr { get; set; }

    internal static void Main()
    {
        CoreLibManager.Panic = &Panic;
        CoreLibManager.PrimitiveDebug = &PrimitiveDebug;

        Random.SeedLcg((int)Intrinsics.ReadTimestamp());

        Framebuffer.Init();
        TextRenderer.Init();
        ConsoleManager.Init();

        //clear old screen
        Framebuffer.Clear(new SolidColor(0x00_00_00));
        Console.WriteLine("Welcome to ChihuahuaOS!\0"u8);

        Random.SeedMersenne(Intrinsics.ReadTimestamp());

        EfiMapWrapper efiMap = new(
            KernelParamsPtr->EfiMemMapStart,
            (int)KernelParamsPtr->EfiMemMapNumEntries,
            KernelParamsPtr->EfiMemMapEntrySize);
        efiMap.Sort();

        InitializeMemoryManagers(ref efiMap);

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("System stopped, you can safely shut down the device.\0"u8);
        SpinLocks.HaltingInfiniteLoop();
    }

    private static void InitializeMemoryManagers(ref EfiMapWrapper efiMap)
    {
        PagingManager kPagingManager = new(
            (PageTable*)PagingManager.GetRootPageTableInitial(),
            &PageFrameAllocator.AllocPageFramesRaw);
        MainMemManager.KernelSetupPagingManager(ref kPagingManager);

        PageFrameAllocator.SetFreeKernelMemoryStart(KernelParamsPtr->FreeMemChunkPhysicalAddress);
        PhysicalMemManager pmm = new(efiMap);
        MainMemManager.KernelSetupPmm(ref pmm);
        Console.WriteLine("Setup physical memory manager (PMM)\0"u8);

        //update the paging manager so it directly uses the PMM instead of the free kernel memory
        PageFrameAllocator.Reset();
        kPagingManager = new PagingManager(
            (PageTable*)PagingManager.GetRootPageTableInitial(),
            &PageFrameAllocator.AllocPageFramesFromPmm);
        MainMemManager.KernelSetupPagingManager(ref kPagingManager);

        MainMemManager.Pmm.InitializeFromEfiMap(efiMap);
        Console.WriteLine("Initialized PMM from EFI memory map\0"u8);

        VirtualMemManager vmm = new();
        MainMemManager.KernelSetupVmm(ref vmm);
        Console.WriteLine("Setup virtual memory manager (VMM)\0"u8);

        MainMemManager.Vmm.InitializeFromCurrentState(efiMap, KernelParamsPtr);
        Console.WriteLine("Initialized VMM from the current memory state\0"u8);

        //now again, so it uses the VMM
        PageFrameAllocator.Reset();
        kPagingManager = new PagingManager(
            (PageTable*)PagingManager.GetRootPageTableInitial(),
            &PageFrameAllocator.AllocPageFramesFromVmm);
        MainMemManager.KernelSetupPagingManager(ref kPagingManager);

        HeapManager.Init();
    }


    [UnmanagedCallersOnly]
    private static void Panic(byte* message)
    {
        Framebuffer.Clear(new SolidColor(0xAA_00_00));

        Console.CursorLeft = 0;
        Console.CursorTop = 0;
        Console.BackgroundColor = ConsoleColor.DarkRed;
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("KERNEL PANIC: \0"u8);
        Console.WriteRaw(message);

        SpinLocks.HaltingInfiniteLoop();
    }

    [UnmanagedCallersOnly]
    private static void PrimitiveDebug(byte* message)
    {
        TextRenderer.DrawText(message);
    }
}
