using System;
using System.Runtime.InteropServices;
using ChihuahuaOS.BootParams;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.Kernel.FramebufferManager;
using ChihuahuaOS.Kernel.MemoryManager;
using ChihuahuaOS.Kernel.MemoryManager.PMM;
using ChihuahuaOS.Kernel.MemoryManager.VMM;
using ChihuahuaOS.MemPaginator;
using ChihuahuaOS.MinimalUtils;

namespace ChihuahuaOS.Kernel;

internal static unsafe class Program
{
    internal static KParams* KernelParamsPtr { get; set; }

    internal static void Main()
    {
        CoreLibManager.Panic = &Panic;
        CoreLibManager.PrimitiveDebug = &PrimitiveDebug;

        Framebuffer.Init();
        TextRenderer.Init();
        ConsoleManager.Init();

        //clear old screen
        Framebuffer.Clear(new SolidColor(0x00_00_00));
        Console.WriteLine("Welcome to ChihuahuaOS!\0"u8);

        EfiMapWrapper efiMap = new(
            KernelParamsPtr->EfiMemMapStart,
            (int)KernelParamsPtr->EfiMemMapNumEntries,
            KernelParamsPtr->EfiMemMapEntrySize);
        efiMap.Sort();

        PagingManager kPagingManager = new(
            (PageTable*)PagingManager.GetRootPageTableInitial(),
            &PmmPageFrameAllocator.AllocPageFramesRaw);
        MainMemManager.KernelSetupPagingManager(ref kPagingManager);

        PmmPageFrameAllocator.SetFreeKernelMemoryStart(KernelParamsPtr->FreeMemChunkPhysicalAddress);
        PhysicalMemManager pmm = new(kPagingManager, efiMap);
        MainMemManager.KernelSetupPmm(ref pmm);
        Console.WriteLine("Setup physical memory manager (PMM)\0"u8);

        MainMemManager.Pmm.InitializeFromEfiMap(efiMap);
        Console.WriteLine("Initialized PMM from EFI memory map\0"u8);

        VirtualMemManager vmm = new(kPagingManager);
        Console.WriteLine("Setup virtual memory manager (VMM)\0"u8);
        MainMemManager.KernelSetupVmm(ref vmm);

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("System stopped, you can safely shut down the device.\0"u8);
        SpinLocks.HaltingInfiniteLoop();
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
