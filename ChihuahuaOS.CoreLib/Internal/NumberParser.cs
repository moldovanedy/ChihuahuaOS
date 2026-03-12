using Internal.Runtime.CompilerHelpers;

namespace System;

internal static class NumberParser
{
    private const int MAX_SYMBOLS_BASE_10 = 20;
    private const int MAX_SYMBOLS_BASE_2 = 65;
    private const string DIGITS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private const int MIN_BASE = 2;
    private const int MAX_BASE = 36;

    public static string ParseInteger(long value)
    {
        if (value == 0)
        {
            //this is done to avoid the caller freeing a readonly buffer
            return new string('0', 1);
        }

        bool negative = value < 0;
        char[] digits = new char[MAX_SYMBOLS_BASE_10];
        int len = 0;

        int i = 0;
        while (i < MAX_SYMBOLS_BASE_10 && value != 0)
        {
            digits[MAX_SYMBOLS_BASE_10 - i - 1] = (char)(Math.Abs(value % 10) + '0');
            value /= 10;
            i++;
            len++;
        }

        if (negative)
        {
            digits[MAX_SYMBOLS_BASE_10 - i - 1] = '-';
            i++;
            len++;
        }

        string str = new(digits, MAX_SYMBOLS_BASE_10 - i, len);
        digits.Dispose();
        return str;
    }

    public static string ParseInteger(ulong value)
    {
        if (value == 0)
        {
            //this is done to avoid the caller freeing a readonly buffer
            return new string('0', 1);
        }

        char[] digits = new char[MAX_SYMBOLS_BASE_10];
        int len = 0;

        int i = 0;
        while (i < MAX_SYMBOLS_BASE_10 && value != 0)
        {
            digits[MAX_SYMBOLS_BASE_10 - i - 1] = (char)(value % 10 + '0');
            value /= 10;
            i++;
            len++;
        }

        string str = new(digits, MAX_SYMBOLS_BASE_10 - i, len);
        digits.Dispose();
        return str;
    }

    public static string ParseInteger(long value, uint numBase)
    {
        if (value == 0)
        {
            //this is done to avoid the caller freeing a readonly buffer
            return new string('0', 1);
        }

        if (numBase < MIN_BASE || numBase > MAX_BASE)
        {
            ThrowHelpers.ThrowArgumentException();
        }

        bool negative = value < 0;
        char[] digits = new char[MAX_SYMBOLS_BASE_2];
        int len = 0;

        int i = 0;
        while (i < MAX_SYMBOLS_BASE_2 && value != 0)
        {
            long idx = value % numBase;
            digits[MAX_SYMBOLS_BASE_2 - i - 1] = DIGITS[(int)Math.Abs(idx)];

            value /= numBase;
            i++;
            len++;
        }

        if (negative)
        {
            digits[MAX_SYMBOLS_BASE_2 - i - 1] = '-';
            i++;
            len++;
        }

        string str = new(digits, MAX_SYMBOLS_BASE_2 - i, len);
        digits.Dispose();
        return str;
    }

    public static string ParseInteger(ulong value, uint numBase)
    {
        if (value == 0)
        {
            //this is done to avoid the caller freeing a readonly buffer
            return new string('0', 1);
        }

        if (numBase < MIN_BASE || numBase > MAX_BASE)
        {
            ThrowHelpers.ThrowArgumentException();
        }

        char[] digits = new char[MAX_SYMBOLS_BASE_2];
        int len = 0;

        int i = 0;
        while (i < MAX_SYMBOLS_BASE_2 && value != 0)
        {
            ulong idx = value % numBase;
            digits[MAX_SYMBOLS_BASE_2 - i - 1] = DIGITS[(int)idx];

            value /= numBase;
            i++;
            len++;
        }

        string str = new(digits, MAX_SYMBOLS_BASE_2 - i, len);
        digits.Dispose();
        return str;
    }
}