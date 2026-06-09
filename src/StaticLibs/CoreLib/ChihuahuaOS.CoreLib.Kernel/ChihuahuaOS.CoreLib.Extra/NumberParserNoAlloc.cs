using System;
using Internal.Runtime.CompilerHelpers;

namespace ChihuahuaOS.CoreLib.Extra;

//NOTE: the logic is the same from the CoreLib's NumberParser, just that it uses UTF-8 strings instead of .NET strings

public static unsafe class NumberParserNoAlloc
{
    private static readonly byte* Digits = (byte*)"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"u8;

    public const int MAX_SYMBOLS_BASE_10 = 20;
    public const int MAX_SYMBOLS_BASE_2 = 65;

    private const int MIN_BASE = 2;
    private const int MAX_BASE = 36;

    public static void ParseInteger(long value, Span<byte> buffer)
    {
        if (value == 0)
        {
            buffer[0] = (byte)'0';
            return;
        }

        bool negative = value < 0;
        Span<byte> digits = stackalloc byte[MAX_SYMBOLS_BASE_10];
        digits.Clear();
        int len = 0;

        int i = 0;
        while (i < MAX_SYMBOLS_BASE_10 && value != 0)
        {
            buffer[MAX_SYMBOLS_BASE_10 - i - 1] = (byte)(Math.Abs(value % 10) + '0');
            value /= 10;
            i++;
            len++;
        }

        if (negative)
        {
            buffer[MAX_SYMBOLS_BASE_10 - i - 1] = (byte)'-';
            len++;
        }

        for (int j = 0; j < len; j++)
        {
            buffer[j] = digits[MAX_SYMBOLS_BASE_10 - (len - j)];
        }
    }

    public static void ParseInteger(ulong value, Span<byte> buffer)
    {
        if (value == 0)
        {
            buffer[0] = (byte)'0';
            return;
        }

        Span<byte> digits = stackalloc byte[MAX_SYMBOLS_BASE_10];
        digits.Clear();
        int len = 0;

        int i = 0;
        while (i < MAX_SYMBOLS_BASE_10 && value != 0)
        {
            digits[MAX_SYMBOLS_BASE_10 - i - 1] = (byte)(value % 10 + '0');
            value /= 10;
            i++;
            len++;
        }

        for (int j = 0; j < len; j++)
        {
            buffer[j] = digits[MAX_SYMBOLS_BASE_10 - (len - j)];
        }
    }

    public static void ParseInteger(long value, uint numBase, Span<byte> buffer)
    {
        if (value == 0)
        {
            buffer[0] = (byte)'0';
            return;
        }

        if (numBase < MIN_BASE || numBase > MAX_BASE)
        {
            ThrowHelpers.ThrowArgumentException();
        }

        bool negative = value < 0;
        Span<byte> digits = stackalloc byte[MAX_SYMBOLS_BASE_2];
        digits.Clear();
        int len = 0;

        int i = 0;
        while (i < MAX_SYMBOLS_BASE_2 && value != 0)
        {
            long idx = value % numBase;
            digits[MAX_SYMBOLS_BASE_2 - i - 1] = Digits[(int)Math.Abs(idx)];

            value /= numBase;
            i++;
            len++;
        }

        if (negative)
        {
            digits[MAX_SYMBOLS_BASE_2 - i - 1] = (byte)'-';
            len++;
        }

        for (int j = 0; j < len; j++)
        {
            buffer[j] = digits[MAX_SYMBOLS_BASE_2 - (len - j)];
        }
    }

    public static void ParseInteger(ulong value, uint numBase, Span<byte> buffer)
    {
        if (value == 0)
        {
            buffer[0] = (byte)'0';
            return;
        }

        if (numBase < MIN_BASE || numBase > MAX_BASE)
        {
            ThrowHelpers.ThrowArgumentException();
        }

        Span<byte> digits = stackalloc byte[MAX_SYMBOLS_BASE_2];
        digits.Clear();
        int len = 0;

        int i = 0;
        while (i < MAX_SYMBOLS_BASE_2 && value != 0)
        {
            ulong idx = value % numBase;
            digits[MAX_SYMBOLS_BASE_2 - i - 1] = Digits[(int)idx];

            value /= numBase;
            i++;
            len++;
        }

        for (int j = 0; j < len; j++)
        {
            buffer[j] = digits[MAX_SYMBOLS_BASE_2 - (len - j)];
        }
    }

    public static bool TryParseString(ReadOnlySpan<byte> s, out ulong result)
    {
        result = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] < '0' || s[i] > '9')
            {
                result = 0;
                return false;
            }

            ulong previous = result;
            result = result * 10 + (ulong)(s[i] - 48);

            //overflow!
            if (previous > result)
            {
                result = 0;
                return false;
            }
        }

        return true;
    }

    public static bool TryParseString(ReadOnlySpan<byte> s, out long result)
    {
        bool isNegative = false;
        result = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if (i == 0 && s[i] == '-')
            {
                isNegative = true;
            }

            if (s[i] < '0' || s[i] > '9')
            {
                result = 0;
                return false;
            }

            long previous = result;
            result = result * 10 + (s[i] - 48);

            //overflow!
            if (previous > result)
            {
                result = 0;
                return false;
            }
        }

        if (isNegative)
        {
            result = -result;
        }

        return true;
    }
}
