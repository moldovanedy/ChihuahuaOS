using System.Runtime;
using System.Runtime.CompilerServices;
using Internal.Runtime.CompilerHelpers;

namespace System;

public static class Math
{
    public const double E = 2.7182818284590452354;

    public const double PI = 3.14159265358979323846;

    public const double Tau = 6.283185307179586476925;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short Abs(short value)
    {
        if (value < 0)
        {
            value = (short)-value;
            if (value < 0)
            {
                ThrowHelpers.ThrowOverflowException();
            }
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Abs(int value)
    {
        if (value < 0)
        {
            value = -value;
            if (value < 0)
            {
                ThrowHelpers.ThrowOverflowException();
            }
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Abs(long value)
    {
        if (value < 0)
        {
            value = -value;
            if (value < 0)
            {
                ThrowHelpers.ThrowOverflowException();
            }
        }

        return value;
    }

    /// <summary>Returns the absolute value of a native signed integer.</summary>
    /// <param name="value">A number that is greater than <see cref="IntPtr.MinValue" />, but less than or equal to <see cref="IntPtr.MaxValue" />.</param>
    /// <returns>A native signed integer, x, such that 0 \u2264 x \u2264 <see cref="IntPtr.MaxValue" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint Abs(nint value)
    {
        if (value < 0)
        {
            value = -value;
            if (value < 0)
            {
                ThrowHelpers.ThrowOverflowException();
            }
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte Abs(sbyte value)
    {
        if (value < 0)
        {
            value = (sbyte)-value;
            if (value < 0)
            {
                ThrowHelpers.ThrowOverflowException();
            }
        }

        return value;
    }

    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Abs(double value)
    {
        const ulong MASK = 0x7FFFFFFFFFFFFFFF;
        ulong raw = BitConverter.DoubleToUInt64Bits(value);

        return BitConverter.UInt64BitsToDouble(raw & MASK);
    }

    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Abs(float value)
    {
        const uint MASK = 0x7FFFFFFF;
        uint raw = BitConverter.SingleToUInt32Bits(value);
        return BitConverter.UInt32BitsToSingle(raw & MASK);
    }

    [Intrinsic]
    public static byte Max(byte val1, byte val2)
    {
        return val1 >= val2 ? val1 : val2;
    }

    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Max(double val1, double val2)
    {
        // This matches the IEEE 754:2019 `maximum` function
        //
        // It propagates NaN inputs back to the caller and
        // otherwise returns the greater of the inputs. It
        // treats +0 as greater than -0 as per the specification.

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (val1 != val2)
        {
            if (!double.IsNaN(val1))
            {
                return val2 < val1 ? val1 : val2;
            }

            return val1;
        }

        return double.IsNegative(val2) ? val1 : val2;
    }

    [Intrinsic]
    public static short Max(short val1, short val2)
    {
        return val1 >= val2 ? val1 : val2;
    }

    [Intrinsic]
    public static int Max(int val1, int val2)
    {
        return val1 >= val2 ? val1 : val2;
    }

    [Intrinsic]
    public static long Max(long val1, long val2)
    {
        return val1 >= val2 ? val1 : val2;
    }

    [Intrinsic]
    public static nint Max(nint val1, nint val2)
    {
        return val1 >= val2 ? val1 : val2;
    }

    [Intrinsic]
    public static sbyte Max(sbyte val1, sbyte val2)
    {
        return val1 >= val2 ? val1 : val2;
    }

    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Max(float val1, float val2)
    {
        // This matches the IEEE 754:2019 `maximum` function
        //
        // It propagates NaN inputs back to the caller and
        // otherwise returns the greater of the inputs. It
        // treats +0 as greater than -0 as per the specification.

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (val1 != val2)
        {
            if (!float.IsNaN(val1))
            {
                return val2 < val1 ? val1 : val2;
            }

            return val1;
        }

        return float.IsNegative(val2) ? val1 : val2;
    }

    [Intrinsic]
    public static ushort Max(ushort val1, ushort val2)
    {
        return val1 >= val2 ? val1 : val2;
    }

    [Intrinsic]
    public static uint Max(uint val1, uint val2)
    {
        return val1 >= val2 ? val1 : val2;
    }

    [Intrinsic]
    public static ulong Max(ulong val1, ulong val2)
    {
        return val1 >= val2 ? val1 : val2;
    }

    [Intrinsic]
    public static nuint Max(nuint val1, nuint val2)
    {
        return val1 >= val2 ? val1 : val2;
    }

    [Intrinsic]
    public static byte Min(byte val1, byte val2)
    {
        return val1 <= val2 ? val1 : val2;
    }

    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Min(double val1, double val2)
    {
        // This matches the IEEE 754:2019 `minimum` function
        //
        // It propagates NaN inputs back to the caller and
        // otherwise returns the lesser of the inputs. It
        // treats +0 as greater than -0 as per the specification.

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (val1 != val2)
        {
            if (!double.IsNaN(val1))
            {
                return val1 < val2 ? val1 : val2;
            }

            return val1;
        }

        return double.IsNegative(val1) ? val1 : val2;
    }

    [Intrinsic]
    public static short Min(short val1, short val2)
    {
        return val1 <= val2 ? val1 : val2;
    }

    [Intrinsic]
    public static int Min(int val1, int val2)
    {
        return val1 <= val2 ? val1 : val2;
    }

    [Intrinsic]
    public static long Min(long val1, long val2)
    {
        return val1 <= val2 ? val1 : val2;
    }

    [Intrinsic]
    public static nint Min(nint val1, nint val2)
    {
        return val1 <= val2 ? val1 : val2;
    }

    [Intrinsic]
    public static sbyte Min(sbyte val1, sbyte val2)
    {
        return val1 <= val2 ? val1 : val2;
    }

    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Min(float val1, float val2)
    {
        // This matches the IEEE 754:2019 `minimum` function
        //
        // It propagates NaN inputs back to the caller and
        // otherwise returns the lesser of the inputs. It
        // treats +0 as greater than -0 as per the specification.

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (val1 != val2)
        {
            if (!float.IsNaN(val1))
            {
                return val1 < val2 ? val1 : val2;
            }

            return val1;
        }

        return float.IsNegative(val1) ? val1 : val2;
    }

    [Intrinsic]
    public static ushort Min(ushort val1, ushort val2)
    {
        return val1 <= val2 ? val1 : val2;
    }

    [Intrinsic]
    public static uint Min(uint val1, uint val2)
    {
        return val1 <= val2 ? val1 : val2;
    }

    [Intrinsic]
    public static ulong Min(ulong val1, ulong val2)
    {
        return val1 <= val2 ? val1 : val2;
    }

    [Intrinsic]
    public static nuint Min(nuint val1, nuint val2)
    {
        return val1 <= val2 ? val1 : val2;
    }


    internal static int ConvertToInt32Checked(double value)
    {
        ThrowHelpers.ThrowNotImplementedException();
        return 0;
    }

    internal static uint ConvertToUInt32Checked(double value)
    {
        ThrowHelpers.ThrowNotImplementedException();
        return 0;
    }

    internal static long ConvertToInt64Checked(double value)
    {
        ThrowHelpers.ThrowNotImplementedException();
        return 0;
    }

    internal static ulong ConvertToUInt64Checked(double value)
    {
        ThrowHelpers.ThrowNotImplementedException();
        return 0;
    }

    internal static int DivInt32(int dividend, int divisor)
    {
        if ((uint)(divisor + 1) <= 1)
        {
            switch (divisor)
            {
                case 0:
                    ThrowHelpers.ThrowDivideByZeroException();
                    return 0;
                case -1 when dividend == int.MinValue:
                    ThrowHelpers.ThrowArgumentException();
                    return 0;
                case -1:
                    return -dividend;
            }
        }

        return DivInt32Internal(dividend, divisor);
    }

    internal static uint DivUInt32(uint dividend, uint divisor)
    {
        if (divisor == 0)
        {
            ThrowHelpers.ThrowDivideByZeroException();
            return 0;
        }

        return DivUInt32Internal(dividend, divisor);
    }

    internal static long DivInt64(long dividend, long divisor)
    {
        if ((int)((ulong)divisor >> 32) == (int)((ulong)(int)divisor >> 32))
        {
            switch ((int)divisor)
            {
                case 0:
                    ThrowHelpers.ThrowDivideByZeroException();
                    return 0;
                case -1 when dividend == long.MinValue:
                    ThrowHelpers.ThrowArgumentException();
                    return 0;
                case -1:
                    return -dividend;
            }

            if ((int)((ulong)dividend >> 32) == (int)((ulong)(int)dividend >> 32))
            {
                return DivInt32Internal((int)dividend, (int)divisor);
            }
        }

        return DivInt64Internal(dividend, divisor);
    }

    internal static ulong DivUInt64(ulong dividend, ulong divisor)
    {
        if ((int)(divisor >> 32) == 0)
        {
            if ((uint)divisor == 0)
            {
                ThrowHelpers.ThrowDivideByZeroException();
                return 0;
            }

            if ((int)(dividend >> 32) == 0)
            {
                return DivUInt32Internal((uint)dividend, (uint)divisor);
            }
        }

        return DivUInt64Internal(dividend, divisor);
    }

    internal static int ModInt32(int dividend, int divisor)
    {
        if ((uint)(divisor + 1) <= 1)
        {
            switch (divisor)
            {
                case 0:
                    ThrowHelpers.ThrowDivideByZeroException();
                    return 0;
                case -1:
                {
                    if (dividend == int.MinValue)
                    {
                        ThrowHelpers.ThrowArgumentException();
                    }

                    return 0;
                }
            }
        }

        return ModInt32Internal(dividend, divisor);
    }

    internal static uint ModUInt32(uint dividend, uint divisor)
    {
        if (divisor == 0)
        {
            ThrowHelpers.ThrowDivideByZeroException();
            return 0;
        }

        return ModUInt32Internal(dividend, divisor);
    }

    internal static long ModInt64(long dividend, long divisor)
    {
        if ((int)((ulong)divisor >> 32) == (int)((ulong)(int)divisor >> 32))
        {
            switch ((int)divisor)
            {
                case 0:
                    ThrowHelpers.ThrowDivideByZeroException();
                    return 0;
                case -1 when dividend == long.MinValue:
                    ThrowHelpers.ThrowArgumentException();
                    return 0;
                case -1:
                    return 0;
            }

            if ((int)((ulong)dividend >> 32) == (int)((ulong)(int)dividend >> 32))
            {
                return ModInt32Internal((int)dividend, (int)divisor);
            }
        }

        return ModInt64Internal(dividend, divisor);
    }

    internal static ulong ModUInt64(ulong dividend, ulong divisor)
    {
        if ((int)(divisor >> 32) == 0)
        {
            if ((uint)divisor == 0)
            {
                ThrowHelpers.ThrowDivideByZeroException();
                return 0;
            }

            if ((int)(dividend >> 32) == 0)
            {
                return ModUInt32Internal((uint)dividend, (uint)divisor);
            }
        }

        return ModUInt64Internal(dividend, divisor);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("*", "DivInt32Internal")]
    private static extern int DivInt32Internal(int dividend, int divisor);

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("*", "DivUInt32Internal")]
    private static extern uint DivUInt32Internal(uint dividend, uint divisor);

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("*", "DivInt64Internal")]
    private static extern long DivInt64Internal(long dividend, long divisor);

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("*", "DivUInt64Internal")]
    private static extern ulong DivUInt64Internal(ulong dividend, ulong divisor);

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("*", "ModInt32Internal")]
    private static extern int ModInt32Internal(int dividend, int divisor);

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("*", "ModUInt32Internal")]
    private static extern uint ModUInt32Internal(uint dividend, uint divisor);

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("*", "ModInt64Internal")]
    private static extern long ModInt64Internal(long dividend, long divisor);

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("*", "ModUInt64Internal")]
    private static extern ulong ModUInt64Internal(ulong dividend, ulong divisor);
}