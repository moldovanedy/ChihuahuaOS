using System;
using System.IO;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.CoreLib.Extra.Runtime;
using ChihuahuaOS.Elf.FileHeader;
using ChihuahuaOS.Elf.ProgramHeader;
using ChihuahuaOS.Elf.SectionHeader;

namespace ChihuahuaOS.Elf;

public class ElfLoader : IDisposable
{
    /// <summary>
    /// The function that needs to allocate memory for a particular ELF program segment.
    /// </summary>
    /// <param name="memSize">The requested memory size (in bytes).</param>
    /// <param name="virtualAddress">The virtual address at which the segment requests to be loaded.</param>
    /// <param name="flags">The segment flags.</param>
    /// <returns>The address at which this loader can write the data.</returns>
    public delegate ulong SegmentMemoryAllocator(ulong memSize, ulong virtualAddress, ElfSegmentFlags flags);


    private Stream _stream;

    public ElfLoader(Stream stream)
    {
        _stream = stream;
    }

    public void Dispose()
    {
        _stream = null!;
        MemUtils.FreeMemory(this);
    }

    public ElfHeader? GetElfHeader(out ElfError error)
    {
        long previousStreamPosition = _stream.Position;

        try
        {
            _stream.Position = 0;
            ElfHeader? headerOpt = ElfHeader.ParseHeader(_stream);
            if (headerOpt == null)
            {
                error = ElfError.ElfFileHeaderCorrupted;
                return null;
            }

            ElfHeader header = headerOpt.Value;
            error = CheckElfHeader(ref header);
            if (error != ElfError.Success)
            {
                return null;
            }

            return header;
        }
        finally
        {
            _stream.Position = previousStreamPosition;
        }
    }

    public ElfProgramHeader[]? GetProgramHeaders(out ElfError error)
    {
        ElfHeader? headerOpt = GetElfHeader(out error);
        if (error != ElfError.Success || headerOpt == null)
        {
            return null;
        }

        ElfHeader header = headerOpt.Value;

        //it shouldn't be different, but you never know...
        if (header.ProgHeaderEntrySize != ElfProgramHeader.FILE_HEADER_SIZE)
        {
            error = ElfError.ElfFileHeaderCorrupted;
            return null;
        }

        ushort numEntries = header.ProgHeaderTableEntriesNum;
        int arraySizeBytes = numEntries * ElfProgramHeader.FILE_HEADER_SIZE;
        ElfProgramHeader[] programHeaders = new ElfProgramHeader[numEntries];

        ulong endPosition = header.ProgHeaderOffset + (ulong)arraySizeBytes;
        if (endPosition > (ulong)_stream.Length)
        {
            error = ElfError.SizeExceeded;
            programHeaders.Dispose();
            return null;
        }

        _stream.Position = (long)header.ProgHeaderOffset;
        for (int i = 0; i < numEntries; i++)
        {
            ElfProgramHeader? progHeaderOpt = ElfProgramHeader.ParseHeader(_stream);
            if (progHeaderOpt == null)
            {
                error = ElfError.ElfProgramHeaderCorrupted;
                programHeaders.Dispose();
                return null;
            }

            programHeaders[i] = progHeaderOpt.Value;
        }

        return programHeaders;
    }

    public ElfSectionHeader[]? GetSectionHeaders(out ElfError error)
    {
        ElfHeader? headerOpt = GetElfHeader(out error);
        if (error != ElfError.Success || headerOpt == null)
        {
            return null;
        }

        ElfHeader header = headerOpt.Value;

        //it shouldn't be different, but you never know...
        if (header.SectionHeaderEntrySize != ElfSectionHeader.FILE_HEADER_SIZE)
        {
            error = ElfError.ElfFileHeaderCorrupted;
            return null;
        }

        ushort numEntries = header.SectionHeaderTableEntriesNum;
        int arraySizeBytes = numEntries * ElfSectionHeader.FILE_HEADER_SIZE;
        ElfSectionHeader[] sectionHeaders = new ElfSectionHeader[numEntries];

        ulong endPosition = header.SectionHeaderOffset + (ulong)arraySizeBytes;
        if (endPosition > (ulong)_stream.Length)
        {
            unsafe
            {
                CoreLibManager.PrimitiveDebug(
                    numEntries.ToString()
                        .ToCharPtrUnsafe());
            }

            error = ElfError.SizeExceeded;
            sectionHeaders.Dispose();
            return null;
        }

        _stream.Position = (long)header.SectionHeaderOffset;
        for (int i = 0; i < numEntries; i++)
        {
            ElfSectionHeader? sectionHeaderOpt = ElfSectionHeader.ParseHeader(_stream);
            if (sectionHeaderOpt == null)
            {
                error = ElfError.ElfSectionHeaderCorrupted;
                sectionHeaders.Dispose();
                return null;
            }

            sectionHeaders[i] = sectionHeaderOpt.Value;
        }

        return sectionHeaders;
    }

    public ElfError LoadExecutableSegment(ref ElfProgramHeader progHeader, SegmentMemoryAllocator allocatorFn)
    {
        if (progHeader.SegmentType == ElfSegmentType.Loadable)
        {
            return SegmentLoader.LoadExecutableSegment(ref progHeader, allocatorFn, _stream);
        }

        return ElfError.ElfSectionNotLoadable;
    }

    public ElfError LoadSection(ref ElfSectionHeader sectionHeader, SegmentMemoryAllocator allocatorFn)
    {
        if (sectionHeader.SectionHeaderType == ElfSectionType.NoBits)
        {
            return SectionLoader.LoadNoBitsSection(ref sectionHeader, allocatorFn, _stream);
        }

        return ElfError.ElfSectionNotLoadable;
    }

    /// <summary>
    /// Checks the ELF header.
    /// </summary>
    /// <returns>
    /// <see cref="ElfError.Success"/> if the ELF header is OK and the ELF type is supported, otherwise an error of
    /// type <see cref="ElfError"/>.
    /// </returns>
    public static ElfError CheckElfHeader(ref ElfHeader header)
    {
        if (
            header.MagicByte0 != ElfHeader.ELF_MAGIC0
            || header.MagicByte1 != ElfHeader.ELF_MAGIC1
            || header.MagicByte2 != ElfHeader.ELF_MAGIC2
            || header.MagicByte3 != ElfHeader.ELF_MAGIC3
            || header.Data != 1
            || header.Class != 2
            || header.HeaderVersion != 1
            || header.Version != 1
            || header.ThisSize != ElfHeader.FILE_HEADER_SIZE)
        {
            return ElfError.ElfFileHeaderCorrupted;
        }

#if ARCH_X64
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (header.Machine != ElfMachineType.X64)
        {
            return ElfError.ElfTypeNotSupported;
        }
#endif

        return ElfError.Success;
    }
}