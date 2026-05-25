using System.Runtime.CompilerServices;

namespace System;

public struct Double
{
    public const double MinValue = -1.7976931348623157E+308;
    public const double MaxValue = 1.7976931348623157E+308;
    public const double Epsilon = 4.9406564584124654E-324;
    public const double NegativeInfinity = -1.0 / 0.0;
    public const double PositiveInfinity = 1.0 / 0.0;
    public const double NaN = 0.0 / 0.0;

    internal const ulong SignMask = 0x8000_0000_0000_0000;
    internal const ulong PositiveZeroBits = 0x0000_0000_0000_0000;
    internal const ulong NegativeZeroBits = 0x8000_0000_0000_0000;
    internal const ulong EpsilonBits = 0x0000_0000_0000_0001;
    internal const ulong PositiveInfinityBits = 0x7FF0_0000_0000_0000;
    internal const ulong NegativeInfinityBits = 0xFFF0_0000_0000_0000;
    internal const ulong SmallestNormalBits = 0x0010_0000_0000_0000;
    internal const ulong MinTrailingSignificand = 0x0000_0000_0000_0000;
    internal const ulong MaxTrailingSignificand = 0x000F_FFFF_FFFF_FFFF;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFinite(double d)
    {
        ulong bits = BitConverter.DoubleToUInt64Bits(d);
        return (~bits & PositiveInfinityBits) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInfinity(double d)
    {
        ulong bits = BitConverter.DoubleToUInt64Bits(Math.Abs(d));
        return bits == PositiveInfinityBits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNaN(double d)
    {
        // A NaN will never equal itself so this is an
        // easy and efficient way to check for NaN.

#pragma warning disable CS1718
        // ReSharper disable once EqualExpressionComparison
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return d != d;
#pragma warning restore CS1718
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsNaNOrZero(double d)
    {
        ulong bits = BitConverter.DoubleToUInt64Bits(d);
        return ((bits - 1) & ~SignMask) >= PositiveInfinityBits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNegative(double d)
    {
        return BitConverter.DoubleToInt64Bits(d) < 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNegativeInfinity(double d)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return d == NegativeInfinity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNormal(double d)
    {
        ulong bits = BitConverter.DoubleToUInt64Bits(Math.Abs(d));
        return bits - SmallestNormalBits < PositiveInfinityBits - SmallestNormalBits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPositiveInfinity(double d)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return d == PositiveInfinity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSubnormal(double d)
    {
        ulong bits = BitConverter.DoubleToUInt64Bits(Math.Abs(d));
        return bits - 1 < MaxTrailingSignificand;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsZero(double d)
    {
        return d == 0;
    }
}