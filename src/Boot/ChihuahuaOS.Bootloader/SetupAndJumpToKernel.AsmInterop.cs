using System.Diagnostics.CodeAnalisys;
using System.Runtime.InteropServices;

namespace ChihuahuaOS.Bootloader;

public static class SetupAndJumpToKernel
{
    [DoesNotReturn]
    public static void Call(ulong rootPageTablePhysicalAddress, ulong kernelEntryPoint, ulong kParamsAddr)
    {
        SetupAndJumpToKernel_Call(rootPageTablePhysicalAddress, kernelEntryPoint, kParamsAddr);
    }

    [DllImport("*")]
    private static extern void SetupAndJumpToKernel_Call(
        ulong rootPageTablePhysicalAddress,
        ulong kernelEntryPoint,
        ulong kParamsAddr);
}