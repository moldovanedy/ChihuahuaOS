namespace ChihuahuaOS.Fs.Ustar.Structures;

internal enum FileType : byte
{
    Normal = (byte)'0',
    HardLink = (byte)'1',
    SymLink = (byte)'2',
    CharacterDevice = (byte)'3',
    BlockDevice = (byte)'4',
    Directory = (byte)'5',
    Fifo = (byte)'6'
}