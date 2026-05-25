using ChihuahuaOS.CoreLib.Extra;
using Internal.Runtime.CompilerHelpers;

namespace System.IO;

public unsafe class MemoryStream : Stream
{
    public override bool CanRead => true;

    public override bool CanWrite => true;

    public override bool CanSeek => true;

    public override long Length { get; }

    public override long Position { get; set; }

    private readonly byte* _rawBuffer;

    public MemoryStream(byte[] buffer)
    {
        Length = buffer.Length;
        fixed (byte* ptr = buffer)
        {
            _rawBuffer = ptr;
        }
    }

    public MemoryStream(byte* rawBuffer, long length)
    {
        _rawBuffer = rawBuffer;
        Length = length;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        HardCheckRwArguments(buffer, offset, count);
        ulong size = (ulong)Math.Min(count, Length - Position);

        fixed (byte* ptr = buffer)
        {
            RawMemory.MemMove(_rawBuffer + Position, ptr + offset, size);
        }

        return (int)size;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        HardCheckRwArguments(buffer, offset, count);
        ulong size = (ulong)Math.Min(count, Length - Position);

        fixed (byte* ptr = buffer)
        {
            RawMemory.MemMove(ptr + offset, _rawBuffer + Position, size);
        }
    }

    public override int ReadRaw(byte* buffer, int offset, int count)
    {
        ulong size = (ulong)Math.Min(count, Length - Position);
        RawMemory.MemMove(_rawBuffer + Position, buffer + offset, size);
        return (int)size;
    }

    public override void WriteRaw(byte* buffer, int offset, int count)
    {
        ulong size = (ulong)Math.Min(count, Length - Position);
        RawMemory.MemMove(buffer + offset, _rawBuffer + Position, size);
    }

    public override byte ReadByte()
    {
        if (Position + 1 >= Length)
        {
            ThrowHelpers.ThrowIndexOutOfRangeException();
        }

        return _rawBuffer[Position++];
    }

    public override void WriteByte(byte data)
    {
        if (Position + 1 >= Length)
        {
            ThrowHelpers.ThrowIndexOutOfRangeException();
        }

        _rawBuffer[Position] = data;
        Position++;
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            default:
            case SeekOrigin.Begin:
                Position = Math.Max(0, Math.Min(Length - 1, offset));
                break;
            case SeekOrigin.Current:
                Position = Math.Max(0, Math.Min(Length - 1, Position + offset));
                break;
            case SeekOrigin.End:
                Position = Math.Max(0, Math.Min(Length - 1, Length - 1 - offset));
                break;
        }

        return Position;
    }


    private static void HardCheckRwArguments(byte[] buffer, int offset, int count)
    {
        if (offset < 0 || count < 0 || offset + count > buffer.Length)
        {
            ThrowHelpers.ThrowArgumentOutOfRangeException();
        }
    }
}