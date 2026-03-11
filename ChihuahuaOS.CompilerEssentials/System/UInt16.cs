using Internal.Runtime.CompilerHelpers;

namespace System;

public struct UInt16
{
    // ReSharper disable InconsistentNaming
    public const ushort MaxValue = 0xFFFF;

    public const ushort MinValue = 0;
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