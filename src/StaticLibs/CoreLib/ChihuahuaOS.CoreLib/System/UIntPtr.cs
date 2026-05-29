using Internal.Runtime.CompilerHelpers;

namespace System;

public struct UIntPtr
{
    public readonly override string ToString()
    {
        return NumberParser.ParseInteger(this);
    }

    public readonly string ToString(string format)
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
