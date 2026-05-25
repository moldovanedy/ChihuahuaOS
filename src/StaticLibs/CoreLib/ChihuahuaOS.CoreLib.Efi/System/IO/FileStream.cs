#if UEFI
using System.Runtime.InteropServices;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.SimpleFsProtocol;
using Internal.Runtime.CompilerHelpers;

namespace System.IO;

public unsafe class FileStream : Stream
{
    private const int BUFFER_SIZE = 4096 * 2;

    public override bool CanRead => true;
    public override bool CanWrite => true;
    public override bool CanSeek => true;

    public override long Length
    {
        get
        {
            if (field == -1)
            {
                EfiGuid fileInfoIdGuid = AllEfiGuids.EfiFileInfoId;
                ulong bufSize = 0;

                //first, make a call without a buffer to get the buffer size, then do the actual call
                EfiStatus status = _efiFile->GetInfo(_efiFile, &fileInfoIdGuid, &bufSize, null);
                if (status != EfiStatus.BufferTooSmall)
                {
                    field = 0;
                    return 0;
                }

                EfiFileInfo* rawBuffer = (EfiFileInfo*)NativeMemory.AllocZeroed((nuint)bufSize);
                status = _efiFile->GetInfo(_efiFile, &fileInfoIdGuid, &bufSize, rawBuffer);
                if (status != EfiStatus.Success || rawBuffer == null)
                {
                    field = 0;
                    return 0;
                }

                field = (long)rawBuffer->FileSize;
                NativeMemory.Free(rawBuffer);
            }

            return field;
        }
    } = -1;

    public override long Position
    {
        get =>
            // ulong pos = 0;
            // EfiStatus status = _efiFile->GetPosition(_efiFile, &pos);
            // if (status != EfiStatus.Success)
            // {
            //     return 0;
            // }
            //
            // return (long)pos;
            _position;
        set
        {
            Flush();

            _internalBufferPosition = 0;
            _internalBufferSize = 0;
            _efiFile->SetPosition(_efiFile, (ulong)value);
            _position = value;
        }
    }

    public EfiStatus LastError { get; private set; } = EfiStatus.Success;


    private readonly EfiFileProtocol* _efiFile;

    private readonly byte[] _internalBuffer = new byte[BUFFER_SIZE];
    private int _internalBufferPosition = -1;
    private uint _internalBufferSize;
    private long _position;
    private long _lastEfiPosition;
    private bool _wasWritingLastTime;

    internal FileStream(EfiFileProtocol* efiFile)
    {
        if (efiFile == null)
        {
            ThrowHelpers.ThrowArgumentException();
        }

        _efiFile = efiFile;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        HardCheckRwArguments(buffer, offset, count);

        if (_wasWritingLastTime)
        {
            Flush();
            _wasWritingLastTime = false;
        }

        int totalBytesRead = 0;
        while (count > 0)
        {
            if (_internalBufferSize == 0 || _internalBufferPosition >= _internalBufferSize)
            {
                ulong bufferSize = BUFFER_SIZE;
                fixed (byte* bufferPtr = _internalBuffer)
                {
                    LastError = _efiFile->Read(_efiFile, &bufferSize, bufferPtr);
                }

                if (LastError != EfiStatus.Success)
                {
                    _internalBufferSize = 0;
                    _internalBufferPosition = 0;
                    return totalBytesRead;
                }

                _internalBufferSize = (uint)bufferSize;
                _internalBufferPosition = 0;

                //the file is now completed, return
                if (_internalBufferSize == 0)
                {
                    return totalBytesRead;
                }
            }

            int copyLength = (int)Math.Min(count, _internalBufferSize - _internalBufferPosition);
            for (int i = 0; i < copyLength; i++)
            {
                buffer[offset + i] = _internalBuffer[_internalBufferPosition + i];
            }

            count -= copyLength;
            offset += copyLength;
            totalBytesRead += copyLength;
            _position += copyLength;
            _internalBufferPosition += copyLength;
        }

        return totalBytesRead;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        HardCheckRwArguments(buffer, offset, count);

        if (_internalBufferSize == 0 || !_wasWritingLastTime)
        {
            _internalBufferSize = BUFFER_SIZE;
            _internalBufferPosition = 0;
            //reset
            Position = _position;
        }

        _wasWritingLastTime = true;
        while (count > 0)
        {
            if (_internalBufferPosition >= _internalBufferSize)
            {
                ulong bufferSize = _internalBufferSize;
                fixed (byte* bufferPtr = _internalBuffer)
                {
                    LastError = _efiFile->Write(_efiFile, &bufferSize, bufferPtr);
                }

                UpdateEfiPos();
                if (LastError != EfiStatus.Success)
                {
                    _internalBufferSize = 0;
                    _internalBufferPosition = 0;
                    return;
                }

                _internalBufferSize = BUFFER_SIZE;
                _internalBufferPosition = 0;
            }

            int copyLength = (int)Math.Min(count, _internalBufferSize - _internalBufferPosition);
            Array.Copy(
                buffer,
                offset,
                _internalBuffer,
                _internalBufferPosition,
                copyLength);

            // for (int i = 0; i < copyLength; i++)
            // {
            //     _internalBuffer[_internalBufferPosition + i] = buffer[offset + i];
            // }

            count -= copyLength;
            offset += copyLength;
            _position += copyLength;
            _internalBufferPosition += copyLength;
        }
    }

