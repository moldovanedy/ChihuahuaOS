using System.IO;
using System.Runtime.InteropServices;

namespace ChihuahuaOS.Elf.FileHeader;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct ElfHeader
{
    public const int ELF_IDENTIFIER_SIZE = 16;
    public const int FILE_HEADER_SIZE = 0x40;

    public const byte ELF_MAGIC0 = 0x7F;
    public const byte ELF_MAGIC1 = (byte)'E';
    public const byte ELF_MAGIC2 = (byte)'L';
    public const byte ELF_MAGIC3 = (byte)'F';


    public readonly byte MagicByte0;

    public readonly byte MagicByte1;

    public readonly byte MagicByte2;

    public readonly byte MagicByte3;

    public readonly byte Class;

    public readonly byte Data;

    public readonly byte HeaderVersion;

    public readonly byte OsAbi;

    public readonly byte AbiVersion;

    public readonly int Padding1;

    public readonly short Padding2;

    public readonly byte Padding3;


    /// <summary>
    /// File type; see <see cref="ElfAppType"/> for more info.
    /// </summary>
    public readonly ElfAppType Type;

    /// <summary>
    /// Target ISA; see <see cref="ElfMachineType"/> for more info.
    /// </summary>
    public readonly ElfMachineType Machine;

    /// <summary>
    /// Should always be 1.
    /// </summary>
    public readonly uint Version;

    /// <summary>
    /// Program entry point. 0 if not an executable.
    /// </summary>
    public readonly ulong EntryPoint;

    /// <summary>
    /// The offset (from the file start) of the program header table. Generally 0x40.
    /// </summary>
    public readonly ulong ProgHeaderOffset;

    /// <summary>
    /// The offset (from the file start) of the section header table.
    /// </summary>
    public readonly ulong SectionHeaderOffset;

    /// <summary>
    /// Architecture-specific flags.
    /// </summary>
    public readonly uint Flags;

    /// <summary>
    /// The size of this header. Should always be 64.
    /// </summary>
    public readonly ushort ThisSize;

    /// <summary>
    /// The size of an entry in the program header table.
    /// </summary>
    public readonly ushort ProgHeaderEntrySize;

    /// <summary>
    /// The number of program header table entries.
    /// </summary>
    public readonly ushort ProgHeaderTableEntriesNum;

    /// <summary>
    /// The size of an entry in the section header table.
    /// </summary>
    public readonly ushort SectionHeaderEntrySize;

    /// <summary>
    /// The number of section header table entries.
    /// </summary>
    public readonly ushort SectionHeaderTableEntriesNum;

    /// <summary>
    /// The index of the entry in the section header table that contains the actual section names.
    /// </summary>
    public readonly ushort SectionNamesEntryIndex;


    public static ElfHeader? ParseHeader(Stream stream)
    {
        byte[] buffer = new byte[FILE_HEADER_SIZE];

        try
        {
            int numBytesRead = stream.Read(buffer, 0, FILE_HEADER_SIZE);
            if (numBytesRead != FILE_HEADER_SIZE)
            {
                return null;
            }

            //we don't care about endianness here, since the caller will check the endianness from ElfHeader.Identifiers
            // and immediately stop parsing the data (it can't work on a different endianness)
            ElfHeader header;
            unsafe
            {
                fixed (byte* ptr = buffer)
                {
                    header = *(ElfHeader*)ptr;
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