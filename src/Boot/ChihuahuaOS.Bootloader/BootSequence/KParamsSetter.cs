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
    public static bool Setup(
        EfiBootServices* bs,
        PagingManager pagingManager,
        Span<KernelExecutableInfo.SegmentDescriptor> segmentDescriptors,
        int numSegmentDescriptors,
        out KParams* kParams)
    {
        kParams = null;
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
            PageFlags.Present | PageFlags.ReadPermission);
        if (pageError != PageError.Success)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            using string err = ((int)pageError).ToString();
            Console.WriteLine(
                "FATAL ERROR: Kernel parameters: could not map a page; error code: " + err);
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }

        kParams = (KParams*)physicalAddress;
        *kParams = new KParams();

        //set the segment descriptors
        kParams->KernelExecInfo = new KernelExecutableInfo
        {
            SegmentsDescriptorsArray =
                (KernelExecutableInfo.SegmentDescriptor*)(physicalAddress + (ulong)sizeof(KParams)),
            NumSegmentsLoaded = numSegmentDescriptors
        };
        for (int i = 0; i < numSegmentDescriptors; i++)
        {
            kParams->KernelExecInfo.SegmentsDescriptorsArray[i] = segmentDescriptors[i];
        }

        FbInfo fbInfo = new();
        bool success = SetFramebufferInfo(ref fbInfo);
        if (!success)
        {
            return false;
        }

        success = SetupFreeKernelMemory(bs, pagingManager, out ulong freeKMemoryPhysAddress);
        if (!success)
        {
            return false;
        }


        kParams->FramebufferInfo = fbInfo;
        kParams->VirtualSpaceInfo = new VirtualAddressesInfo();
        kParams->FreeMemChunkPhysicalAddress = freeKMemoryPhysAddress;
        return true;
    }

    private static bool SetFramebufferInfo(ref FbInfo fbInfo)
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
        fbInfo.Width = gopMode.Info->HorizontalResolution;
        fbInfo.Height = gopMode.Info->VerticalResolution;
        fbInfo.PixelsPerScanLine = gopMode.Info->PixelsPerScanLine;

        switch (gopMode.Info->PixelFormat)
        {
            case EfiGraphicsPixelFormat.PixelRgbReserved8BitPerColor:
            {
                fbInfo.RedBitmask = 0xFF_00_00_00;
                fbInfo.GreenBitmask = 0x00_FF_00_00;
                fbInfo.BlueBitmask = 0x00_00_FF_00;
                fbInfo.ReservedBitmask = 0x00_00_00_FF;
                break;
            }
            case EfiGraphicsPixelFormat.PixelBgrReserved8BitPerColor:
            {
                fbInfo.BlueBitmask = 0xFF_00_00_00;
                fbInfo.GreenBitmask = 0x00_FF_00_00;
                fbInfo.RedBitmask = 0x00_00_FF_00;
                fbInfo.ReservedBitmask = 0x00_00_00_FF;
                break;
            }
            case EfiGraphicsPixelFormat.PixelBitMask:
            {
                fbInfo.RedBitmask = gopMode.Info->PixelInformation.RedBitmask;
                fbInfo.GreenBitmask = gopMode.Info->PixelInformation.GreenBitmask;
                fbInfo.BlueBitmask = gopMode.Info->PixelInformation.BlueBitmask;
                fbInfo.ReservedBitmask = gopMode.Info->PixelInformation.ReservedBitmask;
                break;
            }
            default:
                return false;
        }

        return true;
    }

    private static bool SetupFreeKernelMemory(
        EfiBootServices* bs,
        PagingManager pagingManager,
        out ulong physicalAddress)
    {
        //the number of pages for 2 MiB
        const int NUM_PAGES = 512;

        ulong physAddress = 0;
        physicalAddress = 0;
        EfiStatus status = bs->AllocatePages(
            EfiAllocateType.AllocateAnyPages,
            EfiMemoryType.ChihuahuaFreeKernelMemory,
            NUM_PAGES,
            &physAddress);

        physicalAddress = physAddress;
        if (status != EfiStatus.Success || physicalAddress == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            using string err = ((int)status).ToString();
            Console.WriteLine(
                "FATAL ERROR: Kernel parameters: failed to allocate the free kernel memory; error code:" + err);
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }

        RawMemory.MemSet((void*)physicalAddress, 0, NUM_PAGES * EfiConstants.EFI_PAGE_SIZE);

        PageError pageError = pagingManager.IdentityMapRegion(
            physicalAddress,
            PageFlags.Present | PageFlags.ReadPermission | PageFlags.WritePermission,
            NUM_PAGES,
            out _);
        if (pageError != PageError.Success)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            using string err = ((int)pageError).ToString();
            Console.WriteLine(
                "FATAL ERROR: Kernel parameters: could not map a page of the free kernel memory; error code: " +
                err);
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }

        return true;
    }
}
