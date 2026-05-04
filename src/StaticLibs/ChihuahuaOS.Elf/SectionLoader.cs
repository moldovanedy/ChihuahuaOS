using System.IO;
using ChihuahuaOS.CoreLib.Extra;
using ChihuahuaOS.Elf.ProgramHeader;
using ChihuahuaOS.Elf.SectionHeader;

namespace ChihuahuaOS.Elf;

internal static class SectionLoader
{
    public static ElfError LoadNoBitsSection(
        ref ElfSectionHeader sectionHeader,
        ElfLoader.SegmentMemoryAllocator allocatorFn,
        Stream data)
    {
        if (sectionHeader.SectionSize <= 0 ||
            (sectionHeader.Flags & ElfSectionFlags.Allocatable) == ElfSectionFlags.None)
        {
            return ElfError.ElfSectionNotLoadable;
        }

        if (sectionHeader.OffsetInFile + sectionHeader.SectionSize >= (ulong)data.Length)
        {
            return ElfError.SizeExceeded;
        }

        ulong addr = allocatorFn(
            sectionHeader.SectionSize,
            sectionHeader.VirtualAddress,
            ElfSegmentFlags.Readable | ElfSegmentFlags.Writable);
        if (addr == 0)
        {
            return ElfError.AllocatorError;
        }

        unsafe
        {
            RawMemory.MemSet((void*)addr, 0, sectionHeader.SectionSize);
        }

        return ElfError.Success;
    }
}