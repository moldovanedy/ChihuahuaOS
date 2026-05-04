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
            || progHeader.Offset + progHeader.SizeInFile >= (ulong)data.Length
            || progHeader.Offset + progHeader.SizeInMemory >= (ulong)data.Length)
        {
            return ElfError.SizeExceeded;
        }

        byte[] buffer = new byte[progHeader.SizeInFile];
        try
        {
            int numBytesRead = data.Read(buffer, 0, (int)progHeader.SizeInFile);
            if (numBytesRead < (int)progHeader.SizeInFile)
            {
                return ElfError.SizeExceeded;
            }

            unsafe
            {
                fixed (byte* src = buffer)
                {
                    RawMemory.MemMove(src, (void*)addr, progHeader.SizeInFile);
                }
            }
        }
        finally
        {
            buffer.Dispose();
        }

        //zero-out eventual mismatch between the size in file vs the size in memory
        if (progHeader.SizeInMemory > progHeader.SizeInFile)
        {
            unsafe
            {
                RawMemory.MemSet((void*)addr, 0, progHeader.SizeInMemory - progHeader.SizeInFile);
            }
        }

        return ElfError.Success;
    }
}