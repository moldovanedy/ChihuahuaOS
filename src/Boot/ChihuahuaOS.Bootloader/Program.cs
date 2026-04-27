using System;
using System.Runtime;
using System.Runtime.InteropServices;
using ChihuahuaOS.Bootloader.Tui;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.BootServices;
using ChihuahuaOS.EfiApi.EfiSysTable;
using ChihuahuaOS.EfiApi.RuntimeServices;
using Internal.Runtime.CompilerHelpers;

namespace ChihuahuaOS.Bootloader;

internal static class Program
{
    //for future use (access embedded files without external APIs):
    // objcopy -I binary -O elf64-x86-64 -B i386:x86-64 font.psf font.o
    //
    // unsafe static class Resources
    // {
    //     public static extern byte _binary_font_psf_start;
    //     public static extern byte _binary_font_psf_end;
    // }
    //
    // byte* start = (byte*)(&Resources._binary_font_psf_start);
    // byte* end = (byte*)(&Resources._binary_font_psf_end);
    // ulong size = (ulong)(end - start);
    // byte* data = start;

    private static void Main()
    {
    }

    [RuntimeExport("EfiMain")]
    public static unsafe int EfiMain(IntPtr imageHandle, EfiSystemTable* systemTable)
    {
        //disable the watchdog; we only need it after we try to boot
        systemTable->BootServices->SetWatchdogTimer(0, 0, 0, null);
        Environment.SetEfiSystemTableReference(systemTable);

        CoreLibManager.Panic = &PanicHandler;
        CoreLibManager.Malloc = &MallocHandler;
        CoreLibManager.Free = &FreeHandler;

        Console.Clear();
        Console.CursorVisible = false;
        TuiRenderer.DrawPersistentElements();

        ConsoleKeyInfo keyStroke = new();
        while (true)
        {
            TuiRenderer.RedrawMainContent(keyStroke);

            //abort booting and give control to the next UEFI app
            if (MainScreen.WantsBootAbort())
            {
                return 0;
            }

            keyStroke = TuiRenderer.WantsForcedRedraw() ? new ConsoleKeyInfo() : Console.ReadKey();
        }
    }

    [UnmanagedCallersOnly]
    private static unsafe void PanicHandler(char* errorMsg)
    {
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("Fatal error: ");
        Console.WriteRaw(errorMsg);

        Console.WriteLine(" Boot failed! Press any key to restart the device.");
        _ = Console.ReadKey();

        //restart
        Environment.EfiSysTable->RuntimeServices->ResetSystem(EfiResetType.EfiResetCold, EfiStatus.Aborted, 0, null);

        //this is unreachable, ResetSystem will not return
        while (true)
        {
        }
    }

    [UnmanagedCallersOnly]
    private static unsafe void* MallocHandler(uint size)
    {
        void* result;
        if (Environment.EfiSysTable == null)
        {
            ThrowHelpers.ThrowNullReferenceException();
            return null;
        }

        EfiStatus status =
            Environment.EfiSysTable->BootServices->AllocatePool(EfiMemoryType.EfiLoaderData, size, &result);
        if (status != EfiStatus.Success)
        {
            result = null;
        }

        if (result == null)
        {
            Environment.FailFast("Allocation failed");
        }

        return result;
    }

    [UnmanagedCallersOnly]
    private static unsafe void FreeHandler(void* ptr)
    {
        EfiSystemTable* st = Environment.EfiSysTable;
        if (st == null)
        {
            return;
        }

        st->BootServices->FreePool(ptr);
    }
}