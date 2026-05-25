using System;
using System.Runtime.InteropServices;

namespace ChihuahuaOS.Fs.Ustar.Structures;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct UstarHeader
{
    public const int HEADER_SIZE = 512;


    public fixed byte FileName[100];

    public fixed byte FileMode[8];

    public fixed byte Uid[8];

    public fixed byte Gid[8];

    public fixed byte FileSizeOctal[12];

    public fixed byte LastModifiedTimeOctal[12];

    public fixed byte Checksum[8];

    public FileType Type;

    public fixed byte LinkedFileName[100];

    public fixed byte UstarIdentifier[6];

    public fixed byte UstarVersion[2];

    public fixed byte OwnerUserName[32];

    public fixed byte OwnerGroupName[32];

    public fixed byte DeviceMajorNumber[8];

    public fixed byte DeviceMinorNumber[8];

    public fixed byte FileNamePrefix[155];

    public fixed byte Padding[12];


    public static long GetOctalNumber(ReadOnlySpan<byte> buffer)
    {
        long n = 0;
        int size = buffer.Length;

        while (size > 0)
        {
            n *= 8;
            n += buffer[buffer.Length - size] - '0';
            size--;
        }

        return n;
    }
}