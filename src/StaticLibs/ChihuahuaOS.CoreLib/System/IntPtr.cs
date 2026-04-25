using Internal.Runtime.CompilerHelpers;

namespace System;

public unsafe struct IntPtr
{
    private readonly void* _value;

    // ReSharper disable once ConvertToConstant.Global
    public static readonly nint Zero = 0;
    public static nint MaxValue => unchecked((nint)long.MaxValue);
    public static nint MinValue => unchecked((nint)long.MinValue);

    public static int Size => 8;

    public IntPtr(void* value)
    {
        _value = value;
    }

    public static explicit operator nint(void* value)
    {
        return new nint(value);
    }

    public static explicit operator void*(nint value)
    {
        return value._value;
    }

    public static explicit operator nint(int value)
    {
        return new nint((void*)value);
    }

    public static explicit operator nint(long value)
    {
        return new nint((void*)value);
    }

    public static bool operator ==(nint a, nint b)
    {
        return a._value == b._value;
    }

    public static bool operator !=(nint a, nint b)
    {
        return a._value != b._value;
    }

    public static nint Add(nint pointer, int offset)
    {
        return pointer + offset;
    }

    public static nint operator +(nint pointer, int offset)
    {
        return pointer + offset;
    }

    public static nint Subtract(nint pointer, int offset)
    {
        return pointer - offset;
    }

    public static nint operator -(nint pointer, int offset)
    {
        return pointer - offset;
    }

    public static explicit operator int(nint value)
    {
        return checked((int)value);
    }

    public static explicit operator long(nint value)
    {
        return value;
    }


    public static bool TryParse(string s, out long result)
    {
        result = 0;

        bool success = NumberParser.TryParseString(s, out long parsed);
        if (!success)
        {
            return false;
        }

        result = parsed;
        return true;
    }


    public void* ToPointer()
    {
        return _value;
    }

    public bool Equals(nint other)
    {
        return this == other;
    }

    public override string ToString()
    {
        return NumberParser.ParseInteger(this);
    }

    public string ToString(string format)
    {
        if (string.IsNullOrEmpty(format))
        {
            return ToString();
        }

        switch (format)
        {
            case "X":
                return NumberParser.ParseInteger(this, 16);
            case "B":
                return NumberParser.ParseInteger(this, 2);
            default:
                ThrowHelpers.ThrowFormatException();
                return string.Empty;
        }
    }

    public int ToInt32()
    {
        return (int)_value;
    }

    public long ToInt64()
    {
        return (long)_value;
    }
}