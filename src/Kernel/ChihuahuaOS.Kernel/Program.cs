using System;
using System.Runtime.InteropServices;
using ChihuahuaOS.BootParams;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.Kernel.FramebufferManager;
using ChihuahuaOS.Kernel.MemoryManager.PMM;
using ChihuahuaOS.MemPaginator;
using ChihuahuaOS.MinimalUtils;

namespace ChihuahuaOS.Kernel;

internal static unsafe class Program
{
    internal static KParams* KernelParamsPtr { get; set; }
    internal static PhysicalMemManager Pmm { get; private set; }

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

        PmmPageFrameAllocator.SetFreeKernelMemoryStart(KernelParamsPtr->FreeMemChunkPhysicalAddress);
        PagingManager pagingManager = new(
            (PageTable*)PagingManager.GetRootPageTableInitial(),
            &PmmPageFrameAllocator.AllocPageFramesRaw);

        Pmm = new PhysicalMemManager(pagingManager, efiMap);
        Console.WriteLine("Setup physical memory manager\0"u8);

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
