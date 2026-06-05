using System.IO;
using ChihuahuaOS.CoreLib.Extra;
using ChihuahuaOS.Elf.ProgramHeader;

namespace ChihuahuaOS.Elf;

internal static class SegmentLoader
{
    public static ElfError LoadExecutableSegment(
        ref ElfProgramHeader progHeader,
        ElfLoader.SegmentMemoryAllocator allocatorFn,
        Stream data)
    {
        ulong addr = allocatorFn(progHeader.SizeInMemory, progHeader.VirtualAddress, progHeader.Flags);
        if (addr == 0)
        {
            return ElfError.AllocatorError;
        }

        if (
            progHeader.Offset >= (ulong)data.Length
            || progHeader.Offset + progHeader.SizeInFile >= (ulong)data.Length)
        {
            return ElfError.SizeExceeded;
        }

        data.Position = (long)progHeader.Offset;
        unsafe
        {
            int numBytesRead = data.ReadRaw((byte*)addr, 0, (int)progHeader.SizeInFile);
            if (numBytesRead < (int)progHeader.SizeInFile)
            {
                return ElfError.SizeExceeded;
            }
        }

        //zero-out eventual mismatch between the size in file vs the size in memory
        if (progHeader.SizeInMemory > progHeader.SizeInFile)
        {
            unsafe
            {
                RawMemory.MemSet(
                    (void*)(addr + progHeader.SizeInFile),
                    0,
                    progHeader.SizeInMemory - progHeader.SizeInFile);
            }
        }

        return ElfError.Success;
    }
}
