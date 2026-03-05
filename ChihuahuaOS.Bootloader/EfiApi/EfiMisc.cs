using System;
using System.Runtime.InteropServices;

namespace ChihuahuaOS.Bootloader.EfiApi;

[StructLayout(LayoutKind.Sequential)]
public struct EfiHandle
{
    private IntPtr _handle;

    internal const ulong EFI_ERROR_MASK = 0x8000000000000000;
}

[StructLayout(LayoutKind.Sequential)]
public struct EfiEvent
{
    private IntPtr _handle;
}

public enum EfiStatus : ulong
{
    Success = 0,

    WarnUnknownGlyph = 1,
    WarnDeleteFailure = 2,
    WarnWriteFailure = 3,
    WarnBufferTooSmall = 4,
    WarnStaleData = 5,
    WarnFileSystem = 6,
    WarnResetRequired = 7,

    LoadError = EfiErrorMask | 1,
    InvalidParameter = EfiErrorMask | 2,
    Unsupported = EfiErrorMask | 3,
    BadBufferSize = EfiErrorMask | 4,
    BufferTooSmall = EfiErrorMask | 5,
    NotReady = EfiErrorMask | 6,
    DeviceError = EfiErrorMask | 7,
    WriteProtected = EfiErrorMask | 8,
    OutOfResources = EfiErrorMask | 9,
    VolumeCorrupted = EfiErrorMask | 10,
    VolumeFull = EfiErrorMask | 11,
    NoMedia = EfiErrorMask | 12,
    MediaChanged = EfiErrorMask | 13,
    NotFound = EfiErrorMask | 14,
    AccessDenied = EfiErrorMask | 15,
    NoResponse = EfiErrorMask | 16,
    NoMapping = EfiErrorMask | 17,
    Timeout = EfiErrorMask | 18,
    NotStarted = EfiErrorMask | 19,
    AlreadyStarted = EfiErrorMask | 20,
    Aborted = EfiErrorMask | 21,
    IcmpError = EfiErrorMask | 22,
    TftpError = EfiErrorMask | 23,
    ProtocolError = EfiErrorMask | 24,
    IncompatibleVersion = EfiErrorMask | 25,
    SecurityViolation = EfiErrorMask | 26,
    CrcError = EfiErrorMask | 27,
    EndOfMedia = EfiErrorMask | 28,
    EndOfFile = EfiErrorMask | 31,
    InvalidLanguage = EfiErrorMask | 32,
    CompromisedData = EfiErrorMask | 33,
    IpAddressConflict = EfiErrorMask | 34,
    HttpError = EfiErrorMask | 35,

    EfiErrorMask = 0x8000000000000000
}