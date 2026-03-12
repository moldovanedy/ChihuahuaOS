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

    /// <summary>
    /// The first parameter is the variable name, the second is a reference to the EFI GUID of the vendor (can also
    /// be "global variable"), the third is an out pointer of the attributes, the fourth is an in/out indicating
    /// the size of the data buffer at input and the actual data size at output (might need to recall with a larger
    /// buffer if an error is returned), while the fifth is the actual data buffer.
    /// </summary>
    public readonly delegate* unmanaged<char*, EfiGuid*, EfiVariableAttributes*, ulong*, void*, EfiStatus> GetVariable;

    public readonly delegate* unmanaged<ulong*, char*, EfiGuid*, EfiStatus> GetNextVariableName;

    /// <summary>
    /// The same as with <see cref="GetVariable"/>, but with the following differences: the attributes are a direct
    /// value, not a pointer, and the size of the buffer is in-only.
    /// </summary>
    public readonly delegate* unmanaged<char*, EfiGuid*, EfiVariableAttributes, ulong, void*, EfiStatus> SetVariable;

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