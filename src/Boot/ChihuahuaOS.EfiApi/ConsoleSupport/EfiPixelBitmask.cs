using System.Runtime.InteropServices;

namespace ChihuahuaOS.EfiApi.ConsoleSupport;

[StructLayout(LayoutKind.Sequential)]
public struct EfiPixelBitmask
{
    public uint RedBitmask;
    public uint GreenBitmask;
    public uint BlueBitmask;
    public uint ReservedBitmask;
}