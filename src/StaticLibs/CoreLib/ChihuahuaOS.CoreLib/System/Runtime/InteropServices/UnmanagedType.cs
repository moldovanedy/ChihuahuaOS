namespace System.Runtime.InteropServices;

public enum UnmanagedType
{
    Bool = 0x2, // 4 byte boolean value (true != 0, false == 0)
    I1 = 0x3, // 1 byte signed value
    U1 = 0x4, // 1 byte unsigned value
    I2 = 0x5, // 2 byte signed value
    U2 = 0x6, // 2 byte unsigned value
    I4 = 0x7, // 4 byte signed value
    U4 = 0x8, // 4 byte unsigned value
    I8 = 0x9, // 8 byte signed value
    U8 = 0xa, // 8 byte unsigned value
    R4 = 0xb, // 4 byte floating point
    R8 = 0xc, // 8 byte floating point

    // [EditorBrowsable(EditorBrowsableState.Never)]
    Struct = 0x1b, // Structure
    SysInt = 0x1f, // Hardware natural sized signed integer
    SysUInt = 0x20,
    FunctionPtr = 0x26 // Function pointer
}