    public override int ReadRaw(byte* buffer, int offset, int count)
    {
        _internalBufferSize = 0;
        _internalBufferPosition = 0;

        ulong bufferSize = (ulong)count;
        LastError = _efiFile->Read(_efiFile, &bufferSize, buffer + offset);
        UpdateEfiPos();

        if (LastError != EfiStatus.Success)
        {
            return 0;
        }

        return (int)bufferSize;
    }

    public override void WriteRaw(byte* buffer, int offset, int count)
    {
        _internalBufferSize = 0;
        _internalBufferPosition = 0;
        _wasWritingLastTime = true;

        ulong bufferSize = (ulong)count;
        LastError = _efiFile->Write(_efiFile, &bufferSize, buffer + offset);
        UpdateEfiPos();
    }

    public override byte ReadByte()
    {
        byte[] buffer = new byte[sizeof(byte)];
        int bytesRead = Read(buffer, 0, buffer.Length);
        if (bytesRead == 0)
        {
            buffer.Dispose();
            return 0;
        }

        byte value = buffer[0];
        buffer.Dispose();
        return value;
    }

    public override void WriteByte(byte data)
    {
        byte[] buffer = new byte[sizeof(byte)];
        buffer[0] = data;
        Write(buffer, 0, buffer.Length);
        buffer.Dispose();
    }

    public override void Flush()
    {
        if (_wasWritingLastTime)
        {
            long pos = Position;
            _efiFile->SetPosition(_efiFile, (ulong)_lastEfiPosition);

            //position, not size, dictates the number of bytes to write
            ulong bufferSize = (ulong)_internalBufferPosition;
            fixed (byte* bufferPtr = _internalBuffer)
            {
                LastError = _efiFile->Write(_efiFile, &bufferSize, bufferPtr);
            }

            _efiFile->SetPosition(_efiFile, (ulong)pos);
            _internalBufferPosition = 0;
            _internalBufferSize = 0;
        }

        _efiFile->Flush(_efiFile);
    }
    
    public override long Seek(long offset, SeekOrigin origin)
    {
        Flush();
        
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

        _efiFile->SetPosition(_efiFile, (ulong)Position);
        return Position;
    }

    public override void Close()
    {
        Flush();
        _internalBuffer.Dispose();
        _efiFile->Close(_efiFile);
    }


    private void UpdateEfiPos()
    {
        ulong pos = 0;
        EfiStatus status = _efiFile->GetPosition(_efiFile, &pos);
        if (status != EfiStatus.Success)
        {
            return;
        }

        _lastEfiPosition = (long)pos;
    }

    private static void HardCheckRwArguments(byte[] buffer, int offset, int count)
    {
        if (offset < 0 || count < 0 || offset + count > buffer.Length)
        {
            ThrowHelpers.ThrowArgumentOutOfRangeException();
        }
    }
}

#endif