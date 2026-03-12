using Internal.Runtime.CompilerHelpers;

namespace System;

public struct Int16
{
    // ReSharper disable InconsistentNaming
    public const short MaxValue = 0x7FFF;

    public const short MinValue = unchecked((short)0x8000);
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