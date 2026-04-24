using System.Runtime.InteropServices;

namespace ChihuahuaOS.EfiApi.SimpleFsProtocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EfiFileInfo
{
    public readonly ulong StructSize;
    public readonly ulong FileSize;
    public readonly ulong PhysicalSize;
    public readonly EfiTime CreateTime;
    public readonly EfiTime LastAccessTime;
    public readonly EfiTime ModificationTime;
    public readonly EfiFileAttributes Attribute;
}