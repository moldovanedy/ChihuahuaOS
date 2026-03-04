using System;
using System.Runtime.InteropServices;

namespace ChihuahuaOS.Bootloader.EfiApi.ConsoleSupport;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct EfiSimpleOutputProtocol
{
    public readonly delegate* unmanaged<EfiSimpleOutputProtocol*, bool, EfiStatus> Reset;
    public readonly delegate* unmanaged<EfiSimpleOutputProtocol*, char*, EfiStatus> OutputString;
    private readonly IntPtr _pad1;
    private readonly IntPtr _pad2;
    private readonly IntPtr _pad3;
    private readonly IntPtr _pad4;
    private readonly IntPtr _pad5;
    private readonly IntPtr _pad6;
    private readonly IntPtr _pad7;

    public readonly SimpleTextOutputMode* Mode;
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct SimpleTextOutputMode
{
    public readonly int MaxMode;

    // current settings
    public readonly int Mode;
    public readonly int Attribute;
    public readonly int CursorColumn;
    public readonly int CursorRow;
    public readonly bool CursorVisible;
}