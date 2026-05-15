using System.Diagnostics.CodeAnalisys;
using System.Runtime.InteropServices;

namespace ChihuahuaOS.Bootloader;

public static class SetupAndJumpToKernel
{
    [DoesNotReturn]
    public static void Call(ulong rootPageTablePhysicalAddress, ulong kernelEntryPoint)
    {
        SetupAndJumpToKernel_Call(rootPageTablePhysicalAddress, kernelEntryPoint);
    }

    [DllImport("*")]
    private static extern void SetupAndJumpToKernel_Call(ulong rootPageTablePhysicalAddress, ulong kernelEntryPoint);
}