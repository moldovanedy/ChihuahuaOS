using System;

namespace ChihuahuaOS.EfiApi.SimpleFsProtocol;

[Flags]
public enum EfiFileOpenMode : ulong
{
    Read = 1,
    Write = 2,
    Create = 0x80_00_00_00_00_00_00_00
}