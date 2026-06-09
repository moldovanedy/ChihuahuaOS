using System.Runtime.InteropServices;

namespace ChihuahuaOS.BootParams.ParamsData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FbInfo
{
    public uint Width;
    public uint Height;

    public uint RedBitmask;
    public uint GreenBitmask;
    public uint BlueBitmask;
    public uint ReservedBitmask;

    public uint PixelsPerScanLine;
    public uint Padding1;
}
