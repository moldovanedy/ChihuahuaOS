using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices;

public class RuntimeHelpers
{
    public static unsafe int OffsetToStringData => sizeof(nint) + sizeof(int);

    public static unsafe MethodTable* GetMethodTable(object obj)
    {
        return obj.m_pMethodTable;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal class RawArrayData
{
    public uint Length;

#if X64 || ARM64
    public uint Padding;
#elif X86 || ARM
        // No padding on 32bit
#else
#endif

    public byte Data;
}