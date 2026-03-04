using System;
using System.Runtime;
using ChihuahuaOS.Bootloader.EfiApi;
using ChihuahuaOS.Bootloader.EfiApi.EfiSysTable;

namespace ChihuahuaOS.Bootloader;

internal static class Program
{
    private static void Main()
    {
    }

    [RuntimeExport("EfiMain")]
    public static unsafe int EfiMain(IntPtr imageHandle, EfiSystemTable* systemTable)
    {
        const string HELLO = "Hello world!\r\n";
        fixed (char* pHello = HELLO)
        {
            EfiStatus status = systemTable->ConOut->OutputString(systemTable->ConOut, pHello);
            if (status != EfiStatus.Success)
            {
                return 1;
            }
        }

        while (true)
        {
        }

        // return 0;
    }
}