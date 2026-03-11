using Internal.Runtime.CompilerHelpers;

namespace System;

public struct UInt32
{
    // ReSharper disable InconsistentNaming
    public const uint MaxValue = 0xffffffff;

    public const uint MinValue = 0U;
    // ReSharper restore InconsistentNaming

    public override string ToString()
    {
        return NumberParser.ParseInteger((ulong)this);
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
                return NumberParser.ParseInteger((ulong)this, 16);
            case "B":
                return NumberParser.ParseInteger((ulong)this, 2);
            default:
                ThrowHelpers.ThrowFormatException();
                return string.Empty;
        }
    }
}