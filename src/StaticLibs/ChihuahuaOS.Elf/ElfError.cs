namespace ChihuahuaOS.Elf;

public enum ElfError
{
    Success = 0,

    ///<summary>
    /// The ELF file header was corrupted.
    ///</summary>
    ElfFileHeaderCorrupted = 1,

    ///<summary>
    /// The ELF file type is not supported, for example, the ELF is not for x86_64, or it is not a version 1 ELF.
    ///</summary>
    ElfTypeNotSupported = 2,

    ///<summary>
    /// The section is not loadable, so it was skipped. It's not an error per se, it's just a different state.
    /// Applicable to program headers.
    ///</summary>
    ElfSectionNotLoadable = 3,

    ///<summary>
    /// A parameter indicated to a region outside the ELF file.
    ///</summary>
    SizeExceeded = 4,

    ///<summary>
    /// One of the ELF program headers was corrupted.
    ///</summary>
    ElfProgramHeaderCorrupted = 5,

    ///<summary>
    /// One of the ELF section headers was corrupted.
    ///</summary>
    ElfSectionHeaderCorrupted = 6,

    AllocatorError = 7,

    ///<summary>
    /// A generic error.
    ///</summary>
    UnknownError = int.MaxValue
}