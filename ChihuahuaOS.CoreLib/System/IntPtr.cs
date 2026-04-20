using Internal.Runtime.CompilerHelpers;

namespace System;

public unsafe struct IntPtr
{
    private readonly void* _value;

    // ReSharper disable once ConvertToConstant.Global
    public static readonly IntPtr Zero = 0;

    public IntPtr(void* value)
    {
        _value = value;
    }

    public static explicit operator IntPtr(void* value)
    {
        return new IntPtr(value);
    }

    public static explicit operator void*(IntPtr value)
    {
        return value._value;
    }

    public static explicit operator IntPtr(int value)
    {
        return new IntPtr((void*)value);
    }

    public static explicit operator IntPtr(long value)
    {
        return new IntPtr((void*)value);
    }

    public static bool operator ==(IntPtr a, IntPtr b)
    {
        return a._value == b._value;
    }

    public static bool operator !=(IntPtr a, IntPtr b)
    {
        return a._value != b._value;
    }

    public bool Equals(IntPtr other)
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
}