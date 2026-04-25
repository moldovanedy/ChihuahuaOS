using System;

namespace ChihuahuaOS.EfiApi.SimpleFsProtocol;

[Flags]
public enum EfiFileAttributes
{
    None = 0,
    ReadOnly = 0x01,
    Hidden = 0x02,
    System = 0x04,
    Reserved = 0x08,
    Directory = 0x10,
    Archive = 0x20,
    ValidAttr = 0x37
}