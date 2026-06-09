using System.Diagnostics.CodeAnalisys;
using System.Runtime.InteropServices;

namespace ChihuahuaOS.Bootloader.ASM;

public static class SetupAndJumpToKernel
{
    [DoesNotReturn]
    public static void Call(
        ulong rootPageTablePhysicalAddress,
        ulong kernelEntryPoint,
        ulong kParamsAddr,
        ulong stackTop)
    {
        SetupAndJumpToKernel_Call(rootPageTablePhysicalAddress, kernelEntryPoint, kParamsAddr, stackTop);
    }

    [DllImport("*")]
    private static extern void SetupAndJumpToKernel_Call(
        ulong rootPageTablePhysicalAddress,
        ulong kernelEntryPoint,
        ulong kParamsAddr,
        ulong stackTop);
}
