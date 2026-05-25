using System;
using System.Runtime.CompilerServices;

namespace ChihuahuaOS.CoreLib.Extra;

public static unsafe class RawMemory
{
    public static void MemSet(void* src, byte value, ulong length)
    {
        SpanHelpers.Fill(ref *(byte*)src, value, (nuint)length);
    }

    public static void MemSet(ref byte src, byte value, ulong length)
    {
        SpanHelpers.Fill(ref src, value, (nuint)length);
    }

    public static void MemMove(void* src, void* dest, ulong length)
    {
        SpanHelpers.Memmove(ref *(byte*)dest, ref *(byte*)src, (nuint)length);
    }

    public static void MemMove(ref byte src, ref byte dest, ulong length)
    {
        SpanHelpers.Memmove(ref dest, ref src, (nuint)length);
    }

    /// <summary>
    /// Compared to <see cref="MemMove(ref byte, ref byte, ulong)"/>, this is implemented as a simple for loop
    /// (might be slower since it's not intrinsic) and does not care about overlapping source and destination).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void MemCopy(void* src, void* dest, ulong length)
    {
        byte* byteSrc = (byte*)src;
        byte* byteDest = (byte*)dest;

        for (ulong i = 0; i < length; i++)
        {
            byteDest[i] = byteSrc[i];
        }
    }

    public static int MemCompare(byte* left, byte* right, ulong length)
    {
        for (ulong i = 0; i < length; i++)
        {
            if (left[i] == right[i])
            {
                continue;
            }

            if (left[i] < right[i])
            {
                return -1;
            }

            return 1;
        }

        return 0;
    }
}