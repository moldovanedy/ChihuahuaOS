namespace System.IO;

internal class NullStream : Stream
{
    public override bool CanRead => true;
    public override bool CanWrite => true;
    public override bool CanSeek => true;
    public override long Length => 0;

    public override long Position
    {
        get => 0;
        set { }
    }

    public override void CopyTo(Stream destination, int bufferSize)
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return 0;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
    }

    public override byte ReadByte()
    {
        return 0;
    }

    public override void WriteByte(byte data)
    {
    }

    public override void Flush()
    {
    }
}