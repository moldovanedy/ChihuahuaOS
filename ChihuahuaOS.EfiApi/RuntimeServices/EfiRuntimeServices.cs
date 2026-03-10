using System;
using System.Runtime.InteropServices;
using ChihuahuaOS.EfiApi.EfiSysTable;

namespace ChihuahuaOS.EfiApi.RuntimeServices;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct EfiRuntimeServices
{
    public readonly EfiTableHeader Hdr;

    //
    // Time services
    //

    private readonly IntPtr _GetTime;
    private readonly IntPtr _SetTime;
    private readonly IntPtr _GetWakeupTime;
    private readonly IntPtr _SetWakeupTime;

    //
    // Virtual memory services
    //

    private readonly IntPtr _SetVirtualAddressMap;
    private readonly IntPtr _ConvertPointer;

    //
    // Variable services
    //

    private readonly IntPtr _GetVariable;
    private readonly IntPtr _GetNextVariableName;
    private readonly IntPtr _SetVariable;

    //
    // Misc
    //

    private readonly IntPtr _GetNextHighMonotonicCount;

    /// <summary>
    /// The first argument is the type of reset (generally either restart or shutdown), the second is the status which
    /// requires a reset (Success for normal operation, an error code for a serious error); the next 2 arguments
    /// are optional.
    /// </summary>
    public readonly delegate* unmanaged<EfiResetType, EfiStatus, ulong, void*, void> ResetSystem;

    private readonly IntPtr _UpdateCapsule;
    private readonly IntPtr _QueryCapsuleCapabilities;
    private readonly IntPtr _QueryVariableInfo;
}