using System.Runtime.InteropServices;

namespace ChihuahuaOS.EfiApi.ConsoleSupport;

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