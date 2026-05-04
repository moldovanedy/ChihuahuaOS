using System.IO;
using System.Runtime.InteropServices;

namespace ChihuahuaOS.Elf.SectionHeader;

[StructLayout(LayoutKind.Sequential)]
public struct ElfSectionHeader
{
    public const int FILE_HEADER_SIZE = 0x40;


    /// <summary>
    /// The index of the string (in the ".shstrtab" section) that represents this section.
    /// </summary>
    public uint SectionNameIndex;

    /// <summary>
    /// Section header type.
    /// </summary>
    public ElfSectionType SectionHeaderType;

    /// <summary>
    /// Section attribute flags.
    /// </summary>
    public ElfSectionFlags Flags;

    /// <summary>
    /// The virtual address of this section.
    /// </summary>
    public ulong VirtualAddress;

    /// <summary>
    /// Offset (from the beginning of the file) of this section.
    /// </summary>
    public ulong OffsetInFile;

    /// <summary>
    /// The size in bytes of this section; might be 0.
    /// </summary>
    public ulong SectionSize;

    public uint LinkInfo;
    public uint Info;

    /// <summary>
    /// Must be a power of two.
    /// </summary>
    public ulong AddressAlignment;

    /// <summary>
    /// The size in bytes of each entry for sections that contain fixed-size entries; otherwise it's 0.
    /// </summary>
    public ulong EntryFixedSize;


    public static ElfSectionHeader? ParseHeader(Stream stream)
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
            ElfSectionHeader header;
            unsafe
            {
                fixed (byte* ptr = buffer)
                {
                    header = *(ElfSectionHeader*)ptr;
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