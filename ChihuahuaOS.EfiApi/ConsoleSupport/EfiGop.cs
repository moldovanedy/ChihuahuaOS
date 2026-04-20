using System;
using System.Runtime.InteropServices;

namespace ChihuahuaOS.EfiApi.ConsoleSupport;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EfiGop
{
    /// <summary>
    /// The first argument is the GOP instance, the second one is the mode that you want to query, the third one is
    /// an out pointer to the size of the info structure, the fourth is an out pointer to the info structure pointer.
    /// </summary>
    public readonly delegate* unmanaged<EfiGop*, uint, ulong*, EfiGopModeInformation**, EfiStatus> QueryMode;

    /// <summary>
    /// The first argument is the GOP instance, the second one is the mode that you want to set.
    /// </summary>
    public readonly delegate* unmanaged<EfiGop*, uint, EfiStatus> SetMode;

    private IntPtr _blt;
    public readonly EfiGopMode* Mode;
}