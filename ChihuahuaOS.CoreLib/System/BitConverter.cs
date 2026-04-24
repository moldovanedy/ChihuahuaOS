using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// Deviation from .NET: most methods no longer check the parameters and also inline the functions for better
/// performance
/// </summary>
public static class BitConverter
{
    //TODO: actually set BIGENDIAN if needed
#if BIGENDIAN
    [Intrinsic]
    public static readonly bool IsLittleEndian = false;
#else
    [Intrinsic] public static readonly bool IsLittleEndian = true;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] GetBytes(bool value)
    {
        return [value ? (byte)1 : (byte)0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteBytes(Span<byte> destination, bool value)
    {
        destination[0] = value ? (byte)1 : (byte)0;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] GetBytes(char value)
    {
        byte[] bytes = new byte[sizeof(char)];
        _ = TryWriteBytes(bytes, value);
        return bytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteBytes(Span<byte> destination, char value)
    {
        if (IsLittleEndian)
        {
            destination[0] = (byte)value;
            destination[1] = (byte)(value >> 8);
        }
        else
        {
            destination[0] = (byte)(value >> 8);
            destination[1] = (byte)value;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] GetBytes(short value)
    {
        byte[] bytes = new byte[sizeof(short)];
        _ = TryWriteBytes(bytes, value);
        return bytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteBytes(Span<byte> destination, short value)
    {
        if (IsLittleEndian)
        {
            destination[0] = (byte)value;
            destination[1] = (byte)(value >> 8);
        }
        else
        {
            destination[0] = (byte)(value >> 8);
            destination[1] = (byte)value;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] GetBytes(ushort value)
    {
        byte[] bytes = new byte[sizeof(ushort)];
        _ = TryWriteBytes(bytes, value);
        return bytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteBytes(Span<byte> destination, ushort value)
    {
        if (IsLittleEndian)
        {
            destination[0] = (byte)value;
            destination[1] = (byte)(value >> 8);
        }
        else
        {
            destination[0] = (byte)(value >> 8);
            destination[1] = (byte)value;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] GetBytes(int value)
    {
        byte[] bytes = new byte[sizeof(int)];
        _ = TryWriteBytes(bytes, value);
        return bytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteBytes(Span<byte> destination, int value)
    {
        if (IsLittleEndian)
        {
            destination[0] = (byte)value;
            destination[1] = (byte)(value >> 8);
            destination[2] = (byte)(value >> 16);
            destination[3] = (byte)(value >> 24);
        }
        else
        {
            destination[0] = (byte)(value >> 24);
            destination[1] = (byte)(value >> 16);
            destination[2] = (byte)(value >> 8);
            destination[3] = (byte)value;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] GetBytes(uint value)
    {
        byte[] bytes = new byte[sizeof(uint)];
        _ = TryWriteBytes(bytes, value);
        return bytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteBytes(Span<byte> destination, uint value)
    {
        if (IsLittleEndian)
        {
            destination[0] = (byte)value;
            destination[1] = (byte)(value >> 8);
            destination[2] = (byte)(value >> 16);
            destination[3] = (byte)(value >> 24);
        }
        else
        {
            destination[0] = (byte)(value >> 24);
            destination[1] = (byte)(value >> 16);
            destination[2] = (byte)(value >> 8);
            destination[3] = (byte)value;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] GetBytes(long value)
    {
        byte[] bytes = new byte[sizeof(long)];
        _ = TryWriteBytes(bytes, value);
        return bytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteBytes(Span<byte> destination, long value)
    {
        if (IsLittleEndian)
        {
            destination[0] = (byte)value;
            destination[1] = (byte)(value >> 8);
            destination[2] = (byte)(value >> 16);
            destination[3] = (byte)(value >> 24);
            destination[4] = (byte)(value >> 32);
            destination[5] = (byte)(value >> 40);
            destination[6] = (byte)(value >> 48);
            destination[7] = (byte)(value >> 56);
        }
        else
        {
            destination[0] = (byte)(value >> 56);
            destination[1] = (byte)(value >> 48);
            destination[2] = (byte)(value >> 40);
            destination[3] = (byte)(value >> 32);
            destination[4] = (byte)(value >> 24);
            destination[5] = (byte)(value >> 16);
            destination[6] = (byte)(value >> 8);
            destination[7] = (byte)value;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] GetBytes(ulong value)
    {
        byte[] bytes = new byte[sizeof(ulong)];
        _ = TryWriteBytes(bytes, value);
        return bytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteBytes(Span<byte> destination, ulong value)
    {
        if (IsLittleEndian)
        {
            destination[0] = (byte)value;
            destination[1] = (byte)(value >> 8);
            destination[2] = (byte)(value >> 16);
            destination[3] = (byte)(value >> 24);
            destination[4] = (byte)(value >> 32);
            destination[5] = (byte)(value >> 40);
            destination[6] = (byte)(value >> 48);
            destination[7] = (byte)(value >> 56);
        }
        else
        {
            destination[0] = (byte)(value >> 56);
            destination[1] = (byte)(value >> 48);
            destination[2] = (byte)(value >> 40);
            destination[3] = (byte)(value >> 32);
            destination[4] = (byte)(value >> 24);
            destination[5] = (byte)(value >> 16);
            destination[6] = (byte)(value >> 8);
            destination[7] = (byte)value;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] GetBytes(float value)
    {
        byte[] bytes = new byte[sizeof(float)];
        _ = TryWriteBytes(bytes, value);
        return bytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryWriteBytes(Span<byte> destination, float value)
    {
        int valAsInt = *(int*)&value;
        return TryWriteBytes(destination, valAsInt);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] GetBytes(double value)
    {
        byte[] bytes = new byte[sizeof(double)];
        _ = TryWriteBytes(bytes, value);
        return bytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryWriteBytes(Span<byte> destination, double value)
    {
        long valAsLong = *(long*)&value;
        return TryWriteBytes(destination, valAsLong);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ToBoolean(byte[] value, int startIndex)
    {
        return value[startIndex] != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ToBoolean(ReadOnlySpan<byte> value)
    {
        return value[0] != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char ToChar(byte[] value, int startIndex)
    {
        return unchecked((char)ToInt16(value, startIndex));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char ToChar(ReadOnlySpan<byte> value)
    {
        return unchecked((char)ToInt16(value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ToInt16(byte[] value, int startIndex)
    {
        if (IsLittleEndian)
        {
            return (short)(value[startIndex] | (value[startIndex + 1] << 8));
        }
        else
        {
            return (short)((value[startIndex] << 8) | value[startIndex + 1]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ToInt16(ReadOnlySpan<byte> value)
    {
        if (IsLittleEndian)
        {
            return (short)(value[0] | (value[1] << 8));
        }
        else
        {
            return (short)((value[0] << 8) | value[1]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ToUInt16(byte[] value, int startIndex)
    {
        if (IsLittleEndian)
        {
            return (ushort)(value[startIndex] | (value[startIndex + 1] << 8));
        }
        else
        {
            return (ushort)((value[startIndex] << 8) | value[startIndex + 1]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ToUInt16(ReadOnlySpan<byte> value)
    {
        if (IsLittleEndian)
        {
            return (ushort)(value[0] | (value[1] << 8));
        }
        else
        {
            return (ushort)((value[0] << 8) | value[1]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt32(byte[] value, int startIndex)
    {
        if (IsLittleEndian)
        {
            return value[startIndex]
                   | (value[startIndex + 1] << 8)
                   | (value[startIndex + 2] << 16)
                   | (value[startIndex + 3] << 24);
        }
        else
        {
            return (value[startIndex] << 24)
                   | (value[startIndex + 1] << 16)
                   | (value[startIndex + 2] << 8)
                   | value[startIndex + 3];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt32(ReadOnlySpan<byte> value)
    {
        if (IsLittleEndian)
        {
            return value[0]
                   | (value[1] << 8)
                   | (value[2] << 16)
                   | (value[3] << 24);
        }
        else
        {
            return (value[0] << 24)
                   | (value[1] << 16)
                   | (value[2] << 8)
                   | value[3];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ToUInt32(byte[] value, int startIndex)
    {
        if (IsLittleEndian)
        {
            return (uint)(
                value[startIndex]
                | (value[startIndex + 1] << 8)
                | (value[startIndex + 2] << 16)
                | (value[startIndex + 3] << 24));
        }
        else
        {
            return (uint)(
                (value[startIndex] << 24)
                | (value[startIndex + 1] << 16)
                | (value[startIndex + 2] << 8)
                | value[startIndex + 3]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ToUInt32(ReadOnlySpan<byte> value)
    {
        if (IsLittleEndian)
        {
            return (uint)(
                value[0]
                | (value[1] << 8)
                | (value[2] << 16)
                | (value[3] << 24));
        }
        else
        {
            return (uint)(
                (value[0] << 24)
                | (value[1] << 16)
                | (value[2] << 8)
                | value[3]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToInt64(byte[] value, int startIndex)
    {
        if (IsLittleEndian)
        {
            return value[startIndex]
                   | ((long)value[startIndex + 1] << 8)
                   | ((long)value[startIndex + 2] << 16)
                   | ((long)value[startIndex + 3] << 24)
                   | ((long)value[startIndex + 4] << 32)
                   | ((long)value[startIndex + 5] << 40)
                   | ((long)value[startIndex + 6] << 48)
                   | ((long)value[startIndex + 7] << 56);
        }
        else
        {
            return ((long)value[startIndex] << 56)
                   | ((long)value[startIndex + 1] << 48)
                   | ((long)value[startIndex + 2] << 40)
                   | ((long)value[startIndex + 3] << 32)
                   | ((long)value[startIndex + 4] << 24)
                   | ((long)value[startIndex + 5] << 16)
                   | ((long)value[startIndex + 6] << 8)
                   | value[startIndex + 7];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToInt64(ReadOnlySpan<byte> value)
    {
        if (IsLittleEndian)
        {
            return value[0]
                   | ((long)value[1] << 8)
                   | ((long)value[2] << 16)
                   | ((long)value[3] << 24)
                   | ((long)value[4] << 32)
                   | ((long)value[5] << 40)
                   | ((long)value[6] << 48)
                   | ((long)value[7] << 56);
        }
        else
        {
            return ((long)value[0] << 56)
                   | ((long)value[1] << 48)
                   | ((long)value[2] << 40)
                   | ((long)value[3] << 32)
                   | ((long)value[4] << 24)
                   | ((long)value[5] << 16)
                   | ((long)value[6] << 8)
                   | value[7];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ToUInt64(byte[] value, int startIndex)
    {
        if (IsLittleEndian)
        {
            return value[startIndex]
                   | ((ulong)value[startIndex + 1] << 8)
                   | ((ulong)value[startIndex + 2] << 16)
                   | ((ulong)value[startIndex + 3] << 24)
                   | ((ulong)value[startIndex + 4] << 32)
                   | ((ulong)value[startIndex + 5] << 40)
                   | ((ulong)value[startIndex + 6] << 48)
                   | ((ulong)value[startIndex + 7] << 56);
        }
        else
        {
            return ((ulong)value[startIndex] << 56)
                   | ((ulong)value[startIndex + 1] << 48)
                   | ((ulong)value[startIndex + 2] << 40)
                   | ((ulong)value[startIndex + 3] << 32)
                   | ((ulong)value[startIndex + 4] << 24)
                   | ((ulong)value[startIndex + 5] << 16)
                   | ((ulong)value[startIndex + 6] << 8)
                   | value[startIndex + 7];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ToUInt64(ReadOnlySpan<byte> value)
    {
        if (IsLittleEndian)
        {
            return value[0]
                   | ((ulong)value[1] << 8)
                   | ((ulong)value[2] << 16)
                   | ((ulong)value[3] << 24)
                   | ((ulong)value[4] << 32)
                   | ((ulong)value[5] << 40)
                   | ((ulong)value[6] << 48)
                   | ((ulong)value[7] << 56);
        }
        else
        {
            return ((ulong)value[0] << 56)
                   | ((ulong)value[1] << 48)
                   | ((ulong)value[2] << 40)
                   | ((ulong)value[3] << 32)
                   | ((ulong)value[4] << 24)
                   | ((ulong)value[5] << 16)
                   | ((ulong)value[6] << 8)
                   | value[7];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float ToSingle(byte[] value, int startIndex)
    {
        int valAsInt = ToInt32(value, startIndex);
        return *(float*)&valAsInt;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float ToSingle(ReadOnlySpan<byte> value)
    {
        int valAsInt = ToInt32(value);
        return *(float*)&valAsInt;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe double ToDouble(byte[] value, int startIndex)
    {
        long valAsLong = ToInt64(value, startIndex);
        return *(double*)&valAsLong;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe double ToDouble(ReadOnlySpan<byte> value)
    {
        long valAsLong = ToInt64(value);
        return *(double*)&valAsLong;
    }


    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long DoubleToInt64Bits(double value)
    {
        return Unsafe.BitCast<double, long>(value);
    }

    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Int64BitsToDouble(long value)
    {
        return Unsafe.BitCast<long, double>(value);
    }

    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong DoubleToUInt64Bits(double value)
    {
        return Unsafe.BitCast<double, ulong>(value);
    }

    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double UInt64BitsToDouble(ulong value)
    {
        return Unsafe.BitCast<ulong, double>(value);
    }

    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SingleToInt32Bits(float value)
    {
        return Unsafe.BitCast<float, int>(value);
    }

    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Int32BitsToSingle(int value)
    {
        return Unsafe.BitCast<int, float>(value);
    }

    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint SingleToUInt32Bits(float value)
    {
        return Unsafe.BitCast<float, uint>(value);
    }

    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float UInt32BitsToSingle(uint value)
    {
        return Unsafe.BitCast<uint, float>(value);
    }
}