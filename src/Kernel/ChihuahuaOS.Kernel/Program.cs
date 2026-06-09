using System;
using System.Runtime.InteropServices;
using ChihuahuaOS.BootParams;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.CoreLib.ASM;
using ChihuahuaOS.EfiApi;
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

        PagingManager kPagingManager = new(
            (PageTable*)PagingManager.GetRootPageTableInitial(),
            &PmmPageFrameAllocator.AllocPageFramesRaw);
        MainMemManager.KernelSetupPagingManager(ref kPagingManager);

        PmmPageFrameAllocator.SetFreeKernelMemoryStart(KernelParamsPtr->FreeMemChunkPhysicalAddress);
        PhysicalMemManager pmm = new(efiMap);
        MainMemManager.KernelSetupPmm(ref pmm);
        Console.WriteLine("Setup physical memory manager (PMM)\0"u8);

        MainMemManager.Pmm.InitializeFromEfiMap(efiMap);
        Console.WriteLine("Initialized PMM from EFI memory map\0"u8);

        VirtualMemManager vmm = new();
        Console.WriteLine("Setup virtual memory manager (VMM)\0"u8);
        MainMemManager.KernelSetupVmm(ref vmm);

        MainMemManager.Vmm.InitializeFromCurrentState(efiMap, KernelParamsPtr);
        Console.WriteLine("Initialized VMM from the current memory state\0"u8);

        //create the initial kernel heap (64 KiB)
        ulong address = MainMemManager.Vmm.AllocateKernelVirtualMem(16 * EfiConstants.EFI_PAGE_SIZE);
        if (address == 0)
        {
            CoreLibManager.Panic((byte*)"Kernel VMM: failed to allocate the initial kernel heap!\0"u8);
        }

        Console.Write("Successfully allocated kernel heap at address 0x\0"u8);
        Console.WriteLine(address, 16);

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
