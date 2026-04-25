using System;
using System.Runtime.InteropServices;

namespace ChihuahuaOS.EfiApi;

[StructLayout(LayoutKind.Sequential)]
public struct EfiHandle
{
    private IntPtr _handle;

    internal const ulong EFI_ERROR_MASK = 0x8000000000000000;
}