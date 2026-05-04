namespace ChihuahuaOS.Elf.FileHeader;

public enum ElfAppType : ushort
{
    /// <summary>
    /// Unknown.
    /// </summary>
    None = 0,

    Relocatable = 1,
    Executable = 2,
    DynamicLib = 3,
    Core = 4,

    /// <summary>
    /// Start of OS-specific range; generally ignored.
    /// </summary>
    OsSpecificRangeStart = 0xFE00,

    /// <summary>
    /// End of OS-specific range; generally ignored.
    /// </summary>
    OsSpecificRangeEnd = 0xFEFF
}