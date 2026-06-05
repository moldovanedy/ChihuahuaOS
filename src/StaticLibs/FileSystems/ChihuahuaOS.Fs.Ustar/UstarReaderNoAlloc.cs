using System;
using ChihuahuaOS.CoreLib.Extra;
using ChihuahuaOS.Fs.Ustar.Structures;

namespace ChihuahuaOS.Fs.Ustar;

/// <summary>
/// A reader specially made for the very early kernel, without any memory allocation. Useful for init-ramdisk.
/// </summary>
public readonly unsafe struct UstarReaderNoAlloc
{
    private readonly byte* _rawData;
    private readonly long _size;

    public UstarReaderNoAlloc(byte* rawData, long size)
    {
        _rawData = rawData;
        _size = size;
    }

    public byte* GetFilePointer(ReadOnlySpan<byte> fileName, out long fileSize)
    {
        fileSize = 0;
        if (_size < UstarHeader.HEADER_SIZE)
        {
            return null;
        }

        UstarHeader* headerPtr = (UstarHeader*)_rawData;
        while (RawMemory.MemCompare(headerPtr->UstarIdentifier, (byte*)"ustar\0"u8, 5) == 0)
        {
            //the file size is in octal null-terminated ASCII string, so just 11 characters are needed
            long thisFileSize = UstarHeader.GetOctalNumber(new ReadOnlySpan<byte>(headerPtr->FileSizeOctal, 11));
            if (
                RawMemory.MemCompare(
                    headerPtr->FileName,
                    (byte*)fileName,
                    (ulong)fileName.Length + 1)
                == 0)
            {
                fileSize = thisFileSize;
                return (byte*)headerPtr + UstarHeader.HEADER_SIZE;
            }

            //jump in segments of 512 bytes (the header size), as the memory is aligned as such
            headerPtr += (thisFileSize + (UstarHeader.HEADER_SIZE - 1)) / UstarHeader.HEADER_SIZE + 1;
            if ((byte*)headerPtr - _rawData + UstarHeader.HEADER_SIZE >= _size)
            {
                return null;
            }
        }

        return null;
    }
}
