using System;
using System.Runtime.InteropServices;

namespace ChihuahuaOS.Bootloader.EfiApi;

[StructLayout(LayoutKind.Sequential)]
public struct EfiHandle
{
    private IntPtr _handle;
}

public enum EfiStatus : ulong
{
    Success = 0
}