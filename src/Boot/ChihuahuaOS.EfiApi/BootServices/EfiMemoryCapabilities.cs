using System;

namespace ChihuahuaOS.EfiApi.BootServices;

/// <summary>
/// All these mention capabilities, not obligations, except Runtime, IsaValid, and IsaMask.
/// </summary>
[Flags]
public enum EfiMemoryCapabilities : ulong
{
    UnCacheable = 0x01,
    WriteCombining = 0x02,
    CacheableWithWriteThrough = 0x04,
    CacheableWithWriteBack = 0x08,
    NotCachableExportFetchAndAdd = 0x10,
    WriteProtected = 0x1000,
    ReadProtected = 0x2000,
    ExecuteProtected = 0x4000,
    NonVolatile = 0x8000,
    HighReliability = 0x10000,
    ReadOnly = 0x20000,
    SpecificPurpose = 0x40000,
    CryptoProtected = 0x80000,

    Runtime = 0x8000000000000000,
    IsaValid = 0x4000000000000000,
    IsaMask = 0x0FFFF00000000000
}