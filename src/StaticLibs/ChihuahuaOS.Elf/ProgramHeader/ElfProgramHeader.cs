using System.IO;
using System.Runtime.InteropServices;

namespace ChihuahuaOS.Elf.ProgramHeader;

[StructLayout(LayoutKind.Sequential)]
public struct ElfProgramHeader
{
    public const int FILE_HEADER_SIZE = 0x38;

    /// <summary>
    /// Segment type; see Elf_ProgramType for more info.
    /// </summary>
    public ElfSegmentType SegmentType;

    /// <summary>
    /// Segment-dependent flags; see Elf_SegmentFlags for more info.
    /// </summary>
    public ElfSegmentFlags Flags;

    /// <summary>
    /// Offset (from the beginning of the file) of the segment in the file.
    /// </summary>
    public ulong Offset;

    /// <summary>
    /// The segment's virtual address.
    /// </summary>
    public ulong VirtualAddress;

    /// <summary>
    /// The segment's physical address; should either be 0 or equal to VirtualAddress.
    /// </summary>
    public ulong PhysicalAddress;

    /// <summary>
    /// The segment size in the file; might be 0.
    /// </summary>
    public ulong SizeInFile;

    /// <summary>
    /// The segment size in memory; might be 0.
    /// </summary>
    public ulong SizeInMemory;

    /// <summary>
    /// 0 and 1 specify no alignment. Otherwise, a power of two, where VirtualAddress = Offset % Alignment.
    /// </summary>
    public ulong Alignment;


    public static ElfProgramHeader? ParseHeader(Stream stream)
    {
        byte[] buffer = new byte[FILE_HEADER_SIZE];

        try
        {
            int numBytesRead = stream.Read(buffer, 0, FILE_HEADER_SIZE);
            if (numBytesRead != FILE_HEADER_SIZE)
            {
                return null;
            }

            //we don't care about endianness here, since the app can only be loaded if it's in the native endianness
            ElfProgramHeader header;
            unsafe
            {
                fixed (byte* ptr = buffer)
                {
                    header = *(ElfProgramHeader*)ptr;
                }
            }

            return header;
        }
        finally
        {
            buffer.Dispose();
        }
    }
}