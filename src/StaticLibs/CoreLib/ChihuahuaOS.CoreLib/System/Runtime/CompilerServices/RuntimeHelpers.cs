using Internal.Runtime;

namespace System.Runtime.CompilerServices;

public class RuntimeHelpers
{
    public static unsafe int OffsetToStringData => sizeof(nint) + sizeof(int);

    internal static unsafe MethodTable* GetMethodTable(object obj)
    {
        return obj.m_pEEType;
    }

    [Intrinsic]
    public static extern void InitializeArray(Array array, RuntimeFieldHandle fldHandle);
}