#if UEFI
using System.Diagnostics.CodeAnalisys;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.EfiApi.EfiSysTable;

namespace System;

public static unsafe class Environment
{
    public static EfiSystemTable* EfiSysTable { get; private set; } = null;

    [DoesNotReturn]
    public static void FailFast(string message)
    {
        CoreLibManager.Panic(message.ToCharPtrUnsafe());
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