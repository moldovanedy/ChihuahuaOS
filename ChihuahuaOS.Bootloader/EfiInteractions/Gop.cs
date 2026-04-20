using System;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.ConsoleSupport;

namespace ChihuahuaOS.Bootloader.EfiInteractions;

/// <summary>
/// Utilities to access the EFI Graphics Output Protocol.
/// </summary>
public static unsafe partial class Gop
{
    /// <summary>
    /// Do not use directly! Use <see cref="GetOrFindGop"/> instead.
    /// </summary>
    private static EfiGop* _gop;

    public static int GetModeCount()
    {
        EfiGop* gop = GetOrFindGop();
        if (gop == null)
        {
            return 0;
        }

        return (int)gop->Mode->MaxMode;
    }

    public static GopModeInfoEnumerator? GetModeInfoEnumerator()
    {
        EfiGop* gop = GetOrFindGop();
        if (gop == null)
        {
            return null;
        }

        return new GopModeInfoEnumerator(gop);
    }

    public static bool SetMode(uint modeNumber)
    {
        EfiGop* gop = GetOrFindGop();
        if (gop == null)
        {
            return false;
        }

        EfiStatus status = gop->SetMode(gop, modeNumber);
        return status == EfiStatus.Success;
    }

    public static EfiGop* GetGopHandleUnsafe()
    {
        return GetOrFindGop();
    }

    private static EfiGop* GetOrFindGop()
    {
        if (_gop != null)
        {
            return _gop;
        }

        EfiGuid gopGuid = AllEfiGuids.EfiGop;
        EfiGop* gop;
        EfiStatus status = Environment.EfiSysTable->BootServices->LocateProtocol(&gopGuid, null, (void**)&gop);
        if (status != EfiStatus.Success)
        {
            return null;
        }

        _gop = gop;
        return gop;
    }
}