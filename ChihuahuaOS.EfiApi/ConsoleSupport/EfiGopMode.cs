using System.Runtime.InteropServices;

namespace ChihuahuaOS.EfiApi.ConsoleSupport;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EfiGopMode
{
    public uint MaxMode;
    public uint Mode;
    public EfiGopModeInformation* Info;
    public ulong SizeOfInfo;
    public nint FrameBufferBase;
    public ulong FrameBufferSize;
}