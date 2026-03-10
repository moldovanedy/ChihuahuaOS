using System;
using System.Runtime.InteropServices;

namespace ChihuahuaOS.EfiApi;

[StructLayout(LayoutKind.Sequential)]
public struct EfiEvent
{
    private IntPtr _handle;
}