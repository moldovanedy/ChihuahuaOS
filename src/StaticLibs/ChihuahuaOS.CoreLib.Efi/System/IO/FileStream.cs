#if UEFI
using System.Runtime.InteropServices;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.SimpleFsProtocol;
using Internal.Runtime.CompilerHelpers;

namespace System.IO;

public unsafe class FileStream : Stream
{
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
        get
        {
            ulong pos = 0;
            EfiStatus status = _efiFile->GetPosition(_efiFile, &pos);
            if (status != EfiStatus.Success)
            {
                return 0;
            }

            return (long)pos;
        }
        set => _efiFile->SetPosition(_efiFile, (ulong)value);
    }

    public EfiStatus LastError { get; private set; } = EfiStatus.Success;


    private readonly EfiFileProtocol* _efiFile;

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

        ulong bufferSize = (ulong)count;
        fixed (byte* bufferPtr = buffer)
        {
            LastError = _efiFile->Read(_efiFile, &bufferSize, bufferPtr + offset);
        }

        return (int)bufferSize;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        HardCheckRwArguments(buffer, offset, count);

        ulong bufferSize = (ulong)count;
        fixed (byte* bufferPtr = buffer)
        {
            LastError = _efiFile->Write(_efiFile, &bufferSize, bufferPtr + offset);
        }
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
        _efiFile->Flush(_efiFile);
    }

    public override void Close()
    {
        _efiFile->Close(_efiFile);
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