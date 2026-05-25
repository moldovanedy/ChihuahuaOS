using Internal.Runtime.CompilerHelpers;

namespace System;

public struct UInt64
{
    public const ulong MaxValue = 0xFF_FF_FF_FF_FF_FF_FF_FFL;

    public const ulong MinValue = 0x0;


    public static bool TryParse(string s, out ulong result)
    {
        result = 0;

        bool success = NumberParser.TryParseString(s, out ulong parsed);
        if (!success)
        {
            return false;
        }

        result = parsed;
        return true;
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