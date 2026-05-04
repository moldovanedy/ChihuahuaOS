using System;

namespace ChihuahuaOS.Elf.ProgramHeader;

[Flags]
public enum ElfSegmentFlags
{
    None = 0,
    Executable = 1,
    Writable = 2,
    Readable = 4
}