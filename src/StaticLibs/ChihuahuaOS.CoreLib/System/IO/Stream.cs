using Internal.Runtime.CompilerHelpers;

namespace System.IO;

public abstract class Stream : IDisposable
{
    public static readonly Stream Null = new NullStream();

    public abstract bool CanRead { get; }
    public abstract bool CanWrite { get; }
    public abstract bool CanSeek { get; }
    public virtual bool CanTimeout => false;

    public abstract long Length { get; }
    public abstract long Position { get; set; }

    public virtual int ReadTimeout
    {
        get
        {
            ThrowHelpers.ThrowInvalidOperationException();
            return 0;
        }
        set
        {
            ThrowHelpers.ThrowInvalidOperationException();
            _ = value;
        }
    }

    public virtual int WriteTimeout
    {
        get
        {
            ThrowHelpers.ThrowInvalidOperationException();
            return 0;
        }
        set
        {
            ThrowHelpers.ThrowInvalidOperationException();
            _ = value;
        }
    }

    public void CopyTo(Stream destination)
    {
        // ReSharper disable once IntroduceOptionalParameters.Global
        CopyTo(destination, 8192);
    }

    public virtual void CopyTo(Stream destination, int bufferSize)
    {
        if (bufferSize <= 0)
        {
            ThrowHelpers.ThrowArgumentException();
        }

        if (!CanRead || !destination.CanWrite)
        {
            ThrowHelpers.ThrowStreamException();
        }

        byte[] buffer = new byte[bufferSize];
        int bytesRead;
        while ((bytesRead = Read(buffer, 0, buffer.Length)) != 0)
        {
            destination.Write(buffer, 0, bytesRead);
        }

        buffer.Dispose();
    }

    public abstract int Read(byte[] buffer, int offset, int count);

    public abstract void Write(byte[] buffer, int offset, int count);

    public abstract byte ReadByte();

    public abstract void WriteByte(byte data);

    public abstract void Flush();

    public virtual void Close()
    {
    }

    public void Dispose()
    {
        Close();
    }
}