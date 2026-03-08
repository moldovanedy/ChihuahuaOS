using System.Runtime.CompilerServices;

namespace System;

internal static class SpanHelpers
{
    [Intrinsic]
    public static void ClearWithoutReferences(ref byte dest, nuint len)
    {
        Fill(ref dest, 0, len);
    }

    [Intrinsic]
    internal static void Memmove(ref byte dest, ref byte src, nuint len)
    {
        if ((nuint)Unsafe.ByteOffset(ref src, ref dest) >= len)
        {
            for (nuint i = 0; i < len; i++)
            {
                Unsafe.Add(ref dest, (nint)i) = Unsafe.Add(ref src, (nint)i);
            }
        }
        else
        {
            for (nuint i = len; i > 0; i--)
            {
                Unsafe.Add(ref dest, (nint)(i - 1)) = Unsafe.Add(ref src, (nint)(i - 1));
            }
        }
    }

    internal static void Fill(ref byte dest, byte value, nuint len)
    {
        for (nuint i = 0; i < len; i++)
        {
            Unsafe.Add(ref dest, (nint)i) = value;
        }
    }
}