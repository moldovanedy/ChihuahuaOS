using System.Runtime.InteropServices;

namespace ChihuahuaOS.EfiApi.SimpleFsProtocol;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct EfiFileProtocol
{
    public const ulong Revision1 = 0x10000;
    public const ulong Revision2 = 0x20000;

    public readonly ulong Revision;

    public readonly delegate* unmanaged<
        EfiFileProtocol*, EfiFileProtocol**, char*, EfiFileOpenMode, EfiFileAttributes, EfiStatus> Open;

    public readonly delegate* unmanaged<EfiFileProtocol*, EfiStatus> Close;
    public readonly delegate* unmanaged<EfiFileProtocol*, EfiStatus> Delete;
    public readonly delegate* unmanaged<EfiFileProtocol*, ulong*, void*, EfiStatus> Read;
    public readonly delegate* unmanaged<EfiFileProtocol*, ulong*, void*, EfiStatus> Write;
    public readonly delegate* unmanaged<EfiFileProtocol*, ulong*, EfiStatus> GetPosition;
    public readonly delegate* unmanaged<EfiFileProtocol*, ulong, EfiStatus> SetPosition;

    public readonly delegate* unmanaged<EfiFileProtocol*, EfiGuid*, ulong*, void*, EfiStatus> GetInfo;
    private readonly nint _setInfo;

    public readonly delegate* unmanaged<EfiFileProtocol*, EfiStatus> Flush;

    private readonly nint _openEx;
    private readonly nint _readEx;
    private readonly nint _writeEx;
    private readonly nint _flushEx;
}