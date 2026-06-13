using System;
using System.Runtime.InteropServices;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.DogMalloc;
using ChihuahuaOS.Kernel.MemoryManager;

namespace ChihuahuaOS.Kernel;

public static unsafe class HeapManager
{
    public static void Init()
    {
        DogMallocManager.SetMemMapDelegate(&MemMap);
        DogMallocManager.SetMemUnmapDelegate(&MemUnmap);

        //test allocation
        ulong address = DogMallocManager.MemAlloc(16);
        if (address == 0)
        {
            CoreLibManager.Panic((byte*)"Kernel VMM: failed to allocate the initial kernel heap!\0"u8);
        }

        Console.Write("Successfully allocated kernel heap at address 0x\0"u8);
        Console.WriteLine(address, 16);
    }


    [UnmanagedCallersOnly]
    private static ulong MemMap(ulong size)
    {
        return MainMemManager.Vmm.ExpandKernelHeap(size);
    }

    [UnmanagedCallersOnly]
    private static void MemUnmap(ulong address)
    {
    }
}
