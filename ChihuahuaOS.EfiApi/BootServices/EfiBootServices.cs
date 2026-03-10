using System;
using System.Runtime.InteropServices;
using ChihuahuaOS.EfiApi.EfiSysTable;

namespace ChihuahuaOS.EfiApi.BootServices;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct EfiBootServices
{
    public readonly EfiTableHeader Hdr;

    //
    // Task priority functions
    //

    private readonly IntPtr _RaiseTPL;
    private readonly IntPtr _RestoreTPL;

    //
    // Memory functions
    //

    private readonly IntPtr _AllocatePages;
    private readonly IntPtr _FreePages;
    private readonly IntPtr _GetMemoryMap;
    public readonly delegate* unmanaged<EfiMemoryType, ulong, void**, EfiStatus> AllocatePool;
    public readonly delegate* unmanaged<void*, EfiStatus> FreePool;

    //
    // Event & timer functions
    //

    private readonly IntPtr _CreateEvent;
    private readonly IntPtr _SetTimer;

    /// <summary>
    ///     The first parameter represents the number of events in the events array, the second parameter is the number of
    ///     events, and the third parameter is an out pointer for the index of the event that fired.
    /// </summary>
    public readonly delegate* unmanaged<ulong, EfiEvent*, ulong*, EfiStatus> WaitForEvent;

    private readonly IntPtr _SignalEvent;
    private readonly IntPtr _CloseEvent;
    private readonly IntPtr _CheckEvent;

    //
    // Protocol handler functions
    //

    private readonly IntPtr InstallProtocolInterface;
    private readonly IntPtr ReinstallProtocolInterface;
    private readonly IntPtr UninstallProtocolInterface;
    private readonly IntPtr HandleProtocol;
    private readonly IntPtr PCHandleProtocol;
    private readonly IntPtr RegisterProtocolNotify;
    private readonly IntPtr LocateHandle;
    private readonly IntPtr LocateDevicePath;
    private readonly IntPtr InstallConfigurationTable;

    //
    // Image functions
    //

    private readonly IntPtr LoadImage;
    private readonly IntPtr StartImage;
    private readonly IntPtr Exit;
    private readonly IntPtr UnloadImage;
    private readonly IntPtr ExitBootServices;

    //
    // Misc functions
    //

    private readonly IntPtr GetNextMonotonicCount;
    private readonly IntPtr Stall;

    /// <summary>
    ///     The first param is the number of seconds for the watchdog timer (0 disables it completely). The others can be
    ///     0.
    /// </summary>
    public readonly delegate* unmanaged<ulong, ulong, ulong, char*, EfiStatus> SetWatchdogTimer;

    //
    // DriverSupport Services
    //

    private readonly IntPtr ConnectController;
    private readonly IntPtr DisconnectController;

    //
    // Open and Close Protocol Services
    //
    private readonly IntPtr OpenProtocol;
    private readonly IntPtr CloseProtocol;
    private readonly IntPtr OpenProtocolInformation;

    //
    // Library Services
    //
    private readonly IntPtr ProtocolsPerHandle;
    private readonly IntPtr LocateHandleBuffer;
    private readonly IntPtr LocateProtocol;
    private readonly IntPtr InstallMultipleProtocolInterfaces;
    private readonly IntPtr UninstallMultipleProtocolInterfaces;

    //
    // 32-bit CRC Services
    //
    private readonly IntPtr CalculateCrc32;

    //
    // Misc Services
    //
    private readonly IntPtr CopyMem;
    private readonly IntPtr SetMem;
    private readonly IntPtr CreateEventEx;
}