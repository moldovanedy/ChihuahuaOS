using System.Runtime.InteropServices;

namespace ChihuahuaOS.CoreLib.ASM;

public static class Intrinsics
{
    public static ulong ReadTimestamp()
    {
        return Intrinsics_ReadTimestamp();
    }

    [DllImport("*")]
    private static extern ulong Intrinsics_ReadTimestamp();
}
