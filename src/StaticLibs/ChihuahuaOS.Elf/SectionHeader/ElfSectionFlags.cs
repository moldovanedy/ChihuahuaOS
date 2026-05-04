using System;

namespace ChihuahuaOS.Elf.SectionHeader;

[Flags]
public enum ElfSectionFlags
{
    None = 0,
    Writable = 0x01,

    /// <summary>
    /// Needs to occupy memory during execution.
    /// </summary>
    Allocatable = 0x02,
    Executable = 0x04,
    Mergeable = 0x10,
    Strings = 0x20,

    /// <summary>
    /// ElfSectionHeader.Info contains SHT index (?).
    /// </summary>
    InfoLink = 0x40,

    /// <summary>
    /// Preserve order after combining.
    /// </summary>
    PreserveOrder = 0x80,

    /// <summary>
    /// OS-specific handling is required.
    /// </summary>
    NonConforming = 0x100,
    MemberOfGroup = 0x200,
    TlsData = 0x400
}