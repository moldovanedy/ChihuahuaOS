#if ARCH_X64
using System.Runtime;
using ChihuahuaOS.BootParams;

namespace ChihuahuaOS.Kernel.Arch.X64;

public static class KernelInit
{
    [RuntimeExport("KInit")]
    public static unsafe void KInit(KParams* kParamsPtr)
    {
        Program.KernelParamsPtr = kParamsPtr;
        Program.Main();
    }
}
#endif