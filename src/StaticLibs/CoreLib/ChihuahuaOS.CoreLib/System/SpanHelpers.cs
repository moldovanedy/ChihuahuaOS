using System.Runtime;
using System.Runtime.CompilerServices;

namespace System;

internal static class SpanHelpers
{
    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ClearWithoutReferences(ref byte dest, nuint len)
    {
        Fill(ref dest, 0, len);
    }

    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    /// <summary>
    /// This only works for unmanaged types T.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="source"></param>
    /// <param name="elementCount"></param>
    /// <typeparam name="T">An unmanaged, blittable data type.</typeparam>
    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe void Memmove<T>(ref T destination, ref T source, nuint elementCount) where T : unmanaged
    {
        // Blittable memmove
        Memmove(
            ref Unsafe.As<T, byte>(ref destination),
            ref Unsafe.As<T, byte>(ref source),
            elementCount * (nuint)sizeof(T));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Fill(ref byte dest, byte value, nuint len)
    {
        for (nuint i = 0; i < len; i++)
        {
            Unsafe.Add(ref dest, (nint)i) = value;
        }
    }


    //NOTE: these are not in the source, but I couldn't find any implementation in the dotnet repo, and the linker needs
    // those functions (MemCopy and MemZero)

    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RuntimeExport("RhSpanHelpers_MemCopy")]
    internal static void MemCopy(ref byte dest, ref byte src, nuint len)
    {
        for (nuint i = len; i > 0; i--)
        {
            Unsafe.Add(ref dest, (nint)(i - 1)) = Unsafe.Add(ref src, (nint)(i - 1));
        }
    }

    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RuntimeExport("RhSpanHelpers_MemZero")]
    internal static void MemZero(ref byte dest, nuint len)
    {
        Fill(ref dest, 0, len);
    }
}
