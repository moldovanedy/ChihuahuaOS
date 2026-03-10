using System;
using System.Diagnostics.CodeAnalisys;
using System.Runtime;
using ChihuahuaOS.Bootloader.Tui;
using ChihuahuaOS.EfiApi.EfiSysTable;

namespace ChihuahuaOS.Bootloader;

internal static class Program
{
    private static void Main()
    {
    }

    [DoesNotReturn]
    [RuntimeExport("EfiMain")]
    public static unsafe int EfiMain(IntPtr imageHandle, EfiSystemTable* systemTable)
    {
        //disable the watchdog; we only need it after we try to boot
        systemTable->BootServices->SetWatchdogTimer(0, 0, 0, null);
        Environment.SetEfiSystemTableReference(systemTable);

        Console.Clear();
        TuiRenderer.DrawPersistentElements();

        while (true)
        {
            TuiRenderer.RedrawMainContent();

            //NOTE: this will only be needed when changing the context (like entering settings, closing a pop-up, etc.)
            TuiRenderer.RedrawBottomInstructions();

            _ = Console.ReadKey();
        }

        // return 0;
    }
}