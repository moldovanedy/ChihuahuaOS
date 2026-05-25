using System.Runtime.InteropServices;

namespace ChihuahuaOS.Kernel.FramebufferManager.Psf;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct PsfHeader
{
    public const uint MAGIC_VALUE = 0x72_B5_4A_86;


    public readonly uint Magic;
    public readonly uint Version;
    public readonly uint HeaderSize;
    public readonly PsfFlags Flags;
    public readonly uint NumGlyphs;
    public readonly uint BytesPerGlyph;
    public readonly uint Height;
    public readonly uint Width;


    public bool IsValid()
    {
        return Magic == MAGIC_VALUE && Version == 0 && (Flags == PsfFlags.None || Flags == PsfFlags.HasUnicodeTable);
    }
}