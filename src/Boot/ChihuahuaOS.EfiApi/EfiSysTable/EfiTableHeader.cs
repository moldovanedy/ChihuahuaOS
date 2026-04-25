using System.Runtime.InteropServices;

namespace ChihuahuaOS.EfiApi.EfiSysTable;

[StructLayout(LayoutKind.Sequential)]
public readonly struct EfiTableHeader
{
    public readonly ulong Signature;
    public readonly uint Revision;
    public readonly uint HeaderSize;
    public readonly uint Crc32;
    public readonly uint Reserved;
}