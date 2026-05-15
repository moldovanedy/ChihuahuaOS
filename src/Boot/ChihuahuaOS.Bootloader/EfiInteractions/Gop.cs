using System;
using ChihuahuaOS.BootParams;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.ConsoleSupport;
using ChihuahuaOS.MemPaginator;

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

    public static bool Remap(PagingManager pagingManager)
    {
        EfiGop* gop = GetOrFindGop();
        if (gop == null)
        {
            return false;
        }

        ulong oldBase = (ulong)gop->Mode->FrameBufferBase;
        ulong pageCount = (gop->Mode->FrameBufferSize + (EfiConstants.EFI_PAGE_SIZE - 1)) / EfiConstants.EFI_PAGE_SIZE;
        for (ulong i = 0; i < pageCount; i++)
        {
            PageError error = pagingManager.MapPage(
                oldBase + i * EfiConstants.EFI_PAGE_SIZE,
                KVirtualAddresses.GOP_BASE + i * EfiConstants.EFI_PAGE_SIZE,
                PageFlags.Present | PageFlags.ReadPermission | PageFlags.WritePermission);

            if (error != PageError.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("FATAL ERROR: Could not remap the framebuffer for use in OS!");
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
        }

        return true;
    }

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

    public static EfiGopMode? GetCurrentMode()
    {
        EfiGop* gop = GetOrFindGop();
        if (gop == null)
        {
            return null;
        }

        return *gop->Mode;
    }

    public static EfiGopModeInformation? GetMode(uint modeNumber)
    {
        EfiGop* gop = GetOrFindGop();
        if (gop == null || modeNumber >= gop->Mode->MaxMode)
        {
            return null;
        }

        ulong structSize = 0;
        EfiGopModeInformation* info;
        EfiStatus status = gop->QueryMode(gop, modeNumber, &structSize, &info);
        if (status != EfiStatus.Success)
        {
            return null;
        }

        return *info;
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