using Internal.Runtime.CompilerHelpers;

namespace System;

public struct UInt32
{
    public const uint MaxValue = 0xFF_FF_FF_FF;

    public const uint MinValue = 0U;


    public static bool TryParse(string s, out uint result)
    {
        result = 0;

        bool success = NumberParser.TryParseString(s, out ulong parsed);
        if (!success)
        {
            return false;
        }

        //check for overflow
        if (parsed > MaxValue)
        {
            return false;
        }

        result = (uint)parsed;
        return true;
    }


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