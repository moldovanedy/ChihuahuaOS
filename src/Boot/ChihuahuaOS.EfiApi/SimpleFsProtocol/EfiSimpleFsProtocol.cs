using System.Runtime.InteropServices;

namespace ChihuahuaOS.EfiApi.SimpleFsProtocol;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct EfiSimpleFsProtocol
{
    public readonly ulong Revision;
    public readonly delegate* unmanaged<EfiSimpleFsProtocol*, EfiFileProtocol**, EfiStatus> OpenVolume;
}