#if UEFI || DEBUG

using System.Diagnostics.CodeAnalisys;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.EfiSysTable;
using ChihuahuaOS.EfiApi.RuntimeServices;

namespace System;

public static unsafe class Environment
{
    internal static EfiSystemTable* EfiSysTable = null;

    [DoesNotReturn]
    public static void FailFast(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("Fatal error: ");
        Console.WriteLine(message);

        Console.WriteLine("Boot failed! Press any key to restart the device.");
        _ = Console.ReadKey();

        //restart
        EfiSysTable->RuntimeServices->ResetSystem(EfiResetType.EfiResetCold, EfiStatus.Aborted, 0, null);

        //this is unreachable, ResetSystem will not return
        while (true)
        {
        }
    }

    #region EFI specific

    /// <summary>
    /// This is only used on EFI platforms. Sets the pointer to the EFI system table, used by most services.
    /// Call this as soon as the program starts.
    /// </summary>
    /// <param name="systemTable"></param>
    public static void SetEfiSystemTableReference(EfiSystemTable* systemTable)
    {
        EfiSysTable = systemTable;
    }

    #endregion
}

#endif