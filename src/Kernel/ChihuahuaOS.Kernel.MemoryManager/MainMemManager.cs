using ChihuahuaOS.Kernel.MemoryManager.PMM;
using ChihuahuaOS.Kernel.MemoryManager.VMM;
using ChihuahuaOS.MemPaginator;

namespace ChihuahuaOS.Kernel.MemoryManager;

public static class MainMemManager
{
    public static PhysicalMemManager Pmm { get; private set; }

    public static VirtualMemManager Vmm { get; private set; }

    public static PagingManager KPagingManager { get; private set; }

    public static void KernelSetupPmm(ref PhysicalMemManager pmm)
    {
        Pmm = pmm;
    }

    public static void KernelSetupVmm(ref VirtualMemManager vmm)
    {
        Vmm = vmm;
    }

    public static void KernelSetupPagingManager(ref PagingManager kPagingManager)
    {
        KPagingManager = kPagingManager;
    }
}
