using System.Runtime.InteropServices;

namespace ChihuahuaOS.EfiApi.ConsoleSupport;

[StructLayout(LayoutKind.Sequential)]
public struct EfiGopModeInformation
{
    public uint Version;
    public uint HorizontalResolution;
    public uint VerticalResolution;
    public EfiGraphicsPixelFormat PixelFormat;
    public EfiPixelBitmask PixelInformation;
    public uint PixelsPerScanLine;
}