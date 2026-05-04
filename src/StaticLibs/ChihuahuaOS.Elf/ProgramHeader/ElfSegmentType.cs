namespace ChihuahuaOS.Elf.ProgramHeader;

public enum ElfSegmentType
{
    /// <summary>
    /// Unused program header table entry.
    /// </summary>
    Unused = 0,
    Loadable = 1,
    DynamicLinkInfo = 2,
    InterpreterInfo = 3,
    AuxiliaryInfo = 4,
    Reserved = 5,

    /// <summary>
    /// The segment containing the program header itself.
    /// </summary>
    ProgramHeader = 6,
    TlsTemplate = 7,
    OsSpecificRangeStart = 0x60000000,
    OsSpecificRangeEnd = 0x6FFFFFFF
}