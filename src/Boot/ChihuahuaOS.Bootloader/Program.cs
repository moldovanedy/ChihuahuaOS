using System;
using System.Runtime;
using ChihuahuaOS.Bootloader.Tui;
using ChihuahuaOS.EfiApi.EfiSysTable;

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
}