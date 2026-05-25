namespace ChihuahuaOS.Kernel.FramebufferManager.Psf;

public readonly unsafe struct PsfHandler
{
    public readonly PsfHeader Header;

    private readonly byte* _filePtr;
    private readonly long _fileLength;

    public PsfHandler(byte* filePtr, long length)
    {
        _filePtr = filePtr;
        _fileLength = length;

        Header = *(PsfHeader*)_filePtr;
    }

    public bool IsValid()
    {
        return Header.IsValid();
    }

    public byte* GetCharacterDataNoUnicode(int codePoint)
    {
        long offset = Header.HeaderSize + codePoint * Header.BytesPerGlyph;
        if (offset >= _fileLength)
        {
            return null;
        }

        return _filePtr + offset;
    }
}
