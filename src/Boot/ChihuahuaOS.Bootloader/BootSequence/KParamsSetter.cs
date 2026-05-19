using System;
using ChihuahuaOS.Bootloader.EfiInteractions;
using ChihuahuaOS.BootParams;
using ChihuahuaOS.BootParams.ParamsData;
using ChihuahuaOS.CoreLib.Extra;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.BootServices;
using ChihuahuaOS.EfiApi.ConsoleSupport;
using ChihuahuaOS.MemPaginator;

namespace ChihuahuaOS.Bootloader.BootSequence;

internal static unsafe class KParamsSetter
{
    public static bool Setup(EfiBootServices* bs, PagingManager pagingManager, out ulong kParamsAddr)
    {
        kParamsAddr = 0;
        ulong physicalAddress = 0;
        EfiStatus status = bs->AllocatePages(
            EfiAllocateType.AllocateAnyPages,
            EfiMemoryType.EfiLoaderData,
            1,
            &physicalAddress);

        if (status != EfiStatus.Success)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            using string err = ((int)status).ToString();
            Console.WriteLine(
                "FATAL ERROR: Kernel parameters: failed to allocate memory; error code:" + err);
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }

        RawMemory.MemSet((void*)physicalAddress, 0, EfiConstants.EFI_PAGE_SIZE);

        PageError pageError = pagingManager.IdentityMapPage(
            physicalAddress,
            PageFlags.Present | PageFlags.ReadPermission | PageFlags.WritePermission);
        if (pageError != PageError.Success)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            using string err = ((int)pageError).ToString();
            Console.WriteLine(
                "FATAL ERROR: Kernel parameters: could not map a page; error code: " + err);
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }

        KParams* kParamsPtr = (KParams*)physicalAddress;
        *kParamsPtr = new KParams();
        kParamsAddr = (ulong)kParamsPtr;

        FbInfo* fbInfoPtr = (FbInfo*)((byte*)kParamsPtr + sizeof(KParams));
        bool success = SetFramebufferInfo(fbInfoPtr);
        if (!success)
        {
            return false;
        }

        kParamsPtr->FramebufferInfo = fbInfoPtr;

        return true;
    }

    private static bool SetFramebufferInfo(FbInfo* fbInfoPtr)
    {
        EfiGopMode? gopModeOpt = Gop.GetCurrentMode();
        if (gopModeOpt == null || gopModeOpt.Value.Info == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(
                "FATAL ERROR: Kernel parameters: framebuffer info: could not get GOP mode");
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }

        EfiGopMode gopMode = gopModeOpt.Value;
        *fbInfoPtr = new FbInfo();
        fbInfoPtr->Width = gopMode.Info->HorizontalResolution;
        fbInfoPtr->Height = gopMode.Info->VerticalResolution;
        fbInfoPtr->PixelsPerScanLine = gopMode.Info->PixelsPerScanLine;

        switch (gopMode.Info->PixelFormat)
        {
            case EfiGraphicsPixelFormat.PixelRgbReserved8BitPerColor:
            {
                fbInfoPtr->RedBitmask = 0xFF_00_00_00;
                fbInfoPtr->GreenBitmask = 0x00_FF_00_00;
                fbInfoPtr->BlueBitmask = 0x00_00_FF_00;
                fbInfoPtr->ReservedBitmask = 0x00_00_00_FF;
                break;
            }
            case EfiGraphicsPixelFormat.PixelBgrReserved8BitPerColor:
            {
                fbInfoPtr->BlueBitmask = 0xFF_00_00_00;
                fbInfoPtr->GreenBitmask = 0x00_FF_00_00;
                fbInfoPtr->RedBitmask = 0x00_00_FF_00;
                fbInfoPtr->ReservedBitmask = 0x00_00_00_FF;
                break;
            }
            case EfiGraphicsPixelFormat.PixelBitMask:
            {
                fbInfoPtr->RedBitmask = gopMode.Info->PixelInformation.RedBitmask;
                fbInfoPtr->GreenBitmask = gopMode.Info->PixelInformation.GreenBitmask;
                fbInfoPtr->BlueBitmask = gopMode.Info->PixelInformation.BlueBitmask;
                fbInfoPtr->ReservedBitmask = gopMode.Info->PixelInformation.ReservedBitmask;
                break;
            }
            default:
                return false;
        }

        return true;
    }
}