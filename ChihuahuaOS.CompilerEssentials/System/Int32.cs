using Internal.Runtime.CompilerHelpers;

namespace System;

public struct Int32
{
    // ReSharper disable InconsistentNaming
    public const int MaxValue = 0x7fffffff;

    public const int MinValue = unchecked((int)0x80000000);
    // ReSharper restore InconsistentNaming

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