using ChihuahuaOS.Kernel.MemoryManager.PMM;
using ChihuahuaOS.Kernel.MemoryManager.VMM;
using ChihuahuaOS.MemPaginator;

namespace ChihuahuaOS.Kernel.MemoryManager;

public static class MainMemManager
{
    public static ref VirtualMemManager Vmm => ref _vmm;
    private static VirtualMemManager _vmm;

    public static ref PhysicalMemManager Pmm => ref _pmm;
    private static PhysicalMemManager _pmm;

    public static ref PagingManager KPagingManager => ref _kPagingManager;
    private static PagingManager _kPagingManager;

    public static void KernelSetupPmm(ref PhysicalMemManager pmm)
    {
        _pmm = pmm;
    }

    public static void KernelSetupVmm(ref VirtualMemManager vmm)
    {
        _vmm = vmm;
    }

    public static void KernelSetupPagingManager(ref PagingManager kPagingManager)
    {
        _kPagingManager = kPagingManager;
    }
}
