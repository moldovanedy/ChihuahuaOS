namespace ChihuahuaOS.Elf.SectionHeader;

public enum ElfSectionType
{
    Null = 0,
    ProgramData = 1,
    SymbolTable = 2,
    StringTable = 3,
    RelocationWithAddends = 4,
    SymbolHashTable = 5,
    DynamicLinkInfo = 6,
    Notes = 7,
    NoBits = 8,
    RelocationEntries = 9,
    Reserved = 0x0A,
    DynamicLinkSymbolTable = 0x0B,
    InitArray = 0x0E,
    FiniArray = 0x0F,
    PreInitArray = 0x10,
    SectionGroup = 0x11,
    ExtendedSectionIndices = 0x12
}