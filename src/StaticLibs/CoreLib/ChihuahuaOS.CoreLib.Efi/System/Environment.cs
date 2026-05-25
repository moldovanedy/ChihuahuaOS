#if UEFI
using System.Diagnostics.CodeAnalisys;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.EfiApi.EfiSysTable;

namespace System;

public static unsafe class Environment
{
    public static IntPtr EfiImageHandle { get; private set; } = IntPtr.Zero;

    public static EfiSystemTable* EfiSysTable { get; private set; } = null;

    [DoesNotReturn]
    public static void FailFast(string message)
    {
        CoreLibManager.Panic(message.ToCharPtrUnsafe());
    }

    /// <summary>
    /// This is only used on EFI platforms. Sets the pointer to the EFI system table, used by most services, as well
    /// as the image handle. Call this as soon as the program starts.
    /// </summary>
    /// <param name="systemTable"></param>
    /// <param name="imageHandle"></param>
    public static void SetEfiSystemReferences(IntPtr imageHandle, EfiSystemTable* systemTable)
    {
        EfiImageHandle = imageHandle;
        EfiSysTable = systemTable;
    }
}

#endif