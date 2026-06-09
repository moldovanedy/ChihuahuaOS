using System.Diagnostics.CodeAnalisys;
using System.Runtime.InteropServices;

namespace ChihuahuaOS.MinimalUtils.ASM;

public static class SpinLocks
{
    [DoesNotReturn]
    public static void HaltingInfiniteLoop()
    {
        SpinLocks_HaltInfLoop();
    }

    [DllImport("*")]
    private static extern void SpinLocks_HaltInfLoop();
}
