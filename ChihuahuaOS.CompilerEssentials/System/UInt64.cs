using Internal.Runtime.CompilerHelpers;

namespace System;

public struct UInt64
{
    // ReSharper disable InconsistentNaming
    public const ulong MaxValue = 0xffffffffffffffffL;

    public const ulong MinValue = 0x0;
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