using Internal.Runtime.CompilerHelpers;

namespace System;

public struct Int64
{
    // ReSharper disable InconsistentNaming
    public const long MaxValue = 0x7fffffffffffffffL;

    public const long MinValue = unchecked((long)0x8000000000000000L);
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