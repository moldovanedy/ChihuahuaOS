using System.Runtime.InteropServices;

namespace ChihuahuaOS.EfiApi.ConsoleSupport;

[StructLayout(LayoutKind.Sequential)]
public readonly struct EfiInputKey
{
    public readonly ushort ScanCode;
    public readonly char UnicodeChar;
}