namespace ChihuahuaOS.Elf.FileHeader;

public enum HeaderIdentifierIndex
{
    /// <summary>
    /// 0x7F
    /// </summary>
    Magic0 = 0,

    /// <summary>
    /// 'E' (0x45)
    /// </summary>
    Magic1 = 1,

    /// <summary>
    /// 'L' (0x4C)
    /// </summary>
    Magic2 = 2,

    /// <summary>
    /// 'F' (0x46)
    /// </summary>
    Magic3 = 3,

    /// <summary>
    /// Architecture: always 2 (64-bit).
    /// </summary>
    Class = 4,

    /// <summary>
    /// Endianness: 1 for little endian, 2 for big endian.
    /// </summary>
    Data = 5,

    /// <summary>
    /// ELF version: Always 1.
    /// </summary>
    Version = 6,

    /// <summary>
    /// OS ABI: normally 0 (SysV) for bare metal.
    /// </summary>
    OsAbi = 7,

    /// <summary>
    /// OS specific; generally ignored.
    /// </summary>
    AbiVersion = 8,

    /// <summary>
    /// Padding bytes: 7 bytes that should be zeroed when written, ignored when read.
    /// </summary>
    Pad = 9
}