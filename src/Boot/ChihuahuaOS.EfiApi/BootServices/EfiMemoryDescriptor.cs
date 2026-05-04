using System.Runtime.InteropServices;

namespace ChihuahuaOS.EfiApi.BootServices;

[StructLayout(LayoutKind.Sequential)]
public readonly struct EfiMemoryDescriptor
{
    public readonly EfiMemoryType Type;
    public readonly ulong PhysicalStart;
    public readonly ulong VirtualStart;
    public readonly ulong NumberOfPages;
    public readonly EfiMemoryCapabilities Capabilities;
}