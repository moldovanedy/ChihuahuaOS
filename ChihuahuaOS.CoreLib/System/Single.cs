using System.Runtime.CompilerServices;

namespace System;

public struct Single
{
    public const float MinValue = (float)-3.40282346638528859e+38;
    public const float MaxValue = (float)3.40282346638528859e+38;
    public const float Epsilon = (float)1.4e-45;
    public const float NegativeInfinity = (float)-1.0 / (float)0.0;
    public const float PositiveInfinity = (float)1.0 / (float)0.0;
    public const float NaN = (float)0.0 / (float)0.0;

    internal const uint SignMask = 0x8000_0000;
    internal const uint PositiveZeroBits = 0x0000_0000;
    internal const uint NegativeZeroBits = 0x8000_0000;
    internal const uint MinTrailingSignificand = 0x0000_0000;
    internal const uint MaxTrailingSignificand = 0x007F_FFFF;
    internal const uint PositiveInfinityBits = 0x7F80_0000;
    internal const uint NegativeInfinityBits = 0xFF80_0000;
    internal const uint SmallestNormalBits = 0x0080_0000;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFinite(float f)
    {
        uint bits = BitConverter.SingleToUInt32Bits(f);
        return (~bits & PositiveInfinityBits) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInfinity(float f)
    {
        uint bits = BitConverter.SingleToUInt32Bits(Math.Abs(f));
        return bits == PositiveInfinityBits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNaN(float f)
    {
        // A NaN will never equal itself so this is an
        // easy and efficient way to check for NaN.

#pragma warning disable CS1718
        // ReSharper disable once EqualExpressionComparison
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return f != f;
#pragma warning restore CS1718
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsNaNOrZero(float f)
    {
        uint bits = BitConverter.SingleToUInt32Bits(f);
        return ((bits - 1) & ~SignMask) >= PositiveInfinityBits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNegative(float f)
    {
        return BitConverter.SingleToInt32Bits(f) < 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNegativeInfinity(float f)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return f == NegativeInfinity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNormal(float f)
    {
        uint bits = BitConverter.SingleToUInt32Bits(Math.Abs(f));
        return bits - SmallestNormalBits < PositiveInfinityBits - SmallestNormalBits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPositiveInfinity(float f)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return f == PositiveInfinity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSubnormal(float f)
    {
        uint bits = BitConverter.SingleToUInt32Bits(Math.Abs(f));
        return bits - 1 < MaxTrailingSignificand;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsZero(float f)
    {
        return f == 0;
    }
